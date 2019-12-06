using System;
using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class Mute
    {
        [Key]
        public ulong UserId { get; set; }

        public ulong GuildId { get; set; }

        [Key]
        public DateTime Expires { get; set; }

        public bool Active { get; set; }
        public string Reason { get; set; }
        public ulong MutedBy { get; set; }
    }
}