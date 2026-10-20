namespace PCRadar.DTOs
{
    public record DashboardStatsDto(
        int TotalStores,
        int TotalProducts,
        int TotalTrackedItems,
        int UpdatesToday
    );
}