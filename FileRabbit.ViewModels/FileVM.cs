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
        public string ContentType { get; set; }
        public string FolderId { get; set; }
        public string OwnerId { get; set; }
        public bool IsShared { get; set; }

        public override bool Equals(object obj)
        {
            FileVM file = obj as FileVM;
            if (Id == file.Id && Path == file.Path && FolderId == file.FolderId && OwnerId == file.OwnerId 
                && IsShared == file.IsShared && Name == file.Name && ContentType == file.ContentType)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
