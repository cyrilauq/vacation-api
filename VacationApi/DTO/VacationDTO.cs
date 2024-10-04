namespace VacationApi.DTO.Vacation
{
    record GetVacationsForUser(IEnumerable<VacationApi.Domains.Vacation> vacations);
    record AddVacationDto(bool isSuccess, string message);
    public record VacationDTO(
        string? Id,
        string? Title,
        string? Description,
        string? Place,
        double? Longitude,
        double? Latitude,
        string? DateBegin,
        string? DateEnd,
        string? TimeBegin,
        string? TimeEnd,
        string? PicturePath = null,
        string? OwnerName = "",
        Boolean? IsPublished = false
    );
}
