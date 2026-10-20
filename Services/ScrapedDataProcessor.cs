using Microsoft.EntityFrameworkCore;
using PCRadar.Data;
using PCRadar.Models;

namespace PCRadar.Services
{
    public interface IScrapedDataProcessor
    {
        Task ProcessAndSaveAsync(AppDbContext dbContext, List<ScrapedProduct> scrapedItems, Guid storeId, ProductCategory category, CancellationToken cancellationToken = default);
    }

    public class ScrapedDataProcessor : IScrapedDataProcessor
    {
        public async Task ProcessAndSaveAsync(AppDbContext dbContext, List<ScrapedProduct> scrapedItems, Guid storeId, ProductCategory category, CancellationToken cancellationToken = default)
        {
            var distinctItems = scrapedItems.GroupBy(x => x.Name).Select(x => x.First()).ToList();

            foreach (var item in distinctItems)
            {
                try
                {
                    var productToUse = await dbContext.Products
                        .FirstOrDefaultAsync(p => p.Name == item.Name, cancellationToken)
                        ?? dbContext.Products.Local.FirstOrDefault(p => p.Name == item.Name);

                    if (productToUse == null)
                    {
                        productToUse = new Product
                        {
                            Name = item.Name,
                            Category = category
                        };
                        dbContext.Products.Add(productToUse);
                    }

                    var existingProductStore = await dbContext.ProductStores
                        .FirstOrDefaultAsync(ps => ps.ProductID == productToUse.ID && ps.StoreID == storeId, cancellationToken)
                        ?? dbContext.ProductStores.Local.FirstOrDefault(ps => ps.Product == productToUse && ps.StoreID == storeId);

                    if (existingProductStore == null)
                    {
                        existingProductStore = new ProductStore
                        {
                            Product = productToUse,
                            StoreID = storeId,
                            ScrapeUrl = item.ProductUrl
                        };
                        dbContext.ProductStores.Add(existingProductStore);
                    }
                    else if (existingProductStore.ScrapeUrl != item.ProductUrl)
                    {
                        existingProductStore.ScrapeUrl = item.ProductUrl;
                    }

                    var lastPrice = await dbContext.PriceHistories
                        .Where(ph => ph.ProductStoreID == existingProductStore.ID || ph.ProductStore == existingProductStore)
                        .OrderByDescending(ph => ph.Date)
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? dbContext.PriceHistories.Local
                            .Where(ph => ph.ProductStoreID == existingProductStore.ID || ph.ProductStore == existingProductStore)
                            .OrderByDescending(ph => ph.Date)
                            .FirstOrDefault();

                    if (lastPrice == null || lastPrice.Price != item.Price || lastPrice.IsAvailable != item.IsAvailable)
                    {
                        dbContext.PriceHistories.Add(new PriceHistory
                        {
                            ProductStore = existingProductStore,
                            Price = item.Price,
                            IsAvailable = item.IsAvailable,
                            Date = DateTime.UtcNow
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ScrapedDataProcessor] Error saving product '{item.Name}': {ex.Message}");
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}