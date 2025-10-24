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
    public class CallbackQueryHandler
    {
        private readonly DatabaseService _db;
        private readonly AuthorizationService _auth;

        public CallbackQueryHandler(DatabaseService db, AuthorizationService auth)
        {
            _db = db;
            _auth = auth;
        }

        public async Task ProcessAsync(ITelegramBotClient bot, CallbackQuery query, CancellationToken ct)
        {
            var chatId = query.Message!.Chat.Id;
            var userId = query.From.Id;

            await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);

            switch (query.Data)
            {
                case "add_uid":
                    await bot.SendMessage(chatId, "📝 Nhập: /add UID | ghi chú | giá", cancellationToken: ct);
                    break;

                case "list_uids":
                    await CommandHandler.HandleViewUidListAsync(bot, chatId, userId, _db, ct);
                    break;

                case "stats":
                    await CommandHandler.SendUserStatsAsync(bot, chatId, userId, _db, ct);
                    break;

                case "help":
                    await CommandHandler.SendHelpMessageAsync(bot, chatId, ct);
                    break;

                case "back_to_menu":
                    await CommandHandler.SendWelcomeMenuAsync(bot, chatId, query.From.FirstName ?? "User", ct);
                    break;
            }
        }
    }

}
