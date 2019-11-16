using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileRabbit.BLL.Interfaces;
using AutoMapper;
using FileRabbit.ViewModels;

namespace FileRabbit.PL.Controllers
{
    public class FolderController : Controller
    {
        private readonly IFileSystemService fileSystemService;
        private readonly IMapper mapper;

        public FolderController(IFileSystemService service, IMapper mapper)
        {
            fileSystemService = service;
            this.mapper = mapper;
        }

        // this action returns all folders and files that are contained in the needed folder
        public IActionResult Watch(string folderId)
        {
            // if the user is not authenticated, redirect to login page
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            FolderVM folder = fileSystemService.GetFolderById(folderId);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // check access to needed folder
            if (fileSystemService.CheckAccess(folder, userId))
            {
                List<ElementVM> models = fileSystemService.GetElementsFromFolder(fileSystemService.GetFolderById(folderId), userId).ToList();
                Stack<FolderShortInfoVM> folderPath = fileSystemService.GetFolderPath(folderId);

                ViewBag.FolderPath = folderPath;

                // if folder is empty
                if (models.Count == 0)
                    ViewBag.Empty = true;
                else
                    ViewBag.Empty = false;

                ViewBag.CurrFolderId = folderId;
                return View(models);
            }
            else
                return StatusCode(405, "Error code: 405. You don't have access to this folder.");
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        // this action uploads the collection of files to the hard drive and returns the upload result
        public async Task<IActionResult> Upload(IFormFileCollection uploads, string folderId)
        {
            List<ElementVM> elements = (await fileSystemService.UploadFiles(uploads, fileSystemService.GetFolderById(folderId))).ToList();

            return new ObjectResult(elements);
        }

        // this action returns a single file to download
        public IActionResult Download(string fileId)
        {
            FileVM file = fileSystemService.GetFileById(fileId);
            if (fileSystemService.CheckAccess(file, User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return PhysicalFile(file.Path, file.ContentType, file.Name);
            else
                return StatusCode(405, "Error code: 405. You don't have access to this file.");
        }

        // this action returns an archive with multiple files and folders to download
        public IActionResult DownloadMultiple(string currFolderId, string[] foldersId, string[] filesId)
        {
            //currFolderId = "1244ded8-e7e5-4cb4-845a-10dad1c04982";
            //string userId = "1244ded8-e7e5-4cb4-845a-10dad1c04982";
            //foldersId = { "57bc2f57-fe1c-445e-80b3-a0641f8eaaab", "ee867c79-01ae-4726-ae91-14d3059cb7a9" };
            //filesId = { "9696f236-bf8e-4eaa-a603-0f20b5dbb19d", "20e236a7-2e9d-41cb-919f-9c911152fd2e" };
            MemoryStream ms = fileSystemService.CreateArchive(currFolderId,
                User.FindFirstValue(ClaimTypes.NameIdentifier), foldersId, filesId);
            return File(ms, "application/archive", Guid.NewGuid() + ".zip");
        }

        // this action returns a single file to display
        public IActionResult DisplayFile(string fileId)
        {
            FileVM file = fileSystemService.GetFileById(fileId);
            if (fileSystemService.CheckAccess(file, User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=" + file.Name);
                return PhysicalFile(file.Path, file.ContentType);
            }
            else
                return StatusCode(405, "Error code: 405. You don't have access to this file.");
        }

        // this action creates a new folder and returns the creating result
        [HttpPost]
        public IActionResult AddFolder(string folderId, string newFolderName)
        {
            ElementVM newFolder = fileSystemService.CreateFolder(fileSystemService.GetFolderById(folderId),
                newFolderName, User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (newFolder == null)
                return BadRequest("This folder already exists.");
            return new ObjectResult(newFolder);
        }

        // this action removes the selected files and folders and returns the result removing result
        public IActionResult Delete(string[] foldersId, string[] filesId)
        {
            bool success = fileSystemService.RemoveFilesAndFolders(User.FindFirstValue(ClaimTypes.NameIdentifier), foldersId, filesId);
            return new ObjectResult(success);
        }

        // this action renames the selected file or folder and returns the result of renaming
        public IActionResult Rename(string newName, string elementId, bool isFolder)
        {
            bool available;
            if (isFolder)
                available = fileSystemService.CheckAccess(fileSystemService.GetFolderById(elementId), User.FindFirstValue(ClaimTypes.NameIdentifier));
            else
                available = fileSystemService.CheckAccess(fileSystemService.GetFileById(elementId), User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (available)
            {
                bool success;
                success = isFolder ? fileSystemService.RenameFolder(newName, elementId) : fileSystemService.RenameFile(newName, elementId);
                return new ObjectResult(success);
            }
            else
                return StatusCode(405, "Error code: 405. You don't have access to this " + (isFolder ? "folder" : "file") + " .");
        }
    }
}