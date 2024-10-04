using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using VacationApi.Domains;
using VacationApi.Domains.Exceptions;
using VacationApi.DTO;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Model;
using VacationApi.Services;
using VacationApi.Utils;

namespace VacationApi.Repository
{
    // TODO : Where dbcontext use catch exception for dbcontext use
    public class VacationRepository: IVacationInfrastructure
    {
        private readonly VacationApiDbContext _dbContext;
        private DbSet<Vacation> Vacations { get => _dbContext.Vacations; }

        public VacationRepository(VacationApiDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Vacation>> GetVacations()
        {
            return await _dbContext.Vacations.ToListAsync();
        }

        public async Task<IEnumerable<Vacation>> GetVacationsForUser(string uid)
        {
            var invitations = (await _dbContext.Invitations.ToListAsync()).Where(invitation => invitation.UserId.Equals(uid) && invitation.IsAccepted);
            List<Vacation> vacations = Vacations.Where(x => x.UserId == uid).ToList();

            foreach(var invitation in invitations)
            {
                var vacation = (await _dbContext.Vacations.ToListAsync()).Where(vacation => vacation.Id.Equals(invitation.VacationId)).FirstOrDefault();
                if (vacation != null) 
                {
                    vacations.Add(vacation); 
                }
            }

            return vacations;
        }

        public Vacation? AddVacation(AddVacationModel vacation, string userId)
        {
            IFormatProvider provider = new CultureInfo("fr-FR");
            DateTime begin;
            DateTime end;
            double lon;
            double lat;
            try
            {
                begin = DateTime.Parse(vacation.DateBegin + " " + vacation.HourBegin, provider);
                end = DateTime.Parse(vacation.DateEnd + " " + vacation.HourEnd, provider);
            }
            catch (FormatException e)
            {
                throw new InvalidVacationInformation("One date (or the two) are not correctly formatted.");
            }
            try
            {
                lat = double.Parse(vacation.Latitude.Replace(".", ","));
                lon = double.Parse(vacation.Longitude.Replace(".", ","));
            }
            catch (FormatException e)
            {
                throw new InvalidVacationInformation("One coordinate (or the two) is not correctly formatted.");
            }

            if(ExistsFor(vacation.Title, userId))
            {
                throw new VacationAlreadyExistsException("A vacation with this title alredy exist.");
            }

            if (!isFree(begin, end, userId))
            {
                throw new PeriodNotFreeException("The given period isn't free, you need to choose a new one.");
            }
            var toAdd = Vacation.New(
                begin: begin,
                end: end,
                description: vacation.Description,
                title: vacation.Title,
                latitude: lat,
                longitude: lon,
                place: vacation.Place,
                userId: userId,
                picturePath: vacation.PicturePath,
                country: vacation.Country
            );
            var result = Vacations.Add(toAdd).Entity;
            _dbContext.SaveChanges();
            return result;
        }

        public bool ExistsFor(string title, string userId)
        {
            return Vacations.Where(x => x.Title == title && x.UserId == userId).ToList().Count != 0;
        }

        public bool isFree(DateTime from, DateTime to, string userId)
        {
            return Vacations.Where(x =>
                ((x.DateTimeBegin >= from || x.DateTimeEnd >= from) &&
                (x.DateTimeBegin <= to || x.DateTimeEnd <= to)) &&
                x.UserId == userId
            ).ToList().Count() == 0;
        }

        public Vacation GetVacationByIdFor(string id, string userId)
        {
            var result = _dbContext.Vacations.Where(vacation => vacation.Id == id).FirstOrDefault();
            if(result == null)
            {
                throw new VacationNotFoundException("The wanted vacation wasn't found");
            }
            if(!result.IsPublished && !UserCanSee(result, userId))
            {
                throw new CannotSeeVacationException("You can't see the vacation");
            }
            result.UserName = _dbContext.Users.Where(u => u.Id == result.UserId).FirstOrDefault().UserName;
            return result;
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
                _dbContext.Invitations.Count(a => a.VacationId == vacation.Id && a.UserId == userId && a.IsAccepted) > 0;
        }

        public List<Invitation> AddUsersToVacation(string vacationId, string userId, string[] usersUid)
        {
            if (_dbContext.Users.Count(user => user.Id == userId) == 0)
            {
                throw new UserNotFoundException("The user with " + userId + " uid doesn't exists.");
            }
            if (_dbContext.Vacations.Count(vacation => vacation.Id == vacationId) == 0)
            {
                throw new VacationNotFoundException("The vacation with " + vacationId + " id doesn't exists.");
            }
            try
            {
                var vacation = GetVacationByIdFor(vacationId, userId);
                var result = new List<Invitation>();

                if (vacation.IsPublished)
                {
                    throw new VacationPublishedException("The vacation is published iwch mean you can not modify it.");
                }

                foreach (var uid in usersUid)
                {
                    if(_dbContext.Invitations.Count(invit => invit.UserId == uid && invit.VacationId == vacationId) > 0)
                    {
                        continue;
                    }
                    var user = _dbContext.Users.Where(user => user.Id == uid).FirstOrDefault();
                    if (user is null)
                    {
                        throw new UserNotFoundException("The user with " + uid + " uid doesn't exists.");
                    }
                    var invit = _dbContext.Invitations.Add(
                        Invitation.New(uid, vacationId)
                    ).Entity;

                    result.Add(invit);
                }
                _dbContext.SaveChanges();

                return result;
            }
            catch(CannotSeeVacationException ex)
            {
                throw new WrongCredentialsException("You are not allowed to add user to this vacation.");
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new StockageException(e.Message);
            }
            catch (DbUpdateException e)
            {
                throw new StockageException(e.Message);
            }
        }

        // <summary>
        /// Get all invitations for a specific vacation
        /// </summary>
        /// <param name="id">Vacation ID</param>
        /// <returns></returns>
        public async Task<IEnumerable<Invitation>> GetInvitations(string id, string userId)
        {
            // Will verify that the user can see the vacation
            GetVacationByIdFor(id, userId);
            return (await _dbContext.Invitations.ToListAsync()).Where(i => i.VacationId == id).ToList();
        }

        public async Task<IEnumerable<UserDTO>> GetUsersForVacation(string vacationId, string userId)
        {
            Vacation vacation = Vacations.Where(vac => vac.Id == vacationId).FirstOrDefault();
            if (vacation == null)
            {
                throw new VacationNotFoundException("The wanted vacation wasn't found");
            }
            if(!UserCanSee(vacation, userId))
            {
                throw new WrongCredentialsException("You are not allowed to add user to this vacation.");
            }

            IEnumerable<Invitation> invitations = GetInvitations(vacationId, userId).Result;

            List<string> usersId = new List<string>
            {
                vacation.UserId
            };

            foreach (Invitation invitation in invitations)
            {
                // Avec un système d'invitation, il faudrait check l'état de l'invitation à true ici (champ isAccepted)
                if(!invitation.IsAccepted)
                {
                    continue;
                }
                usersId.Add(invitation.UserId);
            }

            List<UserDTO> users = new List<UserDTO>();
            foreach (string uid in usersId)
            {
                User user = (await _dbContext.Users.ToListAsync()).Where(user => user.Id == uid).FirstOrDefault();
                if (user != null)
                {

                    users.Add(new UserDTO
                    {
                        Firstname = user.FirstName,
                        Name = user.Name,
                        Mail = user.Email,
                        Uid = user.Id,
                        ImgPath = user.PicturePath,
                        Username = user.UserName
                    });
                }
            }

            return users;
        }

        public bool PublishVacation(string vacationId, string userId)
        {
            var result = GetVacationById(vacationId, userId);
            if (result is null)
            {
                throw new VacationNotFoundException("The wanted vacation wasn't found");
            }
            if(userId != result.UserId)
            {
                throw new WrongCredentialsException("You don't have the permissions to modify the vacation");
            }
            if(result.IsPublished)
            {
                throw new VacationPublishedException("The vacation is already published.");
            }
            result.IsPublished = true;
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e) when (e is DbUpdateConcurrencyException || e is DbUpdateException)
            {
                throw new VacationActivitySaveExceptions("An error occurs while publishing the vacation");
            }
            return true;
        }

