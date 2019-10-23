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
using FileRabbit.ViewModels;

namespace FileRabbit.BLL.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly IUnitOfWork _database;
        private readonly IMapper _mapper;
        private const string pathRoot = "C://FileRabbitStore";

        public FileSystemService(IUnitOfWork unit, IMapper mapper)
        {
            _database = unit;
            _mapper = mapper;
        }

        public ICollection<ElementVM> GetElementsFromFolder(FolderVM folder)
        {
            DirectoryInfo dir = new DirectoryInfo(folder.Path);
            FileInfo[] files;
            DirectoryInfo[] dirs;

            files = dir.GetFiles();
            dirs = dir.GetDirectories();

            List<ElementVM> models = new List<ElementVM>();

            foreach (var elem in dirs)
            {
                ElementVM model = new ElementVM
                {
                    Type = ElementVM.FileType.Folder,
                    ElemName = elem.Name,
                    LastModified = elem.LastWriteTime.ToShortDateString(),
                    Size = null
                };
                models.Add(model);
            }

            foreach (var elem in files)
            {
                // для более удобного отображения размера файла, вызовем функцию преобразования
                Tuple<double, ElementVM.Unit> size = new Tuple<double, ElementVM.Unit>(elem.Length, ElementVM.Unit.B);
                size = ElementHelperClass.Recount(size);
                ElementVM.FileType type = ElementHelperClass.DefineFileType(elem.Extension);

                ElementVM model = new ElementVM
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

        public FolderVM GetFolderById(string id)
        {
            FolderVM folder = _mapper.Map<Folder, FolderVM>(_database.Folders.Get(id));
            return folder;
        }

        public string CreateFolder(FolderVM parentFolder, string name, string ownerId)
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
                _database.Folders.Create(newFolder);
                _database.Save();
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
            _database.Folders.Create(newFolder);
            _database.Save();
        }

        public async Task UploadFiles(IFormFileCollection files, FolderVM parentFolder, string ownerId)
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
                    _database.Files.Create(file);
                }
            }
            _database.Save();
        }

        public bool CheckAccess(FolderVM folder, string currentId)
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
