using FileRabbit.BLL.Interfaces;
using System;
using System.Collections.Generic;
using AutoMapper;
using System.IO;
using FileRabbit.BLL.BusinessModels;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FileRabbit.ViewModels;
using FileRabbit.DAL.Entities;
using FileRabbit.Infrastructure.DAL;
using System.Linq;

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

        // this method returns all folders and files that are contained in the needed folder
        public ICollection<ElementVM> GetElementsFromFolder(FolderVM folderVM)
        {
            DirectoryInfo dir = new DirectoryInfo(folderVM.Path);
            FileInfo[] files;
            DirectoryInfo[] dirs;

            files = dir.GetFiles();
            dirs = dir.GetDirectories();

            List<ElementVM> models = new List<ElementVM>();

            List<Folder> childFolders = _database.Folders.Find(f => f.ParentFolderId == folderVM.Id).ToList();
            List<DAL.Entities.File> childFiles = _database.Files.Find(f => f.FolderId == folderVM.Id).ToList();

            foreach (var elem in dirs)
            {
                string s = childFolders[0].Path;
                ElementVM model = new ElementVM
                {
                    Id = childFolders.Find(f => f.Path == elem.FullName.Replace("\\", "//")).Id,
                    IsFolder = true,
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
                    Id = childFiles.Find(f => f.Path == elem.FullName.Replace("\\", "//")).Id,
                    IsFolder = false,
                    Type = type,
                    ElemName = elem.Name,
                    LastModified = elem.LastWriteTime.ToShortDateString(),
                    Size = size
                };
                models.Add(model);
            }

            return models;
        }

        // this method returns folder by id
        public FolderVM GetFolderById(string id)
        {
            FolderVM folder = _mapper.Map<Folder, FolderVM>(_database.Folders.Get(id));
            return folder;
        }

        // this method creates a new folder on the hard drive, saves it in the database and return it
        public ElementVM CreateFolder(FolderVM parentFolder, string name, string ownerId)
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

                ElementVM model = new ElementVM
                {
                    Id = newFolder.Id,
                    IsFolder = true,
                    Type = ElementVM.FileType.Folder,
                    ElemName = name,
                    LastModified = DateTime.Now.ToShortDateString(),
                    Size = null
                };
                return model;
            }
            return null;
        }

        // this method creates a new root folder on the hard drive and saves it in the database
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

        // this method upload files on the hard drive and saves them in the database
        public async Task<ICollection<ElementVM>> UploadFiles(IFormFileCollection files, FolderVM parentFolder)
        {
            List<ElementVM> elements = new List<ElementVM>();
            foreach (var uploadedFile in files)
            {
                string path = parentFolder.Path + "//" + uploadedFile.FileName;

                if (!System.IO.File.Exists(path))
                {
                    DAL.Entities.File file = new DAL.Entities.File
                    {
                        Id = Guid.NewGuid().ToString(),
                        Path = path,
                        IsShared = false,
                        FolderId = parentFolder.Id
                    };
                    _database.Files.Create(file);

                    ElementVM elem = new ElementVM
                    {
                        Id = file.Id,
                        IsFolder = false,
                        ElemName = uploadedFile.FileName,
                        LastModified = DateTime.Now.ToShortDateString(),
                        Type = ElementHelperClass.DefineFileType(ElementHelperClass.DefineFileExtension(uploadedFile.FileName)),
                        Size = ElementHelperClass.Recount(new Tuple<double, ElementVM.Unit>(uploadedFile.Length, ElementVM.Unit.B))
                    };
                    elements.Add(elem);
                }

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                } 
            }
            _database.Save();
            return elements;
        }

        // this method checks access to the needed folder by current user
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
