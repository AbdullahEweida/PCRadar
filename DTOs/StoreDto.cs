namespace PCRadar.DTOs
{
    public record StoreDto(Guid Id, string Name, string BaseUrl, string CardXPath, string NameXPath, string PriceXPath, string LinkXPath, string AvailabilityXPath, string Status, int ProductsTracked, string LastSync);
}
