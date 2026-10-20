namespace PCRadar.DTOs
{
    // DTO لنقاط الرسم البياني (Chart)
    public record PriceHistoryDto(
        DateTime Date,
        decimal Price,
        bool IsAvailable
    );
}