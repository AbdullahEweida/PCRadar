namespace PCRadar.DTOs
{
    public record TestScraperRequestDto(string TestUrl, string CardXPath, string NameXPath, string PriceXPath, string LinkXPath, string AvailabilityXPath);
}
