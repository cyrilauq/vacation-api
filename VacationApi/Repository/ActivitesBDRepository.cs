using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using VacationApi.Auth;
using VacationApi.Domains;
using VacationApi.Domains.Exceptions;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Args;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Utils;

namespace VacationApi.Repository
{
    public class ActivitesBDRepository: IVacationActivitiesInfrastructure
    {
        private readonly VacationApiDbContext _context;
        private readonly IVacationGetter _vacationGetter;

        public ActivitesBDRepository(VacationApiDbContext context, IVacationGetter vacationGetter) 
        {
            _context = context;
            _vacationGetter = vacationGetter;
        }

        public List<VacationActivity> AddActivitiesToVacation(string vacationId, List<ActivityDTO> activities, string userId)
        {
            var vacation = _vacationGetter.GetVacationById(vacationId, userId);
            if(vacation.IsPublished)
            {
                throw new VacationPublishedException("You can't add activities to a published vacation");
            }
            if(!UserCanSee(vacation, userId))
            {
                throw new WrongCredentialsException("You can't add activities to a vacation you doesn't own.");
            }

            try
            {
                List<VacationActivity> result = new List<VacationActivity>();
                foreach (var activity in activities)
                {
                    if(result.Count(a => a.Name == activity.Title) != 0)
                    {
                        throw new ActivityAlreadyExistException("An activity with the name: " + activity.Title + "already exists.");
                    }

                     result.Add(_context.Add(
                        VacationActivity.New(activity.Title, activity.Description, activity.Longitude, activity.Latitude, activity.Place, vacationId)
                    ).Entity);
                }
                _context.SaveChanges();
                return result;
            }
            catch (VacationActivityFormatExceptions ex)
            {
                throw new VacationActivitySaveExceptions(ex.Message);
            }
            catch (Exception e) when (e is DbUpdateConcurrencyException|| e is DbUpdateException)
            {
                throw new VacationActivitySaveExceptions("An error occurs while saving the activities");
            }
        }

        public List<VacationActivity> GetActivitiesForVacation(string vacationId, string userId)
        {
            // TODO : Verif that the user is authorized to see the vacation
            var vacation = _vacationGetter.GetVacationById(vacationId, userId);
            var activities = _context.Activities.Where(activity => activity.VacationId == vacationId);

            return activities.ToList();
        }

        public VacationActivity PlannifyActivity(PlannifyArgs args, string userId)
        {
            var activity = _context.Activities.Count() == 0 ? null : _context.Activities.Where(a => a.Id == args.ActivityId).First();
            if(activity == null)
            {
                throw new ActivityNotFoundException("The given activity wasn't found");
            }
            var vacation = _vacationGetter.GetVacationById(activity.VacationId, userId);
            if(!UserCanSee(vacation, userId))
            {
                throw new WrongCredentialsException("You don't have the right to perform this action");
            }
            if (vacation.IsPublished) 
            {
                throw new VacationPublishedException("You cannot plannify an activity for a vacation that is already published.");
            }
            IFormatProvider provider = new CultureInfo("fr-FR");
            try
            {
                activity.Begin = DateTime.Parse(args.DateBegin + " " + args.TimeBegin, provider);
                activity.End = DateTime.Parse(args.DateEnd + " " + args.TimeEnd, provider);
            }
            catch (FormatException e)
            {
                throw new InvalidVacationInformation("One date (or the two) are not correctly formatted.");
            }
            if (!isFree(activity.Begin, activity.End, activity.VacationId, activity.Id))
            {
                throw new PeriodNotFreeException("There is already a vacation that is in the given period.");
            }

            if (activity.Begin >= activity.End)
            {
                throw new InvalidVacationInformation("The begin date has to be greater than the end date.");
            }

            try
            {
                var result = _context.Update(activity);
                _context.SaveChanges();

                return result.Entity;
            }     
            catch (Exception e) when(e is DbUpdateConcurrencyException || e is DbUpdateException)
            {
                throw new VacationActivitySaveExceptions("An error occurs while saving the activities");
            }
        }

        public bool isFree(DateTime? from, DateTime? to, string vacationId, string activityId)
        {
            return _context.Activities.Where(x =>
                ((x.Begin >= from || x.End >= from) &&
                (x.Begin <= to || x.End <= to)) &&
                x.VacationId == vacationId &&
                x.Id != activityId
            ).ToList().Count() == 0;
        }

        /// <summary>
        /// Verifiy that the user can see the given vacation
        /// </summary>
        /// <param name="vacation">The vacation that the user want to see</param>
        /// <param name="userId">id of the user.</param>
        /// <returns>
        /// True if the user can see the vacation so if he is the owner or is a member of it.
        /// False otherwise
        /// </returns>
        private Boolean UserCanSee(Vacation vacation, string userId)
        {
            return vacation.UserId == userId ||
                _context.Invitations.Count(a => a.VacationId == vacation.Id && a.UserId == userId && a.IsAccepted) > 0;
        }

    }
}
