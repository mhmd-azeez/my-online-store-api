using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MyOnlineStoreAPI.Helpers
{

    public static class AuthorizationExtensions
    {
        public static void AddPermissionPolicy(this AuthorizationOptions options, string permission)
        {
            options.AddPolicy(permission, builder => builder.AddRequirements(new PermissionRequirement(permission)));
        }
    }

    public static class Permissions
    {
        public const string CurrencyGet = "Currency:Get";
        
        public const string ProductsList = "Products:List";
        public const string ProductsGet = "Products:Get";
        public const string ProductsUpdate = "Products:Update";
        public const string ProductsCreate = "Products:Create";
        public const string ProductsDelete = "Products:Delete";

        public const string UsersSearch = "Users:Search";
        public const string UsersOnboard = "Users:Onboard";
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }

    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
             PermissionRequirement requirement)
        {
            if (context.User.HasClaim("permissions", requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}