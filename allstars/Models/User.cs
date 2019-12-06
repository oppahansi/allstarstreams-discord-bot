using System;
using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }

        public ulong Guild { get; set; }
        public int Discriminator { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime Joined { get; set; }
        public DateTime Left { get; set; }
        public DateTime MuteExpires { get; set; }
    }
}