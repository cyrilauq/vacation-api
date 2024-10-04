namespace VacationApi.Infrastructure.Args
{
    /// <summary>
    /// Record used to plannify an activity
    /// </summary>
    /// <param name="ActivityId">Id of the activity to plannify</param>
    /// <param name="DateBegin">Date of begin of the activity</param>
    /// <param name="TimeBegin">Time of begin of the activity</param>
    /// <param name="DateEnd">Date of end of the activity</param>
    /// <param name="TiemEnd">Time of end of the activity</param>
    public record PlannifyArgs(
        string ActivityId,
        string DateBegin,
        string TimeBegin,
        string DateEnd,
        string TimeEnd
    );
}
