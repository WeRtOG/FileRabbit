using FileRabbit.BLL.DTO;
using FileRabbit.BLL.Interfaces;
using FileRabbit.DAL.Interfaces;
using System;
using System.Collections.Generic;
using AutoMapper;
using FileRabbit.DAL.Entites;
using System.IO;
using FileRabbit.BLL.BusinessModels;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FileRabbit.BLL.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly IUnitOfWork database;
        private readonly IMapper _mapper;
        private const string pathRoot = "C://FileRabbitStore";

        public FileSystemService(IUnitOfWork unit, IMapper mapper)
        {
            database = unit;
            _mapper = mapper;
        }

        public ICollection<ElementDTO> GetElementsFromFolder(FolderDTO folder)
        {
            DirectoryInfo dir = new DirectoryInfo(folder.Path);
            FileInfo[] files;
            DirectoryInfo[] dirs;

            files = dir.GetFiles();
            dirs = dir.GetDirectories();

            List<ElementDTO> models = new List<ElementDTO>();

            foreach (var elem in dirs)
            {
                ElementDTO model = new ElementDTO
                {
                    Type = ElementEnums.FileType.Folder,
                    ElemName = elem.Name,
                    LastModified = elem.LastWriteTime.ToShortDateString(),
                    Size = null
                };
                models.Add(model);
            }

            foreach (var elem in files)
            {
                // для более удобного отображения размера файла, вызовем функцию преобразования
                Tuple<double, ElementEnums.Unit> size = new Tuple<double, ElementEnums.Unit>(elem.Length, ElementEnums.Unit.B);
                size = ElementHelperClass.Recount(size);
                ElementEnums.FileType type = ElementHelperClass.DefineFileType(elem.Extension);

                ElementDTO model = new ElementDTO
                {
                    Type = type,
                    ElemName = elem.Name,
                    LastModified = elem.LastWriteTime.ToShortDateString(),
                    Size = size
                };
                models.Add(model);
            }

            return models;
        }

        public FolderDTO GetFolderById(string id)
        {
            FolderDTO folder = _mapper.Map<Folder, FolderDTO>(database.Folders.Get(id));
            return folder;
        }

        public string CreateFolder(FolderDTO parentFolder, string name, string ownerId)
        {
            string newFolderPath = parentFolder.Path + "//" + name;
            if (!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);
                Folder newFolder = new Folder
                {
                    Id = Guid.NewGuid().ToString(),
                    Path = newFolderPath,
                    IsShared = parentFolder.IsShared,
                    OwnerId = ownerId,
                    ParentFolderId = parentFolder.Id
                };
                database.Folders.Create(newFolder);
                database.Save();
                return newFolder.Id;
            }
            return null;
        }

        public void CreateFolder(string ownerId)
        {
            // создаём папку нового пользователя в хранилище
            Directory.CreateDirectory(pathRoot + "//" + ownerId);
            Folder newFolder = new Folder
            {
                Id = ownerId,
                Path = pathRoot + "//" + ownerId,
                IsShared = false,
                OwnerId = ownerId,
                ParentFolderId = null
            };
            database.Folders.Create(newFolder);
            database.Save();
        }

        public async Task UploadFiles(IFormFileCollection files, FolderDTO parentFolder, string ownerId)
        {
            foreach (var uploadedFile in files)
            {
                string path = parentFolder.Path + "//" + uploadedFile.FileName;

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                if (!System.IO.File.Exists(path))
                {
                    DAL.Entites.File file = new DAL.Entites.File
                    {
                        Id = Guid.NewGuid().ToString(),
                        Path = path,
                        IsShared = false,
                        OwnerId = ownerId,
                        FolderId = parentFolder.Id
                    };
                    database.Files.Create(file);
                }
            }
            database.Save();
        }

        public bool CheckAccess(FolderDTO folder, string currentId)
        {
            if (folder.IsShared)
                return true;
            else
            {
                if (folder.OwnerId == currentId)
                    return true;
                return false;
            }   
        }
    }
}
