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

        public override bool Equals(object obj)
        {
            ElementVM elem = obj as ElementVM;
            if (Id == elem.Id && IsFolder == elem.IsFolder && Type == elem.Type && ElemName == elem.ElemName && LastModified == elem.LastModified
                    && IsShared == elem.IsShared)
            {
                if (Size == null && elem.Size == null)
                    return true;
                else if (Size.Item1 == elem.Size.Item1 && Size.Item2 == elem.Size.Item2)
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
