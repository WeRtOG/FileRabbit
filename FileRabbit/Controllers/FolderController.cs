using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FileRabbit.Models;
using FileRabbit.StaticClasses;
using FileRabbit.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileRabbit.Controllers
{
    public class FolderController : Controller
    {
        private ApplicationContext db;

        public FolderController(ApplicationContext context)
        {
            db = context;
        }

        public IActionResult Watch(string folderId)
        {
            // если пользователь не авторизован, кидаем его на страницу авторизации
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");
            
            Folder folder = db.Folders.Find(folderId);

            // если запрашиваемой папки не существует, выкидываем ошибку
            if (folder == null)
                return StatusCode(404, "Error code: 404. This folder doesn't exist.");

            // если пользователю не принадлежит запрашиваемая папка, выкидываем ошибку доступа
            if (folder.OwnerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return StatusCode(405, "Error code: 405. You don't have access to this folder.");

            DirectoryInfo dir = new DirectoryInfo(folder.Path);
            FileInfo[] files;
            DirectoryInfo[] dirs;

            files = dir.GetFiles();
            dirs = dir.GetDirectories();

            List<ElementViewModel> models = new List<ElementViewModel>();

            foreach(var elem in dirs)
            {
                ElementViewModel model = new ElementViewModel
                {
                    Type = ElementViewModel.FileType.Folder,
                    ElemName = elem.Name, 
                    LastModified = elem.LastWriteTime.ToShortDateString(), 
                    Size = null 
                };
                models.Add(model);
            }

            foreach (var elem in files)
            {
                // для более удобного отображения размера файла, вызовем функцию преобразования
                Tuple<double, ElementViewModel.Unit> size = new Tuple<double, ElementViewModel.Unit>(elem.Length, ElementViewModel.Unit.B);
                size = ElementHelperClass.Recount(size);
                ElementViewModel.FileType type = ElementHelperClass.DefineFileType(elem.Extension);

                ElementViewModel model = new ElementViewModel
                {
                    Type = type,
                    ElemName = elem.Name,
                    LastModified = elem.LastWriteTime.ToShortDateString(),
                    Size = size
                };
                models.Add(model);
            }

            ViewBag.Models = models;

            if (files.Length == 0 && dirs.Length == 0)
                ViewBag.Empty = true;
            else
                ViewBag.Empty = false;

            ViewBag.CurrFolderId = folderId;
            return View();
        }

        public IActionResult CreateNewUserFolder(Folder folder)
        {
            // создаём папку нового пользователя в хранилище
            Directory.CreateDirectory(folder.Path);
            db.Folders.Add(folder);
            db.SaveChanges();
            string folderId = folder.Id;
            return RedirectToAction("Watch", "Folder", new { folderId });
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(IFormFileCollection uploads, string folderId)
        {
            Folder parentFolder = db.Folders.Find(folderId);
            foreach (var uploadedFile in uploads)
            {
                string path = parentFolder.Path + "//" + uploadedFile.FileName;

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                Models.File file = new Models.File
                {
                    Id = Guid.NewGuid().ToString(),
                    Path = path,
                    IsShared = false,
                    OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    FolderId = folderId
                };
                db.Files.Add(file);
            }
            db.SaveChanges();

            return RedirectToAction("Watch", "Folder", new { folderId });
        }
    }
}