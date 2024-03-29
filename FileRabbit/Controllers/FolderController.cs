﻿using AutoMapper;
using FileRabbit.BLL.Exceptions;
using FileRabbit.BLL.Interfaces;
using FileRabbit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileRabbit.PL.Controllers
{
    public class FolderController : Controller
    {
        private readonly IFileSystemService _fileSystemService;

        public FolderController(IFileSystemService service)
        {
            _fileSystemService = service;
        }

        // this action returns all folders and files that are contained in the needed folder
        public IActionResult Watch(string folderId)
        {
            // if the user is not authenticated, redirect to login page
            if (folderId == null && !User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            FolderVM folder = _fileSystemService.GetFolderById(folderId);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // check access to needed folder
            if (_fileSystemService.CheckAccessToView(folder, userId) || _fileSystemService.HasSharedChildren(folderId))
            {
                List<ElementVM> elems = _fileSystemService.GetElementsFromFolder(_fileSystemService.GetFolderById(folderId), userId).ToList();
                Stack<FolderShortInfoVM> folderPath = _fileSystemService.GetFolderPath(folderId, userId);

                WatchPageVM model = new WatchPageVM { Elements = elems, FolderPath = folderPath, CurrFolderId = folderId };
                return View(model);
            }
            else
                throw new StatusCodeException($"You don't have access to folder with ID = {folderId}.", StatusCodes.Status403Forbidden);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        // this action uploads the collection of files to the hard drive and returns the upload result
        public async Task<IActionResult> Upload(IFormFileCollection uploads, string folderId)
        {
            if (!_fileSystemService.CheckEditAccess(_fileSystemService.GetFolderById(folderId), User.FindFirstValue(ClaimTypes.NameIdentifier)))
                throw new StatusCodeException($"You don't have access to folder with ID = {folderId}.", StatusCodes.Status403Forbidden);

            List<ElementVM> elements = (await _fileSystemService.UploadFiles(uploads, _fileSystemService.GetFolderById(folderId))).ToList();
            return new ObjectResult(elements);
        }

        // this action returns a single file to download
        public IActionResult Download(string fileId)
        {
            FileVM file = _fileSystemService.GetFileById(fileId);
            if (_fileSystemService.CheckAccessToView(file, User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return PhysicalFile(file.Path, file.ContentType, file.Name);
            else
                throw new StatusCodeException($"You don't have access to file with ID = {fileId}.", StatusCodes.Status403Forbidden);
        }

        // this action returns an archive with multiple files and folders to download
        public IActionResult DownloadMultiple(string currFolderId, string[] foldersId, string[] filesId)
        {
            MemoryStream ms = _fileSystemService.CreateArchive(currFolderId,
                User.FindFirstValue(ClaimTypes.NameIdentifier), foldersId, filesId);
            return File(ms, "application/archive", Guid.NewGuid() + ".zip");
        }

        // this action returns a single file to display
        public IActionResult DisplayFile(string fileId)
        {
            FileVM file = _fileSystemService.GetFileById(fileId);
            if (_fileSystemService.CheckAccessToView(file, User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=" + file.Name);
                return PhysicalFile(file.Path, file.ContentType);
            }
            else
                throw new StatusCodeException($"You don't have access to file with ID = {fileId}.", StatusCodes.Status403Forbidden);
        }

        // this action creates a new folder and returns the creating result
        [HttpPost]
        public IActionResult AddFolder(string folderId, string newFolderName)
        {
            if (!_fileSystemService.CheckEditAccess(_fileSystemService.GetFolderById(folderId), User.FindFirstValue(ClaimTypes.NameIdentifier)))
                throw new StatusCodeException($"You don't have access to folder with ID = {folderId}.", StatusCodes.Status403Forbidden);

            ElementVM newFolder = _fileSystemService.CreateFolder(_fileSystemService.GetFolderById(folderId),
                newFolderName, User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (newFolder == null)
                return BadRequest("This folder already exists.");
            return new ObjectResult(newFolder);
        }

        // this action removes the selected files and folders and returns the result removing result
        [HttpPost]
        public IActionResult Delete(string[] foldersId, string[] filesId)
        {
            bool success = _fileSystemService.RemoveFilesAndFolders(User.FindFirstValue(ClaimTypes.NameIdentifier), foldersId, filesId);
            return new ObjectResult(success);
        }

        // this action renames the selected file or folder and returns the result of renaming
        [HttpPost]
        public IActionResult Rename(string newName, string elementId, bool isFolder)
        {
            bool available;
            if (isFolder)
                available = _fileSystemService.CheckEditAccess(_fileSystemService.GetFolderById(elementId), User.FindFirstValue(ClaimTypes.NameIdentifier));
            else
                available = _fileSystemService.CheckEditAccess(_fileSystemService.GetFileById(elementId), User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (available)
            {
                bool success;
                success = isFolder ? _fileSystemService.RenameFolder(newName, elementId) : _fileSystemService.RenameFile(newName, elementId);
                return new ObjectResult(success);
            }
            else
                throw new StatusCodeException("You don't have access to " + (isFolder ? "folder" : "file") + $" with ID = {elementId}.",
                    StatusCodes.Status403Forbidden);
        }

        // this action shares or unshares the selected files and folder and returns the link to them
        public IActionResult Share(string currFolderId, string[] foldersId, string[] filesId, bool openAccess)
        {
            string link = _fileSystemService.ChangeAccess(currFolderId, User.FindFirstValue(ClaimTypes.NameIdentifier), 
                foldersId, filesId, openAccess);
            return new ObjectResult(link);
        }
    }
}