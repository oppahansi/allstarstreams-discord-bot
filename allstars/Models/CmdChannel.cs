using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class CmdChannel
    {
        [Key]
        public string Command { get; set; }

        public ulong Guild { get; set; }

        [Key]
        public ulong ChannelId { get; set; }
    }
}