        private Vacation GetVacationById(string vacationId, string userId)
        {
            var vacation = Vacations.Where(vac => vac.Id == vacationId).FirstOrDefault();
            if (vacation is null)
            {
                throw new VacationNotFoundException("The wanted vacation wasn't found");
            }
            if (!UserCanSee(vacation, userId))
            {
                throw new WrongCredentialsException("You are not allowed to add user to this vacation.");
            }
            vacation.UserName = _dbContext.Users.Where(u => u.Id == vacation.UserId).FirstOrDefault().UserName;
            return vacation;
        }

        public Boolean AcceptInvitation(String invitationId, String userId)
        {
            var invitation = _dbContext.Invitations.Where(i => i.Id == invitationId).FirstOrDefault();
            if(invitation is null)
            {
                throw new VacationNotFoundException("The given id is not related to any existing invitation.");
            }
            if(userId != invitation.UserId)
            {
                throw new WrongCredentialsException("You can't accept the invitation of someone else.");
            }
            var vacation = _dbContext.Vacations.Where(v => v.Id == invitation.VacationId).FirstOrDefault();
            if (vacation is null)
            {
                throw new VacationNotFoundException("The invitation is not related to any existing vacation.");
            }
            if (vacation.IsPublished)
            {
                throw new VacationPublishedException("The invitation can not be accepted because the vacation is already published.");
            }
            try {
                invitation.IsAccepted = true;
                _dbContext.Update(invitation);
                _dbContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new StockageException("An error occured while accepting the invitation");
            }
        }
    }
}
