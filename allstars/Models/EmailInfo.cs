﻿using System;
using System.Collections.Generic;
using System.Text;

namespace allstars.Models
{
    public class EmailInfo
    {
        public string Username { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
