using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRadar.Data;
using PCRadar.Models;
using PCRadar.DTOs;
using System.ComponentModel;
using PCRadar.Services;

namespace PCRadar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;

        public CategoriesController(AppDbContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }

        // 1. استرجاع كل لينكات الأقسام الخاصة بمتجر معين
        [HttpGet("store/{storeId}")]
        public async Task<IActionResult> GetCategoriesByStore(Guid storeId)
        {
            var categories = await _context.TrackedCategories
                .Include(c => c.Store)
                .Where(c => c.StoreID == storeId)
                .Select(c => new TrackedCategoryDto
                (
                    c.ID,
                    c.StoreID,
                    c.Store.Name,
                    c.CategoryUrl,
                    c.Category.ToString() // بنحول الـ Enum لـ String عشان يتقرأ في الـ JSON
                ))
                .ToListAsync();

            return Ok(categories);
        }

        // 2. إضافة لينك قسم جديد لمتجر (مثلاً: لينك كروت الشاشة لمتجر البدر)
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CreateTrackedCategoryDto dto)
        {
            var store = await _context.Stores.FindAsync(dto.StoreId);

            if (store == null) return NotFound(new { message = "Store not found" });

            // بنحاول نحول الكلمة اللي مبعوتة (زي "GPU") للـ Enum بتاعك 
            // تأكد إن اسم الـ Enum في الموديل عندك اسمه Category
            if (!Enum.TryParse(dto.CategoryName, true, out ProductCategory categoryEnum))
            {
                return BadRequest(new { message = $"Invalid category type: {dto.CategoryName}" });
            }

            var newCategory = new TrackedCategory
            {
                ID = Guid.NewGuid(),
                StoreID = dto.StoreId,
                CategoryUrl = dto.CategoryUrl,
                Category = categoryEnum
            };

            _context.TrackedCategories.Add(newCategory);
            await _context.SaveChangesAsync();
            _ = Task.Run(async () =>
            {
            // بنعمل Scope جديد عشان الـ Background task ميتأثرش بانتهاء الـ HTTP Request
                using var scope = _scopeFactory.CreateScope();
                var backgroundDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var scraperService = scope.ServiceProvider.GetRequiredService<IPriceScraperService>();
                var dataProcessor = scope.ServiceProvider.GetRequiredService<IScrapedDataProcessor>();
                var scrapedItems = await scraperService.ScrapeCategoryAsync(
                    newCategory.CategoryUrl, 
                    store.CardXPath,
                    store.NameXPath,
                    store.PriceXPath,
                    store.LinkXPath,
                    store.AvailabilityXPath
                    );
                await dataProcessor.ProcessAndSaveAsync(backgroundDb, scrapedItems, store.ID, newCategory.Category, CancellationToken.None);
            });
            return Ok(new { message = "Category URL added successfully to track list", id = newCategory.ID });
        }

        // 3. حذف لينك قسم من التتبع
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.TrackedCategories.FindAsync(id);
            if (category == null) return NotFound();

            _context.TrackedCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}