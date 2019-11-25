using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.ViewModels
{
    public class FolderVM
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string ParentFolderId { get; set; }
        public string OwnerId { get; set; }
        public bool IsShared { get; set; }

        public override bool Equals(object obj)
        {
            FolderVM folder = obj as FolderVM;
            if (Id == folder.Id && Path == folder.Path && ParentFolderId == folder.ParentFolderId
                && OwnerId == folder.OwnerId && IsShared == folder.IsShared)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
