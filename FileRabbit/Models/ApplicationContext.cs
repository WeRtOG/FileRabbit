using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Folder> Folders { get; set; }
        public DbSet<File> Files { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
