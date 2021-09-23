using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyOnlineStoreAPI.Data;
using MyOnlineStoreAPI.Helpers;

namespace MyOnlineStoreAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public ProductsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Authorize (Policy = Permissions.ProductsList)]
        [HttpGet]
        public async Task<Page<Product>> GetAllProducts(
            string name = "", int pageIndex = 0, int pageSize = 3)
        {
            pageSize = Math.Min(pageSize, 25);

            var query = _dbContext.Products
                .Where(p => p.Name.Contains(name));

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Page<Product>
            {
                PageIndex = pageIndex,
                Items = products.ToList(),
                TotalCount = totalCount
            };
        }

        [Authorize (Policy = Permissions.ProductsGet)]
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            return product;
        }

        [Authorize (Policy = Permissions.ProductsCreate)]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Product>> CreateProduct(ProductRequest request)
        {
            var product = request.ToModel();

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return Created(Url.Action(nameof(GetProductById), new { id = product.Id } ), product);
        }

        [Authorize (Policy = Permissions.ProductsUpdate)]
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Product>> UpdateProduct(int id, ProductRequest request)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound(); 

            product.Name = request.Name;

            await _dbContext.SaveChangesAsync();

            return product;
        }

        [Authorize (Policy = Permissions.ProductsDelete)]
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound(); 

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            
            return Ok();
        }
    }

    public class ProductRequest : IValidatableObject
    {
        public string Name { get; set; }

        public Product ToModel()
        {
            return new Product
            {
                Name = Name
            };
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
                yield return new ValidationResult("Name can't be empty.");

            if (Name?.Length < 5)
                yield return new ValidationResult("Name must be at least 5 characters.");
        }
    }

    public class Page<T>
    {
        public List<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int TotalCount { get; set; }
    }
}