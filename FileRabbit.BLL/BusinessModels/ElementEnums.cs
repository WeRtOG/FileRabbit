using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.BLL.BusinessModels
{
    public static class ElementEnums
    {
        public enum FileType { Other, Document, Image, Audio, Video, Folder }

        public enum Unit { B, KB, MB, GB, TB }
    }
}
