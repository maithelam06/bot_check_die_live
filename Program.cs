using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CheckLiveBot.Services;
using System.Net;

namespace CheckLiveBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;
        private static CancellationTokenSource _cts;
        private static DatabaseService _databaseService;
        private static AuthorizationService _authService;
        private static MessageHandler _messageHandler;
        private static CallbackQueryHandler _callbackHandler;

        private static readonly string BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

        static async Task Main(string[] args)
        {
            // ✅ Fake web server trả về HTTP để Render không timeout
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();
            Console.WriteLine($"✅ Fake web server is running on port {port}...");

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var context = await listener.GetContextAsync();
                    var response = context.Response;
                    var buffer = System.Text.Encoding.UTF8.GetBytes("✅ Bot is running...");
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
            });

            // ✅ Khởi tạo database và service
            _databaseService = new DatabaseService();
            await _databaseService.InitializeDatabaseAsync();

            _authService = new AuthorizationService(_databaseService);
            _messageHandler = new MessageHandler(_databaseService, _authService);
            _callbackHandler = new CallbackQueryHandler(_databaseService, _authService);

            // ✅ Khởi tạo bot Telegram
            _botClient = new TelegramBotClient(BotToken);
            _cts = new CancellationTokenSource();

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandlePollingErrorAsync,
                new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
                _cts.Token
            );

            var me = await _botClient.GetMe();
            Console.WriteLine($"🤖 Bot @{me.Username} đã khởi động thành công.");

            // ✅ Chạy background checker sau khi bot đã sẵn sàng
            var uidChecker = new UidCheckerBackground(_botClient, _databaseService, _cts.Token);
            _ = uidChecker.StartAsync();

            await Task.Delay(-1); // giữ bot chạy mãi
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                    await _messageHandler.ProcessAsync(bot, update.Message, ct);
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                    await _callbackHandler.ProcessAsync(bot, update.CallbackQuery, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error handling update: {ex.Message}");
            }
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"⚠️ Polling error: {ex.Message}");
            return Task.CompletedTask;
        }
    }
}
