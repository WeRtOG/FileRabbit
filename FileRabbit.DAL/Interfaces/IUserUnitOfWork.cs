using FileRabbit.DAL.Entites;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.DAL.Interfaces
{
    public interface IUserUnitOfWork : IDisposable
    {
        UserManager<User> UserManager { get; }
        //RoleManager<User> RoleManager { get; }
        SignInManager<User> SignInManager { get; }
        Task SaveAsync();
    }
}