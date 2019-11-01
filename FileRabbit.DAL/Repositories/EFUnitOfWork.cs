using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.DAL.Entities;

namespace FileRabbit.DAL.Repositories
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private ApplicationContext _db;
        private FileRepository _fileRepository;
        private FolderRepository _folderRepository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public EFUnitOfWork(ApplicationContext context, UserManager<User> user, SignInManager<User> signIn)
        {
            _db = context;
            _userManager = user;
            _signInManager = signIn;
        }

        public IRepository<Folder> Folders
        {
            get
            {
                if (_folderRepository == null)
                    _folderRepository = new FolderRepository(_db);
                return _folderRepository;
            }
        }

        public IRepository<File> Files
        {
            get
            {
                if (_fileRepository == null)
                    _fileRepository = new FileRepository(_db);
                return _fileRepository;
            }
        } 

        public UserManager<User> UserManager
        {
            get { return _userManager; }
        }

        public SignInManager<User> SignInManager
        {
            get { return _signInManager; }
        }

        private bool disposed = false;
        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
