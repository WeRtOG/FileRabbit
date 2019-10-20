using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Interfaces;
using FileRabbit.DAL.Entites;
using FileRabbit.DAL.Contexts;
using System.Linq;

namespace FileRabbit.DAL.Repositories
{
    public class FileRepository : IRepository<File>
    {
        private ApplicationContext db;

        public FileRepository(ApplicationContext context)
        {
            db = context;
        }

        public void Create(File item)
        {
            db.Files.Add(item);
        }

        public void Delete(string id)
        {
            File file = db.Files.Find(id);
            if (file != null)
                db.Files.Remove(file);
        }

        public IEnumerable<File> Find(Func<File, bool> predicate)
        {
            return db.Files.Where(predicate).ToList();
        }

        public File Get(string id)
        {
            return db.Files.Find(id);
        }

        public IEnumerable<File> GetAll()
        {
            return db.Files;
        }

        public void Update(File item)
        {
            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }
    }
}
