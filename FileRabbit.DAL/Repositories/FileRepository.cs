using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Interfaces;
using FileRabbit.DAL.Entites;
using FileRabbit.DAL.Contexts;
using System.Linq;

namespace FileRabbit.DAL.Repositories
{
    public class FileRepository : BaseRepository<File>, IRepository<File>
    {
        public FileRepository(ApplicationContext context) : base(context)
        { }
    }
}
