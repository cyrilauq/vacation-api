namespace VacationApi.DTO
{
    public record AddMembersDto(
        String VacationId,
        String[] MembersUid
    );
}
