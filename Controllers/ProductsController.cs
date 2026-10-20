using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRadar.Data;
using PCRadar.DTOs;
using PCRadar.Models;

namespace PCRadar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. عرض المنتجات مع Pagination، بحث، وفلترة بالقسم
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse(category , true, out ProductCategory categoryEnum))
            {
                query = query.Where(p => p.Category == categoryEnum);
            }

            var totalItems = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                (
                    p.ID,
                    p.Name,
                    p.Category.ToString(),

                    _context.ProductStores
                        .Where(ps => ps.ProductID == p.ID)
                        .Select(ps => _context.PriceHistories
                            .Where(ph => ph.ProductStoreID == ps.ID)
                            .OrderByDescending(ph => ph.Date)
                            .Select(ph => (decimal?)ph.Price)
                            .FirstOrDefault())
                        .Min(),

                    _context.ProductStores.Count(ps => ps.ProductID == p.ID),

                    _context.ProductStores
                        .Where(ps => ps.ProductID == p.ID)
                        .Any(ps => _context.PriceHistories
                            .Where(ph => ph.ProductStoreID == ps.ID)
                            .OrderByDescending(ph => ph.Date)
                            .Select(ph => ph.IsAvailable)
                            .FirstOrDefault())
                ))
                .ToListAsync();

            return Ok(new { TotalItems = totalItems, Page = page, PageSize = pageSize, Data = products });
        }

        // 2. تفاصيل منتج واحد (بيعرض كل المتاجر اللي بتبيعه مترتبة من الأرخص للأغلى)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetails(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.ProductStores)
                    .ThenInclude(ps => ps.Store)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (product == null) return NotFound();

            var storeInfos = new List<ProductStoreInfoDto>();

            foreach (var ps in product.ProductStores)
            {
                var lastPrice = await _context.PriceHistories
                    .Where(ph => ph.ProductStoreID == ps.ID)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefaultAsync();

                if (lastPrice != null)
                {
                    storeInfos.Add(new ProductStoreInfoDto
                    (
                        ps.ID,
                        ps.Store.Name,
                        ps.ScrapeUrl,
                        lastPrice.Price,
                        lastPrice.IsAvailable,
                        lastPrice.Date
                    ));
                }
            }

            var dto = new ProductDetailsDto
            (
                product.ID,
                product.Name,
                product.Category.ToString(),
                storeInfos.OrderBy(ps => ps.CurrentPrice).ToList() // الترتيب من الأرخص للأغلى
            );

            return Ok(dto);
        }

        // 3. داتا الرسم البياني (Price History Chart) لمنتج في متجر معين
        [HttpGet("price-history/{productStoreId}")]
        public async Task<IActionResult> GetPriceHistory(Guid productStoreId)
        {
            var history = await _context.PriceHistories
                .Where(ph => ph.ProductStoreID == productStoreId)
                .OrderBy(ph => ph.Date) // الترتيب من القديم للجديد عشان الـ Chart يترسم صح
                .Select(ph => new PriceHistoryDto
                (
                    ph.Date,
                    ph.Price,
                    ph.IsAvailable
                ))
                .ToListAsync();

            return Ok(history);
        }
    }
}