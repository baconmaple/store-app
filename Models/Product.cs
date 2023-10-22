using System.ComponentModel.DataAnnotations;

namespace StoreApp.Models;

public class Product
{
    [Key]
    public long Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}
