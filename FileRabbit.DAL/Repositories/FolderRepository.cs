using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Interfaces;
using FileRabbit.DAL.Entites;
using FileRabbit.DAL.Contexts;
using System.Linq;

namespace FileRabbit.DAL.Repositories
{
    public class FolderRepository : IRepository<Folder>
    {
        private ApplicationContext db;

        public FolderRepository(ApplicationContext context)
        {
            db = context;
        }

        public void Create(Folder item)
        {
            db.Folders.Add(item);
        }

        public void Delete(string id)
        {
            Folder folder = db.Folders.Find(id);
            if (folder != null)
                db.Folders.Remove(folder);
        }

        public IEnumerable<Folder> Find(Func<Folder, bool> predicate)
        {
            return db.Folders.Where(predicate).ToList();
        }

        public Folder Get(string id)
        {
            return db.Folders.Find(id);
        }

        public IEnumerable<Folder> GetAll()
        {
            return db.Folders;
        }

        public void Update(Folder item)
        {
            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }
    }
}
