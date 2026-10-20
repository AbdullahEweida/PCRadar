namespace PCRadar.DTOs
{
    public record CreateStoreDto(
        string Name,
        string BaseUrl,
        string CardXPath,
        string NameXPath,
        string PriceXPath,
        string LinkXPath,
        string AvailabilityXPath
    );
}
