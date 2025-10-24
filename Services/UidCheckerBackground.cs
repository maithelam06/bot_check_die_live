using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using CheckLiveBot.Services;

namespace CheckLiveBot.Services
{
    public class UidCheckerBackground
    {
        private readonly ITelegramBotClient _bot;
        private readonly DatabaseService _db;
        private readonly CancellationToken _ct;

        public UidCheckerBackground(ITelegramBotClient bot, DatabaseService db, CancellationToken ct)
        {
            _bot = bot;
            _db = db;
            _ct = ct;
        }

        public async Task StartAsync()
        {
            Console.WriteLine("🛰 UID background checker started...");

            while (!_ct.IsCancellationRequested)
            {
                try
                {
                    var allTracked = await _db.GetAllTrackedUidsAsync();

                    foreach (var item in allTracked)
                    {
                        try
                        {
                            var status = await CheckLiveUid.CheckLiveAsync(item.Uid);
                            var isLive = status == "live";

                            // 📌 Chỉ gửi khi trạng thái thay đổi
                            if (item.IsLive != isLive)
                            {
                                string message = isLive
                                    ? $"✅ UID <code>{item.Uid}</code> ({item.Note}) đã LIVE trở lại 🚀"
                                    : $"❌ UID <code>{item.Uid}</code> ({item.Note}) vừa DIE ☠️";

                                await _bot.SendMessage(
                                    item.User.TelegramUserId,
                                    message,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: _ct
                                );

                                await _db.UpdateUidStatusAsync(item.Id, isLive);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Lỗi khi kiểm tra UID {item.Uid}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BackgroundChecker] ❌ Error: {ex.Message}");
                }

                // 🕒 Kiểm tra lại sau 10 phút (có thể chỉnh nhỏ hơn nếu muốn nhanh hơn)
                await Task.Delay(TimeSpan.FromMinutes(10), _ct);
            }
        }
    }
}
