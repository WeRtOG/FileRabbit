﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FileRabbit.BLL.Interfaces;
using AutoMapper;
using FileRabbit.ViewModels;
using FileRabbit.Infrastructure.BLL;

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
        // this action is called by clicking the registration button in the header
        [HttpGet]
        public IActionResult Register()
        {
            // if the user is authenticated, he doesn't need registration
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        //this action is called by clicking the registration button on the form
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            // if all inputs field are correct
            if (ModelState.IsValid)
            {
                // create a new user
                UserVM user = new UserVM
                { 
                    Id = Guid.NewGuid().ToString(),
                    Email = model.Email,
                    UserName = model.UserName, 
                    Password = model.Password 
                };
                var result = await authorizationService.CreateUser(user);

                // if registration is successful, go to new empty folder of user
                if (result.Succeeded)
                {
                    await authorizationService.SignIn(user, false);
                    fileSystemService.CreateFolder(user.Id);    // new root folder creating
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
        // this action is called by clicking the login button in the header
        [HttpGet]
        public IActionResult Login()
        {
            // if the user is authenticated, he doesn't need registration
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // this action is called by clicking the login button on the form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            // if all inputs field are correct
            if (ModelState.IsValid)
            {
                // try to sign in
                var result = await authorizationService.SignInWithPassword(model);
                if (result.Succeeded)
                {
                    // after successful login redirect to root folder of the user
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

        // this action is called by clicking the logoff button in the header
        [HttpGet]
        public async Task<IActionResult> LogOff()
        {
            // удаляем аутентификационные куки
            await authorizationService.SignOut();
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}