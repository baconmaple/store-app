using Microsoft.EntityFrameworkCore;

namespace StoreApp.Models;

public class ProductContext : DbContext
{
    public ProductContext(DbContextOptions<ProductContext> options)
        : base(options)
    { }

    public DbSet<Product> Products { get; set; } = null!;
}
