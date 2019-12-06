using System;
using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class CmdUserCd
    {
        [Key]
        public string Command { get; set; }

        public ulong Guild { get; set; }

        [Key]
        public ulong UserId { get; set; }

        public DateTime Expires { get; set; }
    }
}