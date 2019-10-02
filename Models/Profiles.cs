﻿using System;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Profiles
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string UrlPhoto { get; set; }
        public sbyte? ProfileAge { get; set; }
        public bool ProfileGender { get; set; }

        public virtual Users User { get; set; }
    }
}
