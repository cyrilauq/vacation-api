namespace VacationApi.DTO
{
    public record ActivityDTO(
        string? ActivityId,
        string Title,
        string Description,
        Double Longitude,
        Double Latitude,
        String Place,
        string? BeginDate = "",
        string? BeginTime = "",
        string? EndDate = "",
        string? EndTime = ""
    );
}
