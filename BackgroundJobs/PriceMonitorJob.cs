using Microsoft.EntityFrameworkCore;
using PCRadar.Data;
using PCRadar.Services;

namespace PCRadar.Jobs
{
    public class PriceMonitorJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PriceMonitorJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"[PCRadar] Starting price check at {DateTime.UtcNow}...");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var scraperService = scope.ServiceProvider.GetRequiredService<IPriceScraperService>();
                    // حقن الـ Service الجديدة
                    var dataProcessor = scope.ServiceProvider.GetRequiredService<IScrapedDataProcessor>();

                    var categoriesToTrack = await dbContext.TrackedCategories
                                                .Include(c => c.Store)
                                                .ToListAsync(stoppingToken);

                    foreach (var category in categoriesToTrack)
                    {
                        var scrapedItems = await scraperService.ScrapeCategoryAsync(
                            category.CategoryUrl,
                            category.Store.CardXPath,
                            category.Store.NameXPath,
                            category.Store.PriceXPath,
                            category.Store.LinkXPath,
                            category.Store.AvailabilityXPath);

                        // نداء واحد بس للـ Service ينفذ كل حاجة!
                        await dataProcessor.ProcessAndSaveAsync(dbContext, scrapedItems, category.StoreID, category.Category, stoppingToken);
                    }
                }

                Console.WriteLine("[PCRadar] Price check finished. Sleeping for 12 hours.");
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
