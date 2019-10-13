using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.ViewModels
{
    public class ElementViewModel
    {
        public enum FileType { Other, Document, Image, Audio, Video, Folder }

        public enum Unit { B, KB, MB, GB, TB }

        public FileType Type { get; set; }

        public string ElemName { get; set; }

        public string LastModified { get; set; }

        public Tuple<double, Unit> Size { get; set; }

    }
}
