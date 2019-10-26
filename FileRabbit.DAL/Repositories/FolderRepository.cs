using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Contexts;
using System.Linq;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.DAL.Entities;

namespace FileRabbit.DAL.Repositories
{
    public class FolderRepository : BaseRepository<Folder>, IRepository<Folder>
    {
        public FolderRepository(ApplicationContext context) : base(context)
        { }
    }
}
