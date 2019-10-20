using AutoMapper;
using FileRabbit.BLL.DTO;
using FileRabbit.DAL.Entites;
using FileRabbit.PL.ViewModels;
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
            //CreateMap<ICollection<ElementDTO>, List<ElementViewModel>>();
            CreateMap<ElementViewModel, ElementDTO>();
            CreateMap<ElementDTO, ElementViewModel>();
            CreateMap<FolderDTO, Folder>();
            CreateMap<Folder, FolderDTO>();
            CreateMap<LoginDTO, LoginViewModel>();
            CreateMap<LoginViewModel, LoginDTO>();
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
        }
    }
}
