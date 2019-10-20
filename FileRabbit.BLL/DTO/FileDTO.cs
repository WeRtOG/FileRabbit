using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.BLL.DTO
{
    public class FileDTO
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string FolderId { get; set; }
        public string OwnerId { get; set; }
        public bool IsShared { get; set; }
    }
}
