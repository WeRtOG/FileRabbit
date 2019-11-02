using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.ViewModels
{
    public class FileVM
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string FolderId { get; set; }
        public string OwnerId { get; set; }
        public bool IsShared { get; set; }
    }
}
