using AutoMapper;
using FileRabbit.BLL.Interfaces;
using FileRabbit.DAL.Entities;
using FileRabbit.Infrastructure.BLL;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.BLL.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUnitOfWork _database;
        private readonly IMapper _mapper;

        public AuthorizationService(IUnitOfWork unit, IMapper mapper)
        {
            _database = unit;
            this._mapper = mapper;
        }

        public async Task<IdentityResult> CreateUser(UserVM user)
        {
            User newUser = _mapper.Map<UserVM, User>(user);
            var result = await _database.UserManager.CreateAsync(newUser, user.Password);

            return result;
        }

        public async Task SignIn(UserVM userDTO, bool remember)
        {
            User user = _mapper.Map<UserVM, User>(userDTO);
            await _database.SignInManager.SignInAsync(user, remember);
        }

        public async Task<SignInResult> SignInWithPassword(LoginVM login)
        {
            var result = await _database.SignInManager.PasswordSignInAsync(login.UserName, login.Password, login.Remember, false);
            return result;
        }

        public async Task<UserVM> FindByName(string name)
        {
            User user = await _database.UserManager.FindByNameAsync(name);
            return _mapper.Map<User, UserVM>(user);
        }

        public async Task SignOut()
        {
            await _database.SignInManager.SignOutAsync();
        }
    }
}
