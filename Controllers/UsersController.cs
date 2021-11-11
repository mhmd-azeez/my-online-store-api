using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MyOnlineStoreAPI.Data;
using MyOnlineStoreAPI.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyOnlineStoreAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
        {
            request.Email = request.Email.ToLowerInvariant();

            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (dbUser is not null)
            {
                ModelState.AddModelError(nameof(request.Email), "This email address is already associated with a user.");
                return ValidationProblem();
            }

            dbUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role,
                IsActive = true,
            };

            var hashResult = PasswordHelper.HashPassword(request.Password);

            dbUser.PasswordHash = hashResult.PasswordHash;
            dbUser.PasswordSalt = hashResult.PasswordSalt;

            _dbContext.Users.Add(dbUser);
            await _dbContext.SaveChangesAsync();

            var response = new UserResponse(dbUser);

            return Created("", response);
        }

        [Authorize]
        [ProducesResponseType(403)]
        [ProducesResponseType(200)]
        [HttpGet("self")]
        public async Task<ActionResult<UserResponse>> GetSelf()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Forbid();

            var user = await _dbContext.Users.FindAsync(userId);
            return new UserResponse(user);
        }

        [Authorize]
        [ProducesResponseType(403)]
        [ProducesResponseType(200)]
        [HttpPut("self")]
        public async Task<ActionResult<UserResponse>> UpdateSelf(UpdateSelfRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Forbid();

            var user = await _dbContext.Users.FindAsync(userId);

            user.Email = request.Email;
            user.FullName = request.FullName;

            if (!string.IsNullOrEmpty( request.Password))
            {
                var hashResult = PasswordHelper.HashPassword(request.Password);
                user.PasswordHash = hashResult.PasswordHash;
                user.PasswordSalt = hashResult.PasswordSalt;
            }

            await _dbContext.SaveChangesAsync();

            return new UserResponse(user);
        }

    }

    public class UpdateSelfRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class UserResponse
    {
        public UserResponse(User user)
        {
            Id = user.Id;
            Email = user.Email;
            FullName = user.FullName;
            Role = user.Role;
            IsActive = user.IsActive;
        }

        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
