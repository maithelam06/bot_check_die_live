using CheckLiveBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLiveBot
{
    public class AuthorizationService
    {
        private readonly DatabaseService _db;

        public AuthorizationService(DatabaseService db) => _db = db;

        public async Task<bool> IsUserAuthorizedAsync(long telegramUserId, string? username, string? firstName, string? lastName)
        {
            if (telegramUserId == 0) return false;

            var user = await _db.GetUserByTelegramIdAsync(telegramUserId);
            if (user == null)
            {
                await _db.CreateOrUpdateUserAsync(telegramUserId, username, 5); // trial 5 ngày
                return true;
            }
            return await _db.IsUserActiveAsync(telegramUserId);
        }
    }

}
