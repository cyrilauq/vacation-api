using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System.Globalization;
using VacationApi.Domains.Exceptions;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Args;
using VacationApi.Utils;

namespace VacationApi.Services
{
    public class ActivitiesServices
    {
        private readonly IVacationInfrastructure _context;
        private readonly IVacationActivitiesInfrastructure _activityRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivitiesServices(IVacationInfrastructure context, IVacationActivitiesInfrastructure activityRepo, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _activityRepo = activityRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public List<ActivityDTO> GetActivitiesForVacation(string id) => DTOMapper.ActivitiesToActivitiesDTO(_activityRepo.GetActivitiesForVacation(id, _httpContextAccessor.ConnectedUserId()));

        public List<ActivityDTO> GetPlanningForVacation(string id) => DTOMapper.ActivitiesToActivitiesDTO(_activityRepo.GetActivitiesForVacation(id, _httpContextAccessor.ConnectedUserId()).Where(a => a.Begin != null));

        public ActivitiesDTO AddActivitiesToVacation(string vacationId, ActivityDTO[] activities)
        {
            return new ActivitiesDTO(
                vacationId,
                _activityRepo.AddActivitiesToVacation(
                    vacationId,
                    activities.ToList(),
                    _httpContextAccessor.ConnectedUserId()
                )
                    .Select(a => new ActivityDTO(
                        a.Id,
                        a.Name,
                        a.Description,
                        a.Longitude,
                        a.Latitude,
                        a.Place
                    )).ToArray()
            );
        }

        public Ical.Net.Calendar GetCalendarForVacation(string id)
        {
            var planning = _activityRepo.GetActivitiesForVacation(id, _httpContextAccessor.ConnectedUserId()).Where(a => a.Begin != null);
            var calendar = new Ical.Net.Calendar
            {
                Method = "PUBLISH"
            };
            foreach (var item in planning)
            {
                var calendarEvent = new CalendarEvent
                {
                    Description = item.Description,
                    End = new CalDateTime((DateTime)item.End),
                    Location = item.Place,
                    Start = new CalDateTime((DateTime)item.Begin)
                };
                calendar.Events.Add(calendarEvent);
            }
            return calendar;
        }

        public ActivityDTO PlannifyActivity(string activityId, PlannifyActivityDTO dto)
        {
            IFormatProvider provider = new CultureInfo("fr-FR");
            try
            {
                var begin = DateTime.Parse(dto.dateTimeBegin, provider);
                var end = DateTime.Parse(dto.dateTimeEnd, provider);
                var result = _activityRepo.PlannifyActivity(
                    new PlannifyArgs(
                        activityId,
                        begin.Date.ToString("dd/MM/yyyy"),
                        begin.ToString("HH:mm"),
                        end.Date.ToString("dd/MM/yyyy"),
                        end.ToString("HH:mm")
                    ),
                    _httpContextAccessor.ConnectedUserId()
                );
                return new ActivityDTO(
                    result.Id,
                    result.Name,
                    result.Description,
                    result.Longitude,
                    result.Latitude,
                    result.Place,
                    result.Begin?.Date.ToString("dd/MM/yyyy"),
                    result.Begin?.ToString("HH:mm"),
                    result.End?.Date.ToString("dd/MM/yyyy"),
                    result.End?.ToString("HH:mm")
                );
            }
            catch (FormatException e)
            {
                throw new InvalidVacationInformation("One date (or the two) are not correctly formatted.");
            }
        }
    }
}
