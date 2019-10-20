﻿using AutoMapper;
using FileRabbit.BLL.DTO;
using FileRabbit.BLL.Interfaces;
using FileRabbit.DAL.Entites;
using FileRabbit.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.BLL.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserUnitOfWork database;
        private readonly IMapper mapper;

        public AuthorizationService(IUserUnitOfWork unit, IMapper mapper)
        {
            database = unit;
            this.mapper = mapper;
        }

        public async Task<IdentityResult> CreateUser(UserDTO user)
        {
            User newUser = new User
            {
                Email = user.Email,
                UserName = user.UserName
            };
            var result = await database.UserManager.CreateAsync(newUser, user.Password);

            return result;
        }

        public async Task SignIn(UserDTO userDTO, bool remember)
        {
            User user = new User
            {
                Email = userDTO.Email,
                UserName = userDTO.UserName
            };
            await database.SignInManager.SignInAsync(user, remember);
        }

        public async Task<SignInResult> SignInWithPassword(LoginDTO login)
        {
            var result = await database.SignInManager.PasswordSignInAsync(login.UserName, login.Password, login.Remember, false);
            return result;
        }

        public async Task<UserDTO> FindByName(string name)
        {
            User user = await database.UserManager.FindByNameAsync(name);
            return mapper.Map<User, UserDTO>(user);
        }

        public async Task SignOut()
        {
            await database.SignInManager.SignOutAsync();
        }
    }
}