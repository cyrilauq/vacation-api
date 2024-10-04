using System.Collections;
using VacationApi.Domains;
using VacationApi.DTO.Vacation;

namespace VacationApi.DTO
{
    class DTOMapper
    {
        public static List<VacationDTO> VacationToVacationDTO(IEnumerable<Domains.Vacation> vacations)
        {
            List<VacationDTO> vacationsDTO = new List<VacationDTO>();

            foreach(Domains.Vacation vacation in vacations)
            {
                VacationDTO vacationDTO = new VacationDTO(
                    vacation.Id,
                    vacation.Title,
                    vacation.Description,
                    vacation.Place,
                    vacation.Longitude,
                    vacation.Latitude,
                    vacation.DateTimeBegin.Date.ToString(),
                    vacation.DateTimeEnd.Date.ToString(),
                    vacation.DateTimeBegin.TimeOfDay.ToString(),
                    vacation.DateTimeEnd.TimeOfDay.ToString());
                vacationsDTO.Add(vacationDTO);
            }

            return vacationsDTO;
        }

        public static List<ActivityDTO> ActivitiesToActivitiesDTO(IEnumerable<Domains.VacationActivity> activities)
        {
            List<ActivityDTO> activitiesDTO = new List<ActivityDTO>();

            foreach (Domains.VacationActivity activity in activities)
            {
                ActivityDTO vacationDTO = new ActivityDTO(
                    activity.Id,
                    activity.Name,
                    activity.Description, 
                    activity.Longitude, 
                    activity.Latitude, 
                    activity.Place,
                    activity.Begin?.Date.ToString("dd/MM/yyyy"),
                    activity.Begin?.ToString("HH:mm"),
                    activity.End?.Date.ToString("dd/MM/yyyy"),
                    activity.End?.ToString("HH:mm"));
                activitiesDTO.Add(vacationDTO);
            }

            return activitiesDTO;
        }

        public static List<UserSearchResultItemDTO> UsersToUsersDTO(IEnumerable<Domains.User> users)
        {
            List<UserSearchResultItemDTO> usersDTO = new List<UserSearchResultItemDTO>();

            foreach(Domains.User user in users)
            {
                UserSearchResultItemDTO userDTO = new UserSearchResultItemDTO(user.Name, user.FirstName, user.Id, user.Email);
                usersDTO.Add(userDTO);
            }

            return usersDTO;
        }
    }
}
