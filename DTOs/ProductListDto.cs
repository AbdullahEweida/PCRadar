namespace PCRadar.DTOs
{
    // DTO لعرض المنتجات في الجدول الرئيسي
    public record ProductListDto(
        Guid Id,
        string Name,
        string Category,
        decimal? LowestPrice, // أقل سعر متاح حالياً للمنتج
        int StoresCount, // كام متجر بيبيع المنتج ده
        bool IsAvailable // هل المنتج متاح حالياً في أي متجر
        );
}