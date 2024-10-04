using VacationApi.DTO;

namespace VacationApi.Model
{
    public record AddMembersVacationModel(
        UserSearchResultItemDTO[] members,
        int count
    );
}
