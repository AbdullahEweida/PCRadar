namespace PCRadar.DTOs
{
    public record RecentUpdateDto(
        Guid PriceHistoryId,
        string ProductName,
        string StoreName,
        string Category,
        decimal Price,
        bool IsAvailable,
        DateTime Date,
        string ProductUrl
    );
}