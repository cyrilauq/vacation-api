namespace VacationApi.DTO
{
    public record ActivitiesDTO(
        string VacationId,
        ActivityDTO[] activities
    );
}
