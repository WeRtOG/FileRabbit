using FileRabbit.BLL.DTO;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.BLL.Interfaces
{
    public interface IAuthorizationService
    {
        Task<IdentityResult> CreateUser(UserDTO user);

        Task SignIn(UserDTO userDTO, bool remember);

        Task<SignInResult> SignInWithPassword(LoginDTO login);

        Task<UserDTO> FindByName(string name);

        Task SignOut();
    }
}
