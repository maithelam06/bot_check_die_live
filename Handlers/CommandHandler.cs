using CheckLiveBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckLiveBot
{
    public static class CommandHandler
    {
        public static async Task SendWelcomeMenuAsync(ITelegramBotClient bot, long chatId, string userName, CancellationToken ct)
        {
            var welcomeText =
                $"🎯 <b>Chào mừng {System.Net.WebUtility.HtmlEncode(userName)}!</b>\n\n" +
                "Bot này giúp bạn <b>theo dõi trạng thái UID Facebook</b>.\n" +
                "Bạn có thể thêm UID và xem trạng thái LIVE/DEAD được cập nhật tự động.\n\n" +
                "📱 <b>Tính năng:</b>\n" +
                "• Thêm UID vào danh sách theo dõi\n" +
                "• Xem danh sách UID đang theo dõi\n" +
                "• Xem thống kê LIVE/DEAD\n" +
                "• Xem hướng dẫn sử dụng\n\n" +
                "Hãy chọn một tùy chọn bên dưới để bắt đầu:";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("➕ Thêm UID", "add_uid"),
                InlineKeyboardButton.WithCallbackData("📋 Danh sách UID", "list_uids"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 Thống kê", "stats"),
                InlineKeyboardButton.WithCallbackData("❓ Hướng dẫn", "help"),
            }
        });

            await bot.SendMessage(chatId, welcomeText, parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
        }

        public static async Task SendUidHelpMessageAsync(ITelegramBotClient bot, long chatId, CancellationToken ct)
        {
            var text = "📖 <b>Hướng dẫn quản lý UID</b>\n\n" +
                       "• /add <code>UID ghi chú | giá</code> - Thêm UID đơn lẻ\n" +
                       "  Ví dụ: /add 100012345678901 VIP khách | 50000\n\n" +
                       "• /addlist - Thêm danh sách UID (mỗi dòng 1 UID | ghi chú | giá)\n" +
                       "• /list - Xem danh sách UID đang theo dõi\n" +
                       "• /help - Xem hướng dẫn này\n";

            await bot.SendMessage(chatId, text, parseMode: ParseMode.Html, cancellationToken: ct);
        }

        public static async Task HandleAddSingleUidAsync(ITelegramBotClient bot, long chatId, long telegramUserId, string messageText, DatabaseService db, CancellationToken ct)
        {
            try
            {
                var content = messageText.Substring(5).Trim();
                var parts = content.Split('|', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .ToArray();

                if (parts.Length < 2)
                {
                    await bot.SendMessage(chatId,
                        "❌ Sai định dạng. Ví dụ:\n<code>/add 100012345678901 VIP khách | 50000</code>",
                        parseMode: ParseMode.Html, cancellationToken: ct);
                    return;
                }

                var firstPart = parts[0];
                var firstSpaceIndex = firstPart.IndexOf(' ');

                if (firstSpaceIndex == -1)
                {
                    await bot.SendMessage(chatId,
                        "❌ Thiếu ghi chú. Ví dụ:\n<code>/add 100012345678901 VIP khách | 50000</code>",
                        parseMode: ParseMode.Html, cancellationToken: ct);
                    return;
                }

                var uidPart = firstPart[..firstSpaceIndex].Trim();
                var notePart = firstPart[(firstSpaceIndex + 1)..].Trim();
                var pricePart = parts[1];

                var isLive = await CheckLiveUid.CheckLiveAsync(uidPart) == "live";

                await db.SaveOrUpdateTrackedUidAsync(telegramUserId, uidPart, notePart, pricePart, isLive);

                await bot.SendMessage(chatId,
                    $"✅ Đã thêm UID <code>{uidPart}</code> với ghi chú \"{notePart}\" và giá {pricePart}",
                    parseMode: ParseMode.Html, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleAddSingleUidAsync: {ex.Message}");
                await bot.SendMessage(chatId, "❌ Lỗi khi thêm UID.", cancellationToken: ct);
            }
        }

        public static async Task HandleViewUidListAsync(ITelegramBotClient bot, long chatId, long telegramUserId, DatabaseService db, CancellationToken ct)
        {
            var list = await db.GetTrackedUidsAsync(telegramUserId);

            if (list.Count == 0)
            {
                await bot.SendMessage(chatId, "📭 Chưa có UID nào được theo dõi.", cancellationToken: ct);
                return;
            }

            var text = "📋 <b>Danh sách UID đang theo dõi</b>\n\n";
            int i = 1;
            foreach (var item in list)
            {
                var status = item.IsLive ? "✅ LIVE" : "❌ DEAD";
                text += $"{i++}. 🆔 <code>{item.Uid}</code> - {status}\n   📝 {item.Note} | 💰 {item.Price} | ⏱ {item.LastChecked:MM/dd HH:mm}\n";
            }

            await bot.SendMessage(chatId, text, parseMode: ParseMode.Html, cancellationToken: ct);
        }

        public static async Task SendHelpMessageAsync(ITelegramBotClient bot, long chatId, CancellationToken ct)
        {
            var helpText =
                "📖 <b>Hướng dẫn sử dụng bot theo dõi UID</b>\n\n" +
                "➕ <b>Thêm UID:</b>\n" +
                "• Lệnh: <code>/add UID | ghi chú | giá</code>\n" +
                "• Ví dụ: <code>/add 100012345678901 VIP khách | 50000</code>\n\n" +
                "📋 <b>Danh sách UID:</b>\n" +
                "• Lệnh: <code>/list</code>\n" +
                "• Xem tất cả UID đang theo dõi và trạng thái LIVE/DEAD mới nhất\n\n" +
                "📊 <b>Thống kê:</b>\n" +
                "• Lệnh: <code>/stats</code>\n" +
                "• Xem tổng UID, số LIVE, số DEAD và ngày hết hạn sử dụng bot\n\n" +
                "💡 <b>Lưu ý:</b>\n" +
                "• UID phải là số, dài 10–20 ký tự\n" +
                "• Trạng thái được cập nhật tự động theo lịch\n" +
                "• Bạn có thể recheck thủ công bằng lệnh <code>/recheck</code>\n";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[] { InlineKeyboardButton.WithCallbackData("🔙 Quay lại Menu", "back_to_menu") }
        });

            await bot.SendMessage(chatId, helpText, parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
        }

        public static async Task SendUserStatsAsync(ITelegramBotClient bot, long chatId, long telegramUserId, DatabaseService db, CancellationToken ct)
        {
            var user = await db.GetUserByTelegramIdAsync(telegramUserId);
            var tracked = await db.GetTrackedUidsAsync(telegramUserId);

            var expiryInfo = user != null
                ? $"📅 <b>Hết hạn:</b> {user.ExpiryDate:yyyy-MM-dd HH:mm} UTC\n" +
                  $"⏰ <b>Còn lại:</b> {Math.Max(0, (user.ExpiryDate - DateTime.UtcNow).Days)} ngày\n\n"
                : "";

            var statsText =
                "📊 <b>Thống kê của bạn</b>\n\n" +
                expiryInfo +
                $"🔢 <b>Tổng UID đang theo dõi:</b> {tracked.Count}";

            await bot.SendMessage(chatId, statsText, parseMode: ParseMode.Html, cancellationToken: ct);
        }

        public static async Task SendUserHistoryAsync(ITelegramBotClient bot, long chatId, long telegramUserId, DatabaseService db, CancellationToken ct)
        {
            var history = await db.GetTrackedUidsAsync(telegramUserId);
            var historyText = "📜 <b>Lịch sử kiểm tra gần đây</b> (10 lần gần nhất)\n\n";

            if (history.Count == 0)
            {
                historyText += "Chưa có dữ liệu. Hãy bắt đầu kiểm tra Facebook ID để thấy lịch sử tại đây!";
            }
            else
            {
                foreach (var check in history.Take(10))
                {
                    var status = check.IsLive ? "✅ LIVE" : "❌ DEAD";
                    var date = check.LastChecked.ToString("MM/dd HH:mm");
                    historyText += $"🆔 <code>{System.Net.WebUtility.HtmlEncode(check.Uid)}</code> - {status} - {date}\n";
                }
            }

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[] { InlineKeyboardButton.WithCallbackData("🔙 Quay lại Menu", "back_to_menu") }
        });

            await bot.SendMessage(chatId, historyText, parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
        }
    }
}
