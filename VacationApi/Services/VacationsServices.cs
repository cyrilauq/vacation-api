using FluentFTP.Helpers;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationApi.Auth;
using VacationApi.Domains;
using VacationApi.DTO;
using VacationApi.DTO.Vacation;
using VacationApi.Infrastructure;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Model;
using VacationApi.Utils;

namespace VacationApi.Services
{
    /// <summary>
    /// Service that will define methods for vacation manipulations.
    /// </summary>
    public class VacationsServices
    {
        private readonly IVacationInfrastructure _vacationRepository;
        private readonly IUserInfrastructure _userRepository;
        private readonly VacationApiDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FileService _fileService;
        private readonly IEmailSender _mailSender;

        public VacationsServices(VacationApiDbContext context, 
            IVacationInfrastructure vacationRepository, 
            IUserInfrastructure userRepository, 
            IHttpContextAccessor httpContextAccessor,
            FileService fileService,
            IEmailSender mailSender) 
        {
            _vacationRepository = vacationRepository;
            _userRepository = userRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;
            _mailSender = mailSender;
        }

        public Dictionary<string, int> GetUserByPlaceForDate(string date)
        {
            if (date == null || String.IsNullOrEmpty(date))
            {
                throw new ArgumentException("Champ de recherche de date obligatoire");
            }

            string dateStringEarliest = date + " 23:59:59";
            DateTime? dateEarliest = null;
            DateTime? dateLatest = null;
            try
            {
                dateEarliest = DateTime.Parse(dateStringEarliest);
                dateLatest = DateTime.Parse(date);
            }
            catch
            {
                throw new ArgumentException("Le format de date n'est pas bon. Format : dd/MM/yyyy");
            }

            Dictionary<string, int> dict = new();
            IEnumerable<Vacation> vacations = _context.Vacations;
            vacations = vacations.Where(v => dateEarliest > v.DateTimeBegin && dateLatest < v.DateTimeEnd).ToList();

            foreach (Vacation vacation in vacations)
            {
                string place = vacation.Country.ToLower();
                int nbOfUsers = _context.Invitations.Where(i => i.VacationId == vacation.Id).Count();

                if (dict.ContainsKey(place))
                {
                    dict[place] += nbOfUsers;
                }
                else
                {
                    dict[place] = nbOfUsers + 1;
                }
            }

            return dict;
        }

        public async Task<IEnumerable<UserDTO>> GetUsersForVacation(string vacationId) => await _vacationRepository.GetUsersForVacation(vacationId, _httpContextAccessor.ConnectedUserId());

        /// <summary>
        /// Retrieve a DTO of the vacation that is related to the given id.
        /// </summary>
        /// <param name="vacationId">Id of the vacation</param>
        /// <returns>
        /// A DTO of the vacation related to the given id.
        /// </returns>
        /// <exception cref="CannotSeeVacationException">If the user can not see the vacation</exception>
        /// <exception cref="VacationNotFoundException">If there is no vacation related to the given id</exception>
        public VacationDTO GetVacationById(string vacationId)
        {
            var result = _vacationRepository.GetVacationByIdFor(vacationId, _httpContextAccessor.ConnectedUserId());
           
            return new VacationDTO(
                Id: result.Id,
                Title: result.Title,
                Description: result.Description,
                Place: result.Place,
                Longitude: result.Longitude,
                Latitude: result.Latitude,
                DateBegin: result.DateTimeBegin.ToString("dd/MM/yyyy"),
                DateEnd: result.DateTimeEnd.ToString("dd/MM/yyyy"),
                TimeBegin: result.DateTimeBegin.ToString("HH:mm"),
                TimeEnd: result.DateTimeEnd.ToString("HH:mm"),
                OwnerName: result.UserName,
                IsPublished: result.IsPublished,
                PicturePath: result.PicturePath == null || result.PicturePath.Trim().IsBlank() ? null : result.PicturePath.Contains("https") ? result.PicturePath : "https://" + result.PicturePath
            );
        }

        /// <summary>
        /// Add a vacation based on the given model.
        /// </summary>
        /// <param name="vacation">Model that represent a vacation</param>
        /// <returns>
        /// A DTO that represents the added vacation
        /// </returns>
        public VacationDTO AddVacation(AddVacationModel vacation)
        {
            // TODO: get vacation country and saved it
            // Make an api call to get the country related to the given adress and saved it in the database

            if(vacation.File is not null)
            {
                vacation.PicturePath = _fileService.SaveFile(vacation.File, _httpContextAccessor.ConnectedUserId() + "_" + vacation.Title, "vacations");
            }

            var result = _vacationRepository.AddVacation(vacation, _httpContextAccessor.ConnectedUserId());

            return new VacationDTO(
                Id: result.Id,
                Title: result.Title,
                Description: result.Description,
                Place: result.Place,
                Longitude: result.Longitude,
                Latitude: result.Latitude,
                DateBegin: result.DateTimeBegin.ToString(),
                DateEnd: result.DateTimeEnd.ToString(),
                TimeBegin: result.DateTimeBegin.ToString(),
                TimeEnd: result.DateTimeEnd.ToString(),
                PicturePath: result.PicturePath
            );
        }

        public async Task<IEnumerable<Vacation>> GetCurrentUserVacation() => await _vacationRepository.GetVacationsForUser(_httpContextAccessor.ConnectedUserId());

        public async Task<AddMembersVacationModel> AddMembersToVacation(AddMembersDto dto)
        {
            var result = _vacationRepository.AddUsersToVacation(
                dto.VacationId,
                _httpContextAccessor.ConnectedUserId(),
                dto.MembersUid
            );

            var vacation = _vacationRepository.GetVacationByIdFor(dto.VacationId, _httpContextAccessor.ConnectedUserId());
            var members = new List<UserSearchResultItemDTO>();

            foreach (var r in result)
            {
                var user = await _userRepository.GetUserByIdAsync(r.UserId);
                members.Add(new UserSearchResultItemDTO(
                    user.Name,
                    user.FirstName,
                    user.Id,
                    user.Email
                ));

                _mailSender.SendEmailAsync(
                    user.Email,
                    "Invitation to the vacation: " + vacation.Title,
                    "<html><body>You have been invited to the vacation: " + vacation.Title + "\nClick here to accept: <a href=\"https://panoramix.cg.helmo.be/~q210129/#/accept-invitation/" + r.Id + "\">Accept invitation</a></body></html>"
                );
            }

            return new AddMembersVacationModel(
                members.ToArray(),
                dto.MembersUid.Length
            );
        }

        public bool PublishVacation(string id) => _vacationRepository.PublishVacation(id, _httpContextAccessor.ConnectedUserId());

        /// <summary>
        /// Accept the invitation for the connected user
        /// </summary>
        /// <param name="invitationId">Id of the invitation</param>
        /// <param name="userId">Id of the user</param>
        /// <returns>
        /// True if the invitation was accepted and the operation successfully ended
        /// </returns>
        /// <exception cref="VacationNotFoundException">when no vacation with the given id exists.</exception>
        /// <exception cref="WrongCredentialsException">when user is not authorized to add members.</exception>
        /// <exception cref="VacationPublishedException">when the vacation is published.</exception>
        /// <exception cref="StockageException">when there is a probleme with the storage.</exception>
        public Boolean AcceptInvitation(String invitationId)
        {
            return _vacationRepository.AcceptInvitation(invitationId, _httpContextAccessor.ConnectedUserId());
        }
    }
}
