using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.ViewModels
{
    public class WatchPageVM
    {
        public List<ElementVM> Elements { get; set; }

        public Stack<FolderShortInfoVM> FolderPath { get; set; }

        public string CurrFolderId { get; set; }
    }
}
