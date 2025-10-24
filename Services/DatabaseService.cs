using Microsoft.EntityFrameworkCore;
using CheckLiveBot.Data;
using CheckLiveBot.Models;

namespace CheckLiveBot.Services
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService()
        {
            _context = new AppDbContext();
        }

        public async Task InitializeDatabaseAsync()
        {
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task<User?> GetUserByTelegramIdAsync(long telegramUserId)
        {
            return await _context.Users
                .Include(u => u.TrackedUids)
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId);
        }

        public async Task<User> CreateOrUpdateUserAsync(long telegramUserId, string? username, int validityDays = 30)
        {
            var user = await GetUserByTelegramIdAsync(telegramUserId);

            if (user == null)
            {
                user = new User
                {
                    TelegramUserId = telegramUserId,
                    TelegramUsername = username,
                    ExpiryDate = DateTime.UtcNow.AddDays(validityDays)
                };
                _context.Users.Add(user);
            }
            else
            {
                user.TelegramUsername = username;
            }

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> IsUserActiveAsync(long telegramUserId)
        {
            var user = await GetUserByTelegramIdAsync(telegramUserId);
            return user != null && user.ExpiryDate > DateTime.UtcNow;
        }

        public async Task SaveTrackedUidAsync(long telegramUserId, string uid, string note, string price)
        {
            var user = await GetUserByTelegramIdAsync(telegramUserId);
            if (user == null) throw new Exception("User not found");

            var tracked = new TrackedUid
            {
                UserId = user.Id,
                Uid = uid,
                Note = note,
                Price = price
            };
            _context.TrackedUids.Add(tracked);
            await _context.SaveChangesAsync();
        }

        public async Task SaveOrUpdateTrackedUidAsync(long telegramUserId, string uid, string note, string price, bool isLive)
        {
            var user = await GetUserByTelegramIdAsync(telegramUserId);
            if (user == null) throw new Exception("User not found");

            var tracked = await _context.TrackedUids
                .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Uid == uid);

            if (tracked == null)
            {
                tracked = new TrackedUid
                {
                    UserId = user.Id,
                    Uid = uid,
                    Note = note,
                    Price = price,
                    IsLive = isLive,
                    LastChecked = DateTime.UtcNow
                };
                _context.TrackedUids.Add(tracked);
            }
            else
            {
                tracked.Note = note;
                tracked.Price = price;
                tracked.IsLive = isLive;
                tracked.LastChecked = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<TrackedUid>> GetTrackedUidsAsync(long telegramUserId)
        {
            var user = await GetUserByTelegramIdAsync(telegramUserId);
            if (user == null) return new List<TrackedUid>();

            return await _context.TrackedUids
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.LastChecked)
                .ToListAsync();
        }

    }
}