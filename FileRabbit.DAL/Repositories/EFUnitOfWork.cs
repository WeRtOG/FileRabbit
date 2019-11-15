using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.DAL.Entities;
using System.Linq;

namespace FileRabbit.DAL.Repositories
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private ApplicationContext _db;
        private FileRepository _fileRepository;
        private FolderRepository _folderRepository;
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public EFUnitOfWork(ApplicationContext context, UserManager<User> user, SignInManager<User> signIn)
        {
            _db = context;
            _userManager = user;
            _signInManager = signIn;
        }

        public Dictionary<Type, object> Repositories
        {
            get { return _repositories; }
            set { Repositories = value; }
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            if (Repositories.Keys.Contains(typeof(T)))
            {
                return Repositories[typeof(T)] as IRepository<T>;
            }

            IRepository<T> repo = new BaseRepository<T>(_db);
            Repositories.Add(typeof(T), repo);
            return repo;
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
