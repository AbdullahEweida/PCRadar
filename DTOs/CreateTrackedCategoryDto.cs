namespace PCRadar.DTOs
{
    // DTO لإضافة قسم جديد
    public record CreateTrackedCategoryDto(
        Guid StoreId,
        string CategoryUrl,
        string CategoryName // مثلاً: "GPU", "CPU"
    );
}