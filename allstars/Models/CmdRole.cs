using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class CmdRole
    {
        [Key]
        public string Command { get; set; }

        public ulong Guild { get; set; }
        public ulong MinRoleId { get; set; }
    }
}