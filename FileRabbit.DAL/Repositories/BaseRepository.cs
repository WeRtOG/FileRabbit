using FileRabbit.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileRabbit.DAL.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public void Create(T item)
        {
            _dbSet.Add(item);
        }

        public void Delete(string id)
        {
            T item = _dbSet.Find(id);
            if (item != null)
                _dbSet.Remove(item);
        }

        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        public T Get(string id)
        {
            return _dbSet.Find(id);
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet;
        }

        public void Update(T item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }
    }
}
