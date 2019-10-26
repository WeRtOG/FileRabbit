using FileRabbit.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.Infrastructure.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Folder> Folders { get; }
        IRepository<File> Files { get; }
        UserManager<User> UserManager { get; }
        SignInManager<User> SignInManager { get; }
        void Save();
    }
}
