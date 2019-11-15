using FileRabbit.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.Infrastructure.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        UserManager<User> UserManager { get; }
        SignInManager<User> SignInManager { get; }
        IRepository<T> GetRepository<T>() where T : class;
        void Save();
    }
}
