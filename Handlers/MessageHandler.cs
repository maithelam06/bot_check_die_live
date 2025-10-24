using CheckLiveBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckLiveBot
{
    public class MessageHandler
    {
        private readonly DatabaseService _db;
        private readonly AuthorizationService _auth;

        public MessageHandler(DatabaseService db, AuthorizationService auth)
        {
            _db = db;
            _auth = auth;
        }

        public async Task ProcessAsync(ITelegramBotClient bot, Message msg, CancellationToken ct)
        {
            var chatId = msg.Chat.Id;
            var text = msg.Text ?? "";
            var user = msg.From;

            if (!await _auth.IsUserAuthorizedAsync(user?.Id ?? 0, user?.Username, user?.FirstName, user?.LastName))
            {
                await bot.SendMessage(chatId, "🚫 Hết hạn sử dụng bot. Liên hệ admin.", cancellationToken: ct);
                return;
            }

            switch (text.Split(' ')[0].ToLower())
            {
                case "/start":
                    await CommandHandler.SendWelcomeMenuAsync(bot, chatId, user?.FirstName ?? "User", ct);
                    break;

                case "/stats":
                    await CommandHandler.SendUserStatsAsync(bot, chatId, user?.Id ?? 0, _db, ct);
                    break;

                case "/history":
                    await CommandHandler.SendUserHistoryAsync(bot, chatId, user?.Id ?? 0, _db, ct);
                    break;

                case "/add":
                    await CommandHandler.HandleAddSingleUidAsync(bot, chatId, user?.Id ?? 0, text, _db, ct);
                    break;

                case "/addlist":
                    await bot.SendMessage(chatId, "📋 Gửi danh sách UID mỗi dòng 1 UID | ghi chú | giá", cancellationToken: ct);
                    break;

                case "/list":
                    await CommandHandler.HandleViewUidListAsync(bot, chatId, user?.Id ?? 0, _db, ct);
                    break;

                case "/help":
                    await CommandHandler.SendUidHelpMessageAsync(bot, chatId, ct);
                    break;

                default:
                    await bot.SendMessage(chatId, "❌ Sai lệnh. Dùng /start để xem menu.", cancellationToken: ct);
                    break;
            }
        }
    }

}
