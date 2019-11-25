using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.ViewModels
{
    public class ElementVM
    {
        public enum FileType { Other, Document, Image, Audio, Video, Folder }

        public enum Unit { B, KB, MB, GB, TB }

        public string Id { get; set; }

        public bool IsFolder { get; set; }

        public FileType Type { get; set; }

        public string ElemName { get; set; }

        public string LastModified { get; set; }

        public Tuple<double, Unit> Size { get; set; }

        public bool IsShared { get; set; }
    }
}
