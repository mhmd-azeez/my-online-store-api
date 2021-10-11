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
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public UsersController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public async Task<List<UserResponse>> GetAllUsers()
        {
            var users = await _applicationDbContext.Users.ToListAsync();
            return users.Select(u => new UserResponse(u)).ToList();
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int id)
        {
            var user = await _applicationDbContext.Users.FindAsync(id);
            if (user is null) return NotFound();

            return Ok(new UserResponse(user));
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserResponse>> CreateUser(UserRequest request)
        {
            if (string.IsNullOrEmpty(request.Password))
            {
                ModelState.AddModelError(nameof(request.Password), "Password is required.");
                return ValidationProblem();
            }

            var dbUser = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (dbUser is not null)
            {
                ModelState.AddModelError(nameof(request.Email), "This email address is associated with another user.");
                return ValidationProblem();
            }

            dbUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role
            };

            var hashResult = PasswordHelper.Hash(request.Password);

            dbUser.PasswordHash = hashResult.PasswordHash;
            dbUser.PasswordSalt = hashResult.Salt;

            _applicationDbContext.Users.Add(dbUser);

            await _applicationDbContext.SaveChangesAsync();

            var response = new UserResponse(dbUser);

            return CreatedAtAction(nameof(GetUserById), new { id = dbUser.Id }, response);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserResponse>> UpdateUser(int id, UserRequest request)
        {
            var user = await _applicationDbContext.Users.FindAsync(id);
            if (user is null) return NotFound();

            user.Email = request.Email;
            user.FullName = request.FullName;
            user.Role = request.Role;
            user.IsActive = request.IsActive;

            if (!string.IsNullOrEmpty(request.Password))
            {
                var hashResult = PasswordHelper.Hash(request.Password);
                user.PasswordHash = hashResult.PasswordHash;
                user.PasswordSalt = hashResult.Salt;
            }

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new UserResponse(user));
        }

        [HttpPut("self")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserResponse>> UpdateSelf(SelfUpdateRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Unauthorized();

            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            return await UpdateUser(int.Parse(userId), new UserRequest
            {
                Email = request.Email,
                FullName = request.FullName,
                Password = request.Password,
                Role = role,
                IsActive = true,
            });
        }

        [HttpGet("self")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserResponse>> GetSelf()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Unauthorized();

            return await GetUserById(int.Parse(userId));
        }
    }

    public class SelfUpdateRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        public string Password { get; set; }
    }

    public class UserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserResponse
    {
        public UserResponse(User model)
        {
            Id = model.Id;
            Email = model.Email;
            FullName = model.FullName;
            Role = model.Role;
            IsActive = model.IsActive;
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
