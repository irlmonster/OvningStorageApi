using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorageApi.Data;
using StorageApi.DTO;
using StorageApi.DTOs;
using StorageApi.Models;

namespace StorageApi.Controllers;   

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly StorageContext _context;
    public ProductsController(StorageContext context)
    {
        _context = context;
    }

    // GET: api/Products?category=Verktyg&name=skruv
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? name)
    {
        var query = _context.Product.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        return await query
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Count = p.Count
            })
            .ToListAsync();
    }

    // GET: api/Product/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Product.FindAsync(id);
        if (product == null) return NotFound();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Count = product.Count
        };
    }

    // GET: api/Products/stats
    [HttpGet("stats")]
    public async Task<ActionResult<ProductStatsDto>> GetStats()
    {
        var products = _context.Product;

        if (!await products.AnyAsync())
            return new ProductStatsDto();

        return new ProductStatsDto
        {
            TotalProducts = await products.CountAsync(),
            TotalInventoryValue = await products.SumAsync(p => p.Price * p.Count),
            AveragePrice = await products.AverageAsync(p => p.Price)
        };
    }

    // PUT: api/Product/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, CreateProductDto dto)
    {
        var product = await _context.Product.FindAsync(id);
        if (product == null) return NotFound();

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Category = dto.Category;
        product.Shelf = dto.Shelf;
        product.Count = dto.Count;
        product.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST: api/Product
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<ProductDto>> PostProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Category = dto.Category,
            Shelf = dto.Shelf,
            Count = dto.Count,
            Description = dto.Description
        };

        _context.Product.Add(product);
        await _context.SaveChangesAsync();

        var result = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Count = product.Count
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, result);
    }

    // DELETE: api/Product/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Product.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Product.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductExists(int id)
    {
        return _context.Product.Any(e => e.Id == id);
    }
}
