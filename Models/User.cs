using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLiveBot.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long TelegramUserId { get; set; }

        public string? TelegramUsername { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public ICollection<TrackedUid> TrackedUids { get; set; } = new List<TrackedUid>();
    }
}
