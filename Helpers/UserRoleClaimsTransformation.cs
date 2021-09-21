using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using MyOnlineStoreAPI.Data;

namespace MyOnlineStoreAPI.Helpers
{
    public class UserRoleClaimsTransformation : IClaimsTransformation
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRoleClaimsTransformation(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (userId is null) return principal;
            var user = await _dbContext.Users.FindAsync(userId);

            if (user?.Role != null)
                identity.AddClaim(new Claim(identity.RoleClaimType, user.Role));

            return principal;
        }
    }
}