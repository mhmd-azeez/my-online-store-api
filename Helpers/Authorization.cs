using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MyOnlineStoreAPI.Helpers
{
    public class RoleOrSuperAdminRequirement : IAuthorizationRequirement
    {
        public RoleOrSuperAdminRequirement(string role)
        {
            Role = role;
        }

        public string Role { get; }
    }

    public class RoleOrSuperAdminRequirementHandler : AuthorizationHandler<RoleOrSuperAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
             RoleOrSuperAdminRequirement requirement)
        {
            if (context.User.HasClaim("permissions", "superadmin") ||
                context.User.IsInRole(requirement.Role))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}