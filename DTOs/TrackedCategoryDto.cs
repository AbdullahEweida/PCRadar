namespace PCRadar.DTOs
{
    // DTO لعرض البيانات
    public record TrackedCategoryDto(
        Guid Id,
        Guid StoreId,
        string StoreName,
        string CategoryUrl,
        string CategoryName // اسم الـ Enum هيتبعت كـ String
    );
}