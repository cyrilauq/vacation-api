namespace VacationApi.DTO
{
    public class InvitationDTO
    {
        public bool IsAccepted { get; set; }
        public string? VacationName { get; set; }
        public string? InvitationId { get; set; }
        public string? VacationId { get; internal set; }
    }
}
