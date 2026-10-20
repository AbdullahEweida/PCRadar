namespace PCRadar.Services
{
    public interface IPriceScraperService
    {
        Task<List<ScrapedProduct>> ScrapeCategoryAsync(string categoryUrl, string cardXPath, string nameXPath, string priceXPath, string linkXPath, string availabilityXPath);
    }
}

