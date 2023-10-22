using Datadog.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using StoreApp.Models;
using StoreApp.Utilities;

namespace StoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _context;
        private readonly ICacheProvider _cacheProvider;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductContext context,
            ICacheProvider cacheProvider,
            ILogger<ProductsController> logger)
        {
            _context = context;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ProductContext.Products'  is null.");
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _cacheProvider.RemoveCache("cache_products");

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var cacheData = await _cacheProvider.GetCache<IEnumerable<Product>>("cache_products");

            if (cacheData != null)
                return cacheData.ToList();

            var products = await _context.Products.ToListAsync();

            cacheData = await _cacheProvider.SetCache("cache_products", products);

            if (cacheData == null)
                return NotFound();

            return cacheData.ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(long id)
        {
            _logger.LogInformation($"Test log from method get product {id}");

            var cacheData = await _cacheProvider.GetCache<Product>($"cache_product_{id}");

            if (cacheData != null)
                return cacheData;

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning($"Test log from method get product {id}: not found product");

                return NotFound();
            }

            cacheData = await _cacheProvider.SetCache($"cache_product_{id}", product);

            return cacheData;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(long id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                await _cacheProvider.RemoveCache($"cache_product_{id}");
                await _cacheProvider.RemoveCache("cache_products");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await _cacheProvider.RemoveCache($"cache_product_{id}");
            await _cacheProvider.RemoveCache("cache_products");

            return NoContent();
        }

        private bool ProductExists(long id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
