using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.BLL.DTO
{
    public class FolderDTO
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string ParentFolderId { get; set; }
        public string OwnerId { get; set; }
        public bool IsShared { get; set; }
    }
}
