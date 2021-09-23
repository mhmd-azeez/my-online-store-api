using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOnlineStoreAPI.Data;
using MyOnlineStoreAPI.Helpers;

namespace MyOnlineStoreAPI.Controllers
{
    [Authorize(Policy = "AdminOrSuperAdmin")]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Auth0Service _auth0Service;
        private readonly ApplicationDbContext _dbContext;

        public UsersController(
            Auth0Service auth0Service,
            ApplicationDbContext dbContext)
        {
            _auth0Service = auth0Service;
            _dbContext = dbContext;
        }

        [HttpGet("Search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<List<Auth0User>> Search(
            [Required] [MinLength(3)] string email)
        {
            return await _auth0Service.SearchAsync(email);
        }

        [HttpPost("Onboard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<User>> Onboard(UserOnboardRequest request)
        {
            var user = await _dbContext.Users.FindAsync(request.UserId);
            if (user is not null)
                return Ok(user);

            var auth0User = await _auth0Service.GetUserAsync(request.UserId);
            if (auth0User is null)
                return NotFound();

            user = new User
            {
                Id = auth0User.Id,
                Name = auth0User.Name,
                Email = auth0User.Email,
                Role = request.Role
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(user);
        }
    }

    public class UserOnboardRequest
    {
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}