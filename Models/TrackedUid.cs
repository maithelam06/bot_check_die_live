using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLiveBot.Models
{
    public class TrackedUid
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Uid { get; set; } = string.Empty;

        public string? Note { get; set; }
        public string? Price { get; set; }

        [Required]
        public bool IsLive { get; set; }

        public DateTime LastChecked { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }

}
