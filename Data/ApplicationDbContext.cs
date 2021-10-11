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
        public int Id { get; set; }

        [StringLength(256)]
        [Required]
        public string Email { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        // HMAC-256 hash
        // Max Length = ceiling(256 / 3) * 4 = 44
        // https://stackoverflow.com/a/60067262/7003797
        [StringLength(44)]
        [Required]
        public string PasswordHash { get; set; }

        // 128-bit salt
        // Max Length = ceiling(128 / 3) * 4 = 24
        // https://stackoverflow.com/a/60067262/7003797
        [StringLength(24)]
        [Required]
        public string PasswordSalt { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;
    }
}