using PCRadar.Models;

namespace PCRadar.DTOs
{
    // DTO لصفحة تفاصيل المنتج الكاملة
    public record ProductDetailsDto(
        Guid Id,
        string Name,
        string Category,
        List<ProductStoreInfoDto> Stores
    );
}