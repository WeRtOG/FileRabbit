using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FileRabbit.DAL.Entities;

namespace FileRabbit.DAL.Contexts
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<Folder> Folders { get; set; }
        public DbSet<File> Files { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            // create database with Folders and Files tables + all tables for Identity
            Database.EnsureCreated();
        }
    }
}
