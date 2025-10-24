using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CheckLiveBot.Services;
using System.Net; // 👈 thêm để fake web server

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

        // ⚠️ KHÔNG hardcode token nữa, dùng biến môi trường để bảo mật
        private static readonly string BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

        static async Task Main(string[] args)
        {
            // Fake web server để Render nghĩ app là web service -> không bị kill ✅
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/");
            listener.Start();
            Console.WriteLine("✅ Fake web server is running on port 8080...");

            _databaseService = new DatabaseService();
            await _databaseService.InitializeDatabaseAsync();

            _authService = new AuthorizationService(_databaseService);
            _messageHandler = new MessageHandler(_databaseService, _authService);
            _callbackHandler = new CallbackQueryHandler(_databaseService, _authService);

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

            await Task.Delay(-1); // 👈 giữ bot chạy mãi
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
