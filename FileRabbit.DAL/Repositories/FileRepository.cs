using System;
using System.Collections.Generic;
using System.Text;
using FileRabbit.DAL.Contexts;
using System.Linq;
using FileRabbit.Infrastructure.DAL;
using FileRabbit.DAL.Entities;

namespace FileRabbit.DAL.Repositories
{
    public class FileRepository : BaseRepository<File>, IRepository<File>
    {
        public FileRepository(ApplicationContext context) : base(context)
        { }
    }
}
