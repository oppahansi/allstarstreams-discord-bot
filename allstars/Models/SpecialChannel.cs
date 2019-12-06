using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class SpecialChannel
    {
        [Key]
        public string Type { get; set; }

        public ulong Id { get; set; }
        public ulong Guild { get; set; }
    }
}