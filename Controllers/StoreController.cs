using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRadar.Data;
using PCRadar.Models;
using PCRadar.DTOs;
using HtmlAgilityPack;
using PCRadar.Services;

namespace PCRadar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory; // ضفنا دي

        public StoresController(AppDbContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }

        // 1. استرجاع كل المتاجر لعرضها في القائمة الجانبية
        [HttpGet]
        public async Task<IActionResult> GetStores()
        {
            var stores = await _context.Stores
                .Select(s => new StoreDto
                (
                    s.ID,
                    s.Name,
                    s.WebsiteUrl,
                    s.CardXPath,
                    s.NameXPath,
                    s.PriceXPath,
                    s.LinkXPath,
                    s.AvailabilityXPath,
                    "active",
                    _context.ProductStores.Count(ps => ps.StoreID == s.ID),
                    "Just now" // ممكن نجيبها من آخر PriceHistory
                ))
                .ToListAsync();

            return Ok(stores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStore(Guid id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return NotFound();

            var dto = new StoreDto
            (
                store.ID,
                store.Name,
                store.WebsiteUrl,
                store.CardXPath,
                store.NameXPath,
                store.PriceXPath,
                store.LinkXPath,
                store.AvailabilityXPath,
                "active",
                await _context.ProductStores.CountAsync(ps => ps.StoreID == store.ID),
                "Just now"
            );

            return Ok(dto);
        }

        // 2. إضافة متجر جديد
        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] CreateStoreDto dto) // التعديل هنا
        {
            var store = new Store
            {
                Name = dto.Name,
                WebsiteUrl = dto.BaseUrl,
                CardXPath = dto.CardXPath,
                NameXPath = dto.NameXPath,
                PriceXPath = dto.PriceXPath,
                LinkXPath = dto.LinkXPath,
                AvailabilityXPath = dto.AvailabilityXPath
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            // هنرجع StoreDto كامل عشان الـ Frontend يعرضه في الجدول
            var returnDto = new StoreDto
            (
                store.ID,
                store.Name,
                store.WebsiteUrl,
                store.CardXPath,
                store.NameXPath,
                store.PriceXPath,
                store.LinkXPath,
                store.AvailabilityXPath,
                "active",
                0,
                "Just now"
            );

            return CreatedAtAction(nameof(GetStore), new { id = store.ID }, returnDto);
        }
        // 3. تعديل متجر
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStore(Guid id, [FromBody] CreateStoreDto dto)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return NotFound();

            store.Name = dto.Name;
            store.WebsiteUrl = dto.BaseUrl;
            store.CardXPath = dto.CardXPath;
            store.NameXPath = dto.NameXPath;
            store.PriceXPath = dto.PriceXPath;
            store.LinkXPath = dto.LinkXPath;
            store.AvailabilityXPath = dto.AvailabilityXPath;

            await _context.SaveChangesAsync();

            // خد نسخة من القيم قبل الـ Task.Run
            var (card, name, price, link, avail) =
                (store.CardXPath, store.NameXPath, store.PriceXPath, store.LinkXPath, store.AvailabilityXPath);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var backgroundDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var scraper = scope.ServiceProvider.GetRequiredService<IPriceScraperService>();
                    var processor = scope.ServiceProvider.GetRequiredService<IScrapedDataProcessor>();

                    var cats = await backgroundDb.TrackedCategories.Where(c => c.StoreID == id).ToListAsync();
                    foreach (var c in cats)
                    {
                        var items = await scraper.ScrapeCategoryAsync(c.CategoryUrl, card, name, price, link, avail);
                        await processor.ProcessAndSaveAsync(backgroundDb, items, id, c.Category, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UpdateStore background] {ex}"); // من غيرها الأخطاء بتضيع
                }
            });

            return Ok(new StoreDto(store.ID, store.Name, store.WebsiteUrl, store.CardXPath, store.NameXPath,
                store.PriceXPath, store.LinkXPath, store.AvailabilityXPath, "active",
                await _context.ProductStores.CountAsync(ps => ps.StoreID == id), "Just now"));
        }        // 4. حذف متجر
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(Guid id)
        {
            var store = await _context.Stores
                .Include(s => s.ProductStores)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (store == null) return NotFound();

            var productIdsToCheck = store.ProductStores.Select(ps => ps.ProductID).ToList();

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();

            if (productIdsToCheck.Any())
            {
                var activeProductIds = await _context.ProductStores
                    .Where(ps => productIdsToCheck.Contains(ps.ProductID))
                    .Select(ps => ps.ProductID)
                    .Distinct()
                    .ToListAsync();

                var orphanProductIds = productIdsToCheck.Except(activeProductIds).ToList();

                if (orphanProductIds.Any())
                {
                    var orphanProducts = await _context.Products
                        .Where(p => orphanProductIds.Contains(p.ID))
                        .ToListAsync();

                    _context.Products.RemoveRange(orphanProducts);
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        // 5. نقطة فحص الـ Scraper (الميزة الذكية)
        [HttpPost("test-scraper")]
        public async Task<IActionResult> TestScraper([FromBody] TestScraperRequestDto request)
        {
            try
            {
                var web = new HtmlWeb();

                web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                var document = await web.LoadFromWebAsync(request.TestUrl);
                var productCards = document.DocumentNode.SelectNodes(request.CardXPath);

                if (productCards == null || productCards.Count == 0)
                {
                    Console.WriteLine(document.DocumentNode.OuterHtml.Substring(0, Math.Min(500, document.DocumentNode.OuterHtml.Length)));
                    return Ok(new { status = "error", message = "Scraper test failed — check Card XPath selectors." });
                }

                var firstCard = productCards.FirstOrDefault();
                var nameNode = firstCard?.SelectSingleNode(request.NameXPath);
                var priceNode = firstCard?.SelectSingleNode(request.PriceXPath);
                var availabilityNode = firstCard?.SelectSingleNode(request.AvailabilityXPath);

                if (nameNode == null || priceNode == null)
                {
                    return Ok(new { status = "error", message = $"Cards found, but Name or Price XPath is wrong." });
                }

                if (availabilityNode == null)
                { 
                    return Ok(new { status = "error", message = $"Cards found, but Availability XPath is wrong." });
                }

                return Ok(new { status = "success", message = $"Scraper test passed — {productCards.Count} products found." });
            }
            catch (Exception ex)
            {
                return Ok(new { status = "error", message = $"Error connecting to URL: {ex.Message}" });
            }
        }
    }
}