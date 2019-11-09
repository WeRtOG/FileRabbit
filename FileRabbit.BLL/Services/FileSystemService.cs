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
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
//using Ionic.Zip;

namespace FileRabbit.BLL.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly IUnitOfWork _database;
        private readonly IMapper _mapper;
        private const string pathRoot = "C:\\FileRabbitStorage";

        public FileSystemService(IUnitOfWork unit, IMapper mapper)
        {
            _database = unit;
            _mapper = mapper;
        }

        // this method returns all folders and files that are contained in the needed folder
        public ICollection<ElementVM> GetElementsFromFolder(FolderVM folderVM, string userId)
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
                Folder folder = childFolders.Find(f => f.Path == elem.FullName);
                if (CheckAccess(new FolderVM { OwnerId = folder.OwnerId, IsShared = folder.IsShared }, userId))
                {
                    ElementVM model = new ElementVM
                    {
                        Id = folder.Id,
                        IsFolder = true,
                        Type = ElementVM.FileType.Folder,
                        ElemName = elem.Name,
                        LastModified = elem.LastWriteTime.ToShortDateString(),
                        Size = null
                    };

                    models.Add(model);
                }
            }

            foreach (var elem in files)
            {
                DAL.Entities.File file = childFiles.Find(f => f.Path == elem.FullName);
                FileVM vm = new FileVM { IsShared = file.IsShared, OwnerId = _database.Folders.Get(file.FolderId).OwnerId };
                if (CheckAccess(vm, userId))
                {
                    // for a more convenient display of file size, call the conversion function
                    Tuple<double, ElementVM.Unit> size = new Tuple<double, ElementVM.Unit>(elem.Length, ElementVM.Unit.B);
                    size = ElementHelperClass.Recount(size);
                    ElementVM.FileType type = ElementHelperClass.DefineFileType(elem.Extension);

                    ElementVM model = new ElementVM
                    {
                        Id = childFiles.Find(f => f.Path == elem.FullName).Id,
                        IsFolder = false,
                        Type = type,
                        ElemName = elem.Name,
                        LastModified = elem.LastWriteTime.ToShortDateString(),
                        Size = size
                    };
                    models.Add(model);
                }
            }

            return models;
        }

        // this method return path to current folder as list
        public Stack<FolderShortInfoVM> GetFolderPath(string currFolderId)
        {
            Stack<FolderShortInfoVM> folderPath = new Stack<FolderShortInfoVM>();

            bool rootFolder = false;
            do
            {
                Folder folder = _database.Folders.Get(currFolderId);
                FolderShortInfoVM folderInfo = new FolderShortInfoVM { Id = folder.Id, Name = ElementHelperClass.DefineFileName(folder.Path) };
                currFolderId = folder.ParentFolderId;
                if (folder.ParentFolderId == null)
                {
                    rootFolder = true;
                    folderInfo.Name = "Your drive";
                }
                folderPath.Push(folderInfo);
            } while (!rootFolder);

            return folderPath;
        }

        // this method returns folder by id
        public FolderVM GetFolderById(string id)
        {
            FolderVM folder = _mapper.Map<Folder, FolderVM>(_database.Folders.Get(id));
            return folder;
        }

        // this method returns file by id
        public FileVM GetFileById(string id)
        {
            FileVM file = _mapper.Map<DAL.Entities.File, FileVM>(_database.Files.Get(id));
            file.OwnerId = _database.Folders.Get(file.FolderId).OwnerId;
            file.Name = ElementHelperClass.DefineFileName(file.Path);
            //file.Path = file.Path.Replace("//", "/");
            return file;
        }

        // this method creates a new folder on the hard drive, saves it in the database and return it
        public ElementVM CreateFolder(FolderVM parentFolder, string name, string ownerId)
        {
            string newFolderPath = parentFolder.Path + '\\' + name;
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
            // create a folder for new user
            Directory.CreateDirectory(pathRoot + "\\" + ownerId);
            Folder newFolder = new Folder
            {
                Id = ownerId,
                Path = pathRoot + '\\' + ownerId,
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
                string path = parentFolder.Path + '\\' + uploadedFile.FileName;

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

        // this method creates an archive and returns it for download
        public MemoryStream CreateArchive(string currFolderId, string userId, string[] foldersId, string[] filesId)
        {
            Folder parentFolder = _database.Folders.Get(currFolderId);

            ZipStrings.UseUnicode = true;
            MemoryStream outputMemStream = new MemoryStream();
            using (var zipStream = new ZipOutputStream(outputMemStream))
            {
                zipStream.SetLevel(0);
                int folderOffset = parentFolder.Path.Length + (parentFolder.Path.EndsWith("\\") ? 0 : 1);

                foreach (var id in filesId)
                {
                    DAL.Entities.File file = _database.Files.Get(id);
                    FileVM vm = new FileVM { IsShared = file.IsShared, OwnerId = _database.Folders.Get(file.FolderId).OwnerId };
                    if (CheckAccess(vm, userId))
                        CompressFile(file, zipStream, folderOffset);
                }

                foreach (var id in foldersId)
                {
                    Folder folder = _database.Folders.Get(id);
                    FolderVM vm = new FolderVM { IsShared = folder.IsShared, OwnerId = folder.OwnerId };
                    if (CheckAccess(vm, userId))
                        CompressFolder(folder, userId, zipStream, folderOffset);
                }

                zipStream.IsStreamOwner = false;
            }
            outputMemStream.Position = 0;
            return outputMemStream;
        }

        // this method recursively compresses a folder structure
        private void CompressFolder(Folder folder, string userId, ZipOutputStream zipStream, int folderOffset)
        {
            List<Folder> childFolders = _database.Folders.Find(f => f.ParentFolderId == folder.Id).ToList();
            List<DAL.Entities.File> childFiles = _database.Files.Find(f => f.FolderId == folder.Id).ToList();

            foreach (var childFile in childFiles)
            {
                FileVM file = new FileVM { IsShared = childFile.IsShared, OwnerId = _database.Folders.Get(childFile.FolderId).OwnerId };
                if (CheckAccess(file, userId))
                    CompressFile(childFile, zipStream, folderOffset);
            }

            foreach (var childFolder in childFolders)
            {
                FolderVM vm = new FolderVM { IsShared = childFolder.IsShared, OwnerId = childFolder.OwnerId };
                if (CheckAccess(vm, userId))
                    CompressFolder(childFolder, userId, zipStream, folderOffset);
            }
        }

        // this method compresses a file
        private void CompressFile(DAL.Entities.File file, ZipOutputStream zipStream, int folderOffset)
        {
            FileInfo fi = new FileInfo(file.Path);
            string entryName = file.Path.Substring(folderOffset);
            entryName = ZipEntry.CleanName(entryName);

            ZipEntry newEntry = new ZipEntry(entryName);
            newEntry.DateTime = fi.LastWriteTime;
            newEntry.Size = fi.Length;
            zipStream.PutNextEntry(newEntry);

            var buffer = new byte[4096];
            using (FileStream fsInput = System.IO.File.OpenRead(file.Path))
            {
                StreamUtils.Copy(fsInput, zipStream, buffer);
            }
            zipStream.CloseEntry();
        }

        // this method removes needed files and folders
        public bool RemoveFilesAndFolders(string userId, string[] foldersId, string[] filesId)
        {
            bool success = true;
            foreach(var id in foldersId)
            {
                Folder folder = _database.Folders.Get(id);
                FolderVM vm = new FolderVM { IsShared = folder.IsShared, OwnerId = folder.OwnerId };
                if (CheckAccess(vm, userId))
                {
                    try
                    {
                        Directory.Delete(folder.Path, true);
                        _database.Folders.Delete(id);
                        _database.Save();
                    }
                    catch
                    {
                        success = false;
                    }
                }
            }

            foreach (var id in filesId)
            {
                DAL.Entities.File file = _database.Files.Get(id);
                FileVM vm = new FileVM { IsShared = file.IsShared, OwnerId = _database.Folders.Get(file.FolderId).OwnerId };
                if (CheckAccess(vm, userId))
                {
                    try
                    {
                        System.IO.File.Delete(file.Path);
                        _database.Folders.Delete(id);
                        _database.Save();
                    }
                    catch
                    {
                        success = false;
                    }
                }
            }

            return success;
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

        // this method checks access to the needed file by current user
        public bool CheckAccess(FileVM file, string currentId)
        {
            if (file.IsShared)
                return true;
            else
            {
                if (file.OwnerId == currentId)
                    return true;
                return false;
            }
        }
    }
}
