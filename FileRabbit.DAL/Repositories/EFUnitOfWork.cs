using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Interfaces;
using FileRabbit.DAL.Entites;
using FileRabbit.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FileRabbit.DAL.Repositories
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private ApplicationContext db;
        private FileRepository fileRepository;
        private FolderRepository folderRepository;

        public EFUnitOfWork(ApplicationContext context)
        {
            db = context;
        }

        public IRepository<Folder> Folders
        {
            get
            {
                if (folderRepository == null)
                    folderRepository = new FolderRepository(db);
                return folderRepository;
            }
        }

        public IRepository<File> Files
        {
            get
            {
                if (fileRepository == null)
                    fileRepository = new FileRepository(db);
                return fileRepository;
            }
        }

        private bool disposed = false;
        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    db.Dispose();
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
            db.SaveChanges();
        }
    }
}
