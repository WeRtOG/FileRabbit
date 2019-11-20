using FileRabbit.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
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
        ICollection<ElementVM> GetElementsFromFolder(FolderVM folder, string userId);
        Stack<FolderShortInfoVM> GetFolderPath(string currFolderId, string userId);
        MemoryStream CreateArchive(string currFolderId, string userId, string[] foldersId, string[] filesId);
        bool RemoveFilesAndFolders(string userId, string[] foldersId, string[] filesId);
        bool RenameFolder(string newName, string folderId);
        bool RenameFile(string newName, string fileId);
        string ChangeAccess(string currFolderId, string userId, string[] foldersId, string[] filesId, bool openAccess);
        bool HasSharedChildren(string parentFolderId);
        bool CheckAccessToView(FolderVM folder, string currentId);
        bool CheckAccessToView(FileVM file, string currentId);
        bool CheckEditAccess(FolderVM folder, string currentId);
        bool CheckEditAccess(FileVM file, string currentId);
    }
}
