using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class QuickHelp
    {
        [Key]
        public string Tag { get; set; }

        public string Help { get; set; }
    }
}