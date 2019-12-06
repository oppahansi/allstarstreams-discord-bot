using System;
using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class AutoMessage
    {
        [Key]
        public ulong Channel { get; set; }
        [Key]
        public string Message { get; set; }
        public int TimeInterval { get; set; }
        public int MessagesInterval { get; set; }
        public DateTime Expiration { get; set; }
    }
}