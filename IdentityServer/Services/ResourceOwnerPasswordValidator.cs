using IdentityModel;
using IdentityServer.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> userManager;

        public ResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //Email kontrol
            var existUser = await userManager.FindByEmailAsync(context.UserName);
            if (existUser == null)
            {
                var errors = new Dictionary<string, object>();
                errors.Add("errors", new List<string> { "Email veya şifre yanlış" });
                context.Result.CustomResponse = errors;
                return;
            }

            //Password kontrol
            var passwordCheck = await userManager.CheckPasswordAsync(existUser, context.Password);
            if (passwordCheck == false)
            {
                var errors = new Dictionary<string, object>();
                errors.Add("errors", new List<string> { "Email veya şifre yanlış" });
                context.Result.CustomResponse = errors;
                return;
            }

            //başarılı olma durumunda idsrv'e bildiriyoruz.
            context.Result = new GrantValidationResult(existUser.Id.ToString(),
                OidcConstants.AuthenticationMethods.Password);
        }
    }
}
