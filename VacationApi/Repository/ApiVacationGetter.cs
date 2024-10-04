using Microsoft.EntityFrameworkCore;
using VacationApi.Domains;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Exceptions;

namespace VacationApi.Repository
{
    public class ApiVacationGetter : IVacationGetter
    {
        private readonly VacationApiDbContext _context;

        public ApiVacationGetter(VacationApiDbContext context)
        {
            _context = context;
        }

        public Vacation? GetVacationById(string id, string userId)
        {
            var result = _context.Vacations.Where(vacation => vacation.Id == id).FirstOrDefault();
            if (result == null)
            {
                throw new VacationNotFoundException("The wanted vacation wasn't found");
            }
            if (!result.IsPublished && !UserCanSee(result, userId))
            {
                throw new CannotSeeVacationException("You can't see the vacation");
            }
            result.UserName = _context.Users.Where(u => u.Id == result.UserId).FirstOrDefault().UserName;
            return result;
        }
        private Boolean UserCanSee(Vacation vacation, string userId)
        {
            return vacation.UserId == userId ||
                _context.Invitations.Count(a => a.VacationId == vacation.Id && a.UserId == userId) > 0;
        }
    }
}
