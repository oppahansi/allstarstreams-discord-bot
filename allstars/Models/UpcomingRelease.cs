using System;
using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class UpcomingRelease
    {
        [Key]
        public string Title { get; set; }

        public string TitleExtended { get; set; }
        public string ImageUrl { get; set; }
        public DateTime DvdReleaseDate { get; set; }
        public DateTime BluRayReleaseDate { get; set; }
        public DateTime UhdReleaseDate { get; set; }
        public bool DvdReleasePosted { get; set; }
        public bool BluRayPosted { get; set; }
        public bool UhdPosted { get; set; }
    }
}