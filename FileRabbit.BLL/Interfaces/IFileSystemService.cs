using FileRabbit.BLL.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.BLL.Interfaces
{
    public interface IFileSystemService
    {
        Task UploadFiles(IFormFileCollection files, FolderDTO parentFolder, string ownerId);
        string CreateFolder(FolderDTO parentFolder, string name, string ownerId);
        void CreateFolder(string ownerId);
        FolderDTO GetFolderById(string id);
        ICollection<ElementDTO> GetElementsFromFolder(FolderDTO folder);
        bool CheckAccess(FolderDTO folder, string currentId);
    }
}
