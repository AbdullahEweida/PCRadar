namespace PCRadar.DTOs
{
    // DTO لعرض بيانات المتجر جوه صفحة تفاصيل المنتج
    public record ProductStoreInfoDto(
        Guid ProductStoreId,
        string StoreName,
        string ScrapeUrl,
        decimal CurrentPrice,
        bool IsAvailable,
        DateTime LastUpdated
    );
}