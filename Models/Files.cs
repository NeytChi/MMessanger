using System;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Files
    {
        public long FileId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileExtension { get; set; }
        public string FileLastName { get; set; }
        public string FileFullpath { get; set; }
    }
}
