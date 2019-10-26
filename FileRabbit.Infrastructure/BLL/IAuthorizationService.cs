using FileRabbit.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileRabbit.Infrastructure.BLL
{
    public interface IAuthorizationService
    {
        Task<IdentityResult> CreateUser(UserVM user);

        Task SignIn(UserVM userDTO, bool remember);

        Task<SignInResult> SignInWithPassword(LoginVM login);

        Task<UserVM> FindByName(string name);

        Task SignOut();
    }
}
