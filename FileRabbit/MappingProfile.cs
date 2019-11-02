using AutoMapper;
using FileRabbit.DAL.Entities;
using FileRabbit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.PL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FileVM, File>();
            CreateMap<File, FileVM>();
            CreateMap<FolderVM, Folder>();
            CreateMap<Folder, FolderVM>();
            CreateMap<User, UserVM>();
            CreateMap<UserVM, User>();
        }
    }
}
