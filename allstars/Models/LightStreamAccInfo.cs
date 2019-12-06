using System;

namespace allstars.Models
{
    public class LightStreamAccInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime Expiration { get; set; }
        public string Status { get; set; }
        public string Owner { get; set; }
        public string MaxConnections { get; set; }
        public string Notes { get; set; }
    }
}