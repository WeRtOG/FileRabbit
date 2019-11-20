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
using Microsoft.AspNetCore.StaticFiles;
using FileRabbit.BLL.Exceptions;
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

            List<Folder> childFolders = _database.GetRepository<Folder>().Find(f => f.ParentFolderId == folderVM.Id).ToList();
            List<DAL.Entities.File> childFiles = _database.GetRepository<DAL.Entities.File>().Find(f => f.FolderId == folderVM.Id).ToList();

            foreach (var elem in dirs)
            {
                string s = childFolders[0].Path;
                Folder folder = childFolders.Find(f => f.Path == elem.FullName);
                if (folder != null)
                {
                    if (CheckAccessToView(new FolderVM { OwnerId = folder.OwnerId, IsShared = folder.IsShared }, userId))
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
            }

            foreach (var elem in files)
            {
                DAL.Entities.File file = childFiles.Find(f => f.Path == elem.FullName);
                if (file != null)
                {
                    FileVM vm = new FileVM { IsShared = file.IsShared, OwnerId = _database.GetRepository<Folder>().Get(file.FolderId).OwnerId };
                    if (CheckAccessToView(vm, userId))
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
            }

            return models;
        }

        // this method return path to current folder as list
        public Stack<FolderShortInfoVM> GetFolderPath(string currFolderId, string userId)
        {
            Stack<FolderShortInfoVM> folderPath = new Stack<FolderShortInfoVM>();

            bool rootFolder = false;
            do
            {
                Folder folder = _database.GetRepository<Folder>().Get(currFolderId);
                if (folder == null)
                    throw new StatusCodeException("Unable to define folder path.", StatusCodes.Status500InternalServerError);

                FolderVM vm = new FolderVM { IsShared = folder.IsShared, OwnerId = folder.OwnerId };
                if (CheckAccessToView(vm, userId) || HasSharedChildren(currFolderId))
                {
                    FolderShortInfoVM folderInfo = new FolderShortInfoVM { Id = folder.Id, Name = ElementHelperClass.DefineFileName(folder.Path) };
                    currFolderId = folder.ParentFolderId;
                    if (folder.ParentFolderId == null)
                    {
                        rootFolder = true;
                        folderInfo.Name = "Your drive";
                    }
                    folderPath.Push(folderInfo);
                }
                else
                    rootFolder = true;
            } while (!rootFolder);

            return folderPath;
        }

        // this method returns folder by id
        public FolderVM GetFolderById(string id)
        {
            FolderVM folder = _mapper.Map<Folder, FolderVM>(_database.GetRepository<Folder>().Get(id));
            if (folder == null)
                throw new StatusCodeException($"The folder with ID = {id} doesn't exists.", StatusCodes.Status404NotFound);
            return folder;
        }

        // this method returns file by id
        public FileVM GetFileById(string id)
        {
            FileVM file = _mapper.Map<DAL.Entities.File, FileVM>(_database.GetRepository<DAL.Entities.File>().Get(id));
            if (file == null)
                throw new StatusCodeException($"The file with ID = {id} doesn't exists.", StatusCodes.Status404NotFound);

            file.OwnerId = _database.GetRepository<Folder>().Get(file.FolderId).OwnerId;
            file.Name = ElementHelperClass.DefineFileName(file.Path);

            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(file.Name, out contentType))
            {
                contentType = "application/octet-stream";
            }
            file.ContentType = contentType;

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
                _database.GetRepository<Folder>().Create(newFolder);
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
            _database.GetRepository<Folder>().Create(newFolder);
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
                        IsShared = parentFolder.IsShared,
                        FolderId = parentFolder.Id
                    };
                    _database.GetRepository<DAL.Entities.File>().Create(file);

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
            Folder parentFolder = _database.GetRepository<Folder>().Get(currFolderId);
            if (parentFolder == null)
                throw new StatusCodeException($"Invalid current folder ID ({currFolderId}). Unable to create archive.", 
                    StatusCodes.Status500InternalServerError);

            ZipStrings.UseUnicode = true;
            MemoryStream outputMemStream = new MemoryStream();
            using (var zipStream = new ZipOutputStream(outputMemStream))
            {
                zipStream.SetLevel(0);
                int folderOffset = parentFolder.Path.Length + (parentFolder.Path.EndsWith("\\") ? 0 : 1);

                foreach (var id in filesId)
                {
                    DAL.Entities.File file = _database.GetRepository<DAL.Entities.File>().Get(id);
                    FileVM vm = new FileVM { IsShared = file.IsShared, OwnerId = _database.GetRepository<Folder>().Get(file.FolderId).OwnerId };
                    if (CheckAccessToView(vm, userId))
                        CompressFile(file, zipStream, folderOffset);
                }

                foreach (var id in foldersId)
                {
                    Folder folder = _database.GetRepository<Folder>().Get(id);
                    FolderVM vm = new FolderVM { IsShared = folder.IsShared, OwnerId = folder.OwnerId };
                    if (CheckAccessToView(vm, userId))
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
            List<Folder> childFolders = _database.GetRepository<Folder>().Find(f => f.ParentFolderId == folder.Id).ToList();
            List<DAL.Entities.File> childFiles = _database.GetRepository<DAL.Entities.File>().Find(f => f.FolderId == folder.Id).ToList();

            foreach (var childFile in childFiles)
            {
                FileVM file = new FileVM { IsShared = childFile.IsShared, OwnerId = _database.GetRepository<Folder>().Get(childFile.FolderId).OwnerId };
                if (CheckAccessToView(file, userId))
                    CompressFile(childFile, zipStream, folderOffset);
            }

            foreach (var childFolder in childFolders)
            {
                FolderVM vm = new FolderVM { IsShared = childFolder.IsShared, OwnerId = childFolder.OwnerId };
                if (CheckAccessToView(vm, userId))
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

        // this method removes desired files and folders
        public bool RemoveFilesAndFolders(string userId, string[] foldersId, string[] filesId)
        {
            bool success = true;
            foreach(var id in foldersId)
            {
                Folder folder = _database.GetRepository<Folder>().Get(id);
                if (folder == null)
                    throw new StatusCodeException($"The folder with ID = {id} doesn't exists.", StatusCodes.Status404NotFound);

                FolderVM vm = new FolderVM { IsShared = folder.IsShared, OwnerId = folder.OwnerId };
                if (CheckEditAccess(vm, userId))
                {
                    try
                    {
                        Directory.Delete(folder.Path, true);
                        Folder deletedFolder = _database.GetRepository<Folder>().Get(id);
                        RemoveChildrenFolders(deletedFolder);
                        _database.GetRepository<Folder>().Delete(id);
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
                DAL.Entities.File file = _database.GetRepository<DAL.Entities.File>().Get(id);
                if (file == null)
                    throw new StatusCodeException($"The file with ID = {id} doesn't exists.", StatusCodes.Status404NotFound);

                FileVM vm = new FileVM { IsShared = file.IsShared, OwnerId = _database.GetRepository<Folder>().Get(file.FolderId).OwnerId };
                if (CheckEditAccess(vm, userId))
                {
                    try
                    {
                        System.IO.File.Delete(file.Path);
                        _database.GetRepository<DAL.Entities.File>().Delete(id);
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

        // this method removes folders from database recursive
        private void RemoveChildrenFolders(Folder folder)
        {
            List<Folder> childs = _database.GetRepository<Folder>().Find(f => f.ParentFolderId == folder.Id).ToList();
            foreach(Folder child in childs)
            {
                RemoveChildrenFolders(child);
            }
            _database.GetRepository<Folder>().Delete(folder.Id);
        }

        // this method renames the desired folder
        public bool RenameFolder(string newName, string folderId)
        {
            Folder folder = _database.GetRepository<Folder>().Get(folderId);
            if (folder == null)
                throw new StatusCodeException($"The folder with ID = {folderId} doesn't exists.", StatusCodes.Status404NotFound);

            string oldPath = folder.Path;
            string newPath = oldPath.Substring(0, oldPath.LastIndexOf('\\') + 1) + newName;
            folder.Path = newPath;
            if (!Directory.Exists(newPath))
            {
                Directory.Move(oldPath, newPath);
                _database.GetRepository<Folder>().Update(folder);
                ChangeChildrenPath(folder, oldPath, newPath);
                _database.Save();
                return true;
            }
            else 
                return false;
        }

        // this method changes the path of the children of the folder recursively in the database
        private void ChangeChildrenPath(Folder folder, string oldPath, string newPath)
        {
            List<Folder> childFolders = _database.GetRepository<Folder>().Find(f => f.ParentFolderId == folder.Id).ToList();
            List<DAL.Entities.File> childFiles = _database.GetRepository<DAL.Entities.File>().Find(f => f.FolderId == folder.Id).ToList();

            foreach(var file in childFiles)
            {
                file.Path = file.Path.Replace(oldPath, newPath);
                _database.GetRepository<DAL.Entities.File>().Update(file);
            }

            foreach(var childFolder in childFolders)
            {
                childFolder.Path = childFolder.Path.Replace(oldPath, newPath);
                _database.GetRepository<Folder>().Update(childFolder);
                ChangeChildrenPath(childFolder, oldPath, newPath);
            }
        }

        // this method renames the desired file
        public bool RenameFile(string newName, string fileId)
        {
            DAL.Entities.File file = _database.GetRepository<DAL.Entities.File>().Get(fileId);
            if (file == null)
                throw new StatusCodeException($"The file with ID = {fileId} doesn't exists.", StatusCodes.Status404NotFound);

            string oldPath = file.Path;
            string extension = ElementHelperClass.DefineFileExtension(oldPath);
            string newPath = oldPath.Substring(0, oldPath.LastIndexOf('\\') + 1) + newName + extension;
            file.Path = newPath;
            if (!System.IO.File.Exists(newPath))
            {
                System.IO.File.Move(oldPath, newPath);
                _database.GetRepository<DAL.Entities.File>().Update(file);
                _database.Save();
                return true;
            }
            else
                return false;
        }

        // this method changes access to view for the selected folders and files and returns folder id
        public string ChangeAccess(string currFolderId, string userId, string[] foldersId, string[] filesId, bool openAccess)
        {
            Folder parent = _database.GetRepository<Folder>().Get(currFolderId);
            if (parent == null)
                throw new StatusCodeException($"The folder with ID = {currFolderId} doesn't exists.", StatusCodes.Status404NotFound);

            FolderVM parentVM = new FolderVM { IsShared = parent.IsShared, OwnerId = parent.OwnerId };
            if (CheckEditAccess(parentVM, userId))
            {

                string idForLink;
                if (foldersId.Length == 1 && filesId.Length == 0)
                    idForLink = foldersId[0];
                else
                    idForLink = currFolderId;

                foreach (string id in filesId)
                {
                    DAL.Entities.File file = _database.GetRepository<DAL.Entities.File>().Get(id);
                    if (file == null)
                        throw new StatusCodeException($"The file with ID = {id} doesn't exists.", StatusCodes.Status404NotFound);

                    FileVM vm = new FileVM { IsShared = file.IsShared, OwnerId = _database.GetRepository<Folder>().Get(file.FolderId).OwnerId };
                    if (CheckEditAccess(vm, userId))
                    {
                        file.IsShared = openAccess;
                        _database.GetRepository<DAL.Entities.File>().Update(file);
                    }
                }

                foreach (string id in foldersId)
                {
                    Folder folder = _database.GetRepository<Folder>().Get(id);
                    if (folder == null)
                        throw new StatusCodeException($"The folder with ID = {id} doesn't exists.", StatusCodes.Status404NotFound);

                    FolderVM vm = new FolderVM { IsShared = folder.IsShared, OwnerId = folder.OwnerId };
                    if (CheckEditAccess(vm, userId))
                    {
                        folder.IsShared = openAccess;
                        _database.GetRepository<Folder>().Update(folder);
                        ChangeChildrenAccess(folder, openAccess);
                    }
                }

                _database.Save();
                return idForLink;
            }
            else
                throw new StatusCodeException($"You don't have access to folder with ID = {currFolderId}.", StatusCodes.Status403Forbidden);
        }

        // this method changes access to view for the children of the selected folder
        private void ChangeChildrenAccess(Folder parentFolder, bool openAccess)
        {
            List<Folder> childFolders = _database.GetRepository<Folder>().Find(f => f.ParentFolderId == parentFolder.Id).ToList();
            List<DAL.Entities.File> childFiles = _database.GetRepository<DAL.Entities.File>().Find(f => f.FolderId == parentFolder.Id).ToList();

            foreach (var file in childFiles)
            {
                file.IsShared = openAccess;
                _database.GetRepository<DAL.Entities.File>().Update(file);
            }

            foreach (var folder in childFolders)
            {
                folder.IsShared = openAccess;
                _database.GetRepository<Folder>().Update(folder);
                ChangeChildrenAccess(folder, openAccess);
            }
        }

        // this method checks that the folder has shared children
        public bool HasSharedChildren(string parentFolderId)
        {
            Folder parent = _database.GetRepository<Folder>().Get(parentFolderId);
            if (parent == null)
                throw new StatusCodeException($"The folder with ID = {parentFolderId} doesn't exists.", StatusCodes.Status404NotFound);

            List<Folder> childFolders = _database.GetRepository<Folder>().Find(f => f.ParentFolderId == parent.Id).ToList();
            List<DAL.Entities.File> childFiles = _database.GetRepository<DAL.Entities.File>().Find(f => f.FolderId == parent.Id).ToList();

            foreach (var child in childFolders)
            {
                if (child.IsShared)
                    return true;
            }

            foreach (var child in childFiles)
            {
                if (child.IsShared)
                    return true;
            }

            return false;
        }

        // this method checks access to view the needed folder by current user
        public bool CheckAccessToView(FolderVM folder, string currentId)
        {
            if (folder == null)
                throw new StatusCodeException("Unable to check access to the folder.", StatusCodes.Status500InternalServerError);

            if (folder.IsShared)
                return true;
            else
                return CheckEditAccess(folder, currentId);
        }

        // this method checks access to view the needed file by current user
        public bool CheckAccessToView(FileVM file, string currentId)
        {
            if (file == null)
                throw new StatusCodeException("Unable to check access to the file.", StatusCodes.Status500InternalServerError);

            if (file.IsShared)
                return true;
            else
                return CheckEditAccess(file, currentId);
        }

        // this method checks the current user for editing access to the needed folder
        public bool CheckEditAccess(FolderVM folder, string currentId)
        {
            if (folder == null)
                throw new StatusCodeException("Unable to check access to the folder.", StatusCodes.Status500InternalServerError);

            if (folder.OwnerId == currentId)
                return true;
            return false;
        }

        // this method checks the current user for editing access to the needed file
        public bool CheckEditAccess(FileVM file, string currentId)
        {
            if (file == null)
                throw new StatusCodeException("Unable to check access to the file.", StatusCodes.Status500InternalServerError);

            if (file.OwnerId == currentId)
                return true;
            return false;
        }
    }
}
