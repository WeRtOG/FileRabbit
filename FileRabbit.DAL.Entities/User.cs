using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.DAL.Entities
{
    public class User : IdentityUser
    {
        public virtual ICollection<Folder> Folders { get; set; }

        public User()
        {
            Folders = new List<Folder>();
        }
    }
}
