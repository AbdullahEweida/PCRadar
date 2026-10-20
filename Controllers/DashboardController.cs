using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRadar.Data;
using PCRadar.DTOs;

namespace PCRadar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // 1. استرجاع الأرقام والإحصائيات الرئيسية
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalStores = await _context.Stores.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalTracked = await _context.ProductStores.CountAsync();

            // حساب التحديثات اللي حصلت النهاردة بس
            var today = DateTime.UtcNow.Date;
            var updatesToday = await _context.PriceHistories
                .Where(ph => ph.Date >= today)
                .CountAsync();

            var stats = new DashboardStatsDto(
                TotalStores: totalStores,
                TotalProducts: totalProducts,
                TotalTrackedItems: totalTracked,
                UpdatesToday: updatesToday
            );

            return Ok(stats);
        }

        // 2. استرجاع أحدث 10 أسعار أو تغيرات حصلت في السيستم
        [HttpGet("recent-updates")]
        public async Task<IActionResult> GetRecentUpdates()
        {
            var recentUpdates = await _context.PriceHistories
                .Include(ph => ph.ProductStore)
                    .ThenInclude(ps => ps.Product)
                .Include(ph => ph.ProductStore)
                    .ThenInclude(ps => ps.Store)
                .OrderByDescending(ph => ph.Date)
                .Take(10)
                .Select(ph => new RecentUpdateDto
                (
                    ph.ID,
                    ph.ProductStore.Product.Name,
                    ph.ProductStore.Store.Name,
                    ph.ProductStore.Product.Category.ToString(),
                    ph.Price,
                    ph.IsAvailable,
                    ph.Date,
                    ph.ProductStore.ScrapeUrl
                ))
                .ToListAsync();

            return Ok(recentUpdates);
        }
    }
}