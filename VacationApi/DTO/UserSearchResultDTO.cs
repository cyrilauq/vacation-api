namespace VacationApi.DTO
{
    public record UserSearchResultDTO(
        int count,
        UserSearchResultItemDTO[] result
    );
}
