using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using FileRabbit.PL.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FileRabbit.BLL.Interfaces;
using AutoMapper;
using FileRabbit.BLL.DTO;

namespace FileRabbit.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IFileSystemService fileSystemService;
        private readonly IMapper mapper;

        public AccountController(IAuthorizationService auth, IFileSystemService service, IMapper mapper)
        {
            authorizationService = auth;
            fileSystemService = service;
            this.mapper = mapper;
        }

        #region Register
        [HttpGet]
        public IActionResult Register()
        {
            // если пользователь уже авторизован, то регистрация ему не нужна - перенаправляем на главную страницу
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserDTO user = new UserDTO 
                { 
                    Id = Guid.NewGuid().ToString(),
                    Email = model.Email,
                    UserName = model.UserName, 
                    Password = model.Password 
                };
                var result = await authorizationService.CreateUser(user);

                if (result.Succeeded)
                {
                    await authorizationService.SignIn(user, false);
                    fileSystemService.CreateFolder(user.Id);
                    string folderId = user.Id;
                    return RedirectToAction("Watch", "Folder", new { folderId });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        #endregion

        #region Login/Logoff
        [HttpGet]
        public IActionResult Login()
        {
            // если пользователь уже авторизован, то регистрация ему не нужна - перенаправляем на главную страницу
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                LoginDTO loginDTO = mapper.Map<LoginViewModel, LoginDTO>(model);
                var result = await authorizationService.SignInWithPassword(loginDTO);
                if (result.Succeeded)
                {
                    var user = await authorizationService.FindByName(model.UserName);
                    var userId = user.Id;
                    if (userId != null)
                        return RedirectToAction("Watch", "Folder", new { folderId = user.Id });
                    else
                        return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            // удаляем аутентификационные куки
            await authorizationService.SignOut();
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}