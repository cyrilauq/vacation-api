namespace VacationApi.DTO
{
    public record MailDTO(
        String SenderName,
        String SenderFirstName,
        String SenderMail,
        String Message
    );
}
