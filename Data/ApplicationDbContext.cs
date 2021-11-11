using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyOnlineStoreAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }

    [Table("products")]
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(50)]     
        public string Name { get; set; }
    }

    [Table("users")]
    public class User
    {
        [StringLength(50)]
        public string Id { get; set; }
        [StringLength(50)]
        public string FullName { get; set; }
        [StringLength(50)]
        public string Email { get; set; }
        [StringLength(50)]
        public string PasswordHash { get; set; }
        [StringLength(50)]
        public string PasswordSalt { get; set; }
        [StringLength(50)]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;
    }
}