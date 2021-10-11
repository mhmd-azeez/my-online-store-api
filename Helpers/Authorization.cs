using Microsoft.AspNetCore.Authorization;

using MyOnlineStoreAPI.Data;

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyOnlineStoreAPI.Helpers
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public class IsActiveRequirement : IAuthorizationRequirement
    {

    }

    public class IsActiveRequirementHandler : AuthorizationHandler<IsActiveRequirement>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public IsActiveRequirementHandler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsActiveRequirement requirement)
        {
            var userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId != null && int.TryParse(userId, out var id))
            {
                var user = await _applicationDbContext.Users.FindAsync(id);

                if (user.IsActive)
                    context.Succeed(requirement);
            }
        }
    }
}
