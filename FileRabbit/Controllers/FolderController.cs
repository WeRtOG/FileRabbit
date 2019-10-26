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

        public IActionResult Watch(string folderId)
        {
            // если пользователь не авторизован, кидаем его на страницу авторизации
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            FolderVM folder = fileSystemService.GetFolderById(folderId);

            if (fileSystemService.CheckAccess(folder, User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                List<ElementVM> models = fileSystemService.GetElementsFromFolder(fileSystemService.GetFolderById(folderId)).ToList();

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
        public async Task<IActionResult> Upload(IFormFileCollection uploads, string folderId)
        {
            await fileSystemService.UploadFiles(uploads, fileSystemService.GetFolderById(folderId));

            return RedirectToAction("Watch", "Folder", new { folderId });
        }

        [HttpPost]
        public IActionResult AddFolder(string folderId, string newFolderName)
        {
            string newFolderId = fileSystemService.CreateFolder(fileSystemService.GetFolderById(folderId),
                newFolderName, User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (newFolderId == null)
            {
                ViewBag.ErrorMessage = "This folder already exists.";
                return PartialView("_AddFolder");
            }

            return RedirectToAction("Watch", "Folder", new { folderId });
        }
    }
}