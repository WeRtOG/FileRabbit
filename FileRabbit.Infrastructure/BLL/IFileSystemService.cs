using FileRabbit.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.BLL.Interfaces
{
    public interface IFileSystemService
    {
        Task<ICollection<ElementVM>> UploadFiles(IFormFileCollection files, FolderVM parentFolder);
        ElementVM CreateFolder(FolderVM parentFolder, string name, string ownerId);
        void CreateFolder(string ownerId);
        FolderVM GetFolderById(string id);
        FileVM GetFileById(string id);
        ICollection<ElementVM> GetElementsFromFolder(FolderVM folder);
        Stack<FolderShortInfoVM> GetFolderPath(string currFolderId);
        bool CheckAccess(FolderVM folder, string currentId);
        bool CheckAccess(FileVM file, string currentId);
    }
}
