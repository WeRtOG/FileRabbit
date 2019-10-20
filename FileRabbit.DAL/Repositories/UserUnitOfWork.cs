using FileRabbit.DAL.Contexts;
using FileRabbit.DAL.Entites;
using FileRabbit.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.DAL.Repositories
{
    public class UserUnitOfWork : IUserUnitOfWork
    {
        private UserContext db;
        private UserManager<User> userManager;
        //private RoleManager<User> roleManager;
        private SignInManager<User> signInManager;

        public UserUnitOfWork(UserContext context, UserManager<User> user, SignInManager<User> signIn)
        {
            db = context;
            userManager = user;
            //roleManager = role;
            signInManager = signIn;
        }

        public UserManager<User> UserManager
        {
            get { return userManager; }
        }

        //public RoleManager<User> RoleManager
        //{
        //    get { return roleManager; }
        //}

        public SignInManager<User> SignInManager
        {
            get { return signInManager; }
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

        public async Task SaveAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
