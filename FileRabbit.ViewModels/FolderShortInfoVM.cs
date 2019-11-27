using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.ViewModels
{
    public class FolderShortInfoVM
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            FolderShortInfoVM info = obj as FolderShortInfoVM;
            if (Id == info.Id && Name == info.Name)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
