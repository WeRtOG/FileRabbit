using FileRabbit.DAL.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Folder> Folders { get; }
        IRepository<File> Files { get; }
        void Save();
    }
}
