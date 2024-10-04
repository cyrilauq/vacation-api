using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tests.Utils;
using VacationApi.Auth;
using VacationApi.Domains;
using VacationApi.Domains.Exceptions;
using VacationApi.DTO;
using VacationApi.Infrastructure.Args;
using VacationApi.Infrastructure.Exceptions;
using VacationApi.Model;
using VacationApi.Repository;
using VacationApi.Services;
using VacationApi.Utils;

namespace Tests.Repositories
{
    public class ActivitiesDBRepositoryTests
    {
        IFormatProvider provider = new CultureInfo("fr-FR");
        private UserManager<User> userManager;
        private BDUserRepository _repository;
        private VacationRepository _vacationRepo;
        private ActivitesBDRepository _activitiesRepo;
        private VacationApiDbContext _context;
        private User _coUser;

        [SetUp]
        public async Task SetUpAsync()
        {
            _context = DbContextFactory.Create();
            var mockedUserManager = MockedUserManager.GetUserManagerMock<User>(_context);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            userManager = mockedUserManager.Object;
            _repository = new BDUserRepository(_context, userManager, null);
            var touka_ki = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "touka_ki@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Kirishima",
                FirstName = "Touka",
                UserName = "touka_ki",
                PicturePath = "url"
            };
            await userManager.CreateAsync(touka_ki, "Password123@");
            var touka_ki2 = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "touka_ki2@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Kirishima2",
                FirstName = "Touka2",
                UserName = "touka_ki2",
                PicturePath = "url"
            };
            await userManager.CreateAsync(touka_ki2, "Password123@");
            _coUser = _context.Users.Where(x => x.UserName == "touka_ki").FirstOrDefault()!;

            _vacationRepo = new VacationRepository(_context);
            var vacationGetter = new ApiVacationGetter(_context);
            _activitiesRepo = new ActivitesBDRepository(_context, vacationGetter);
        }

        [TearDown]
        public void TearDown()
        {
            DbContextFactory.Destroy(_context);
        }

        [Test]
        public void WhenVacationIdDoesNotExistThenThrowExeption()
        {
            Assert.Throws<VacationNotFoundException>(() => _activitiesRepo.AddActivitiesToVacation(
                "lkjjlkj",
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "", "", .0, 3.0, "")
                },
                _coUser.Id
            ));
        }

        [Test]
        public void WhenVacationIsPublishedThenThrowExeption()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Country = "Belgique",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );
            _context.Vacations.Where(vac => vacation.Id == vac.Id).First().IsPublished = true;
            _context.SaveChanges();

            Assert.Throws<VacationPublishedException>(() => _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "", "", .0, 3.0, "")
                },
                _coUser.Id
            ));
        }

        [Test]
        public void WhenActivitiesHasRightInformationThenItIsAddedToTheDataBase()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Country = "Belgique",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            var result = _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            );

            Assert.That(_context.Activities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void WhenActivitiesWithSameNameAlreadyExistsThenThrowException()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            Assert.Throws<ActivityAlreadyExistException>(() => _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id),
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            ));
        }

        [Test]
        public void WhenThereIsVacationInDataBaseButIdNotThenThrowException()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Country = "Belgique",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            Assert.Throws<VacationNotFoundException>(() => _activitiesRepo.AddActivitiesToVacation(
                "wrong_id_test",
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "", "", .0, 3.0, "")
                },
                _coUser.Id
            ));
        }

        [Test]
        public void WhenNoActivityPlannedForTheVacationThenReturnPlannifiedActivity()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            var activity = _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            )[0];

            _activitiesRepo.PlannifyActivity(
                new PlannifyArgs(activity.Id, "27/08/2024", "18:00", "30/08/2024", "18:00"),
                _coUser.Id
            );

            var result = _context.Activities.Where(a => a.Id == activity.Id).First();
            try
            {

                Assert.That(result.Begin, Is.EqualTo(DateTime.Parse("27/08/2024 18:00", provider)));
                Assert.That(result.End, Is.EqualTo(DateTime.Parse("30/08/2024 18:00", provider)));
            }
            catch (FormatException e)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void WhenActivityIsPlannedWhenANotherIsThenThrowException()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            var activities = _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id),
                        new ActivityDTO(ActivityId: null, "dua trie", "Una description por dua activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            );

            _activitiesRepo.PlannifyActivity(
                new PlannifyArgs(activities[0].Id, "27/08/2024", "18:00", "30/08/2024", "18:00"),
                _coUser.Id
            );

            Assert.Throws<PeriodNotFreeException>(
                () => _activitiesRepo.PlannifyActivity(
                    new PlannifyArgs(activities[1].Id, "27/08/2024", "18:00", "30/08/2024", "18:00"),
                    _coUser.Id
                )
            );
        }

        [Test]
        public void WhenActivityBeginIsPlannedDuringOtherActivityPeriodThenThrowException()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Description = "Una descriptione",
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            var activities = _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id),
                        new ActivityDTO(ActivityId: null, "dua trie", "Una description por dua activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            );

            _activitiesRepo.PlannifyActivity(
                new PlannifyArgs(activities[0].Id, "27/08/2024", "18:00", "30/08/2024", "18:00"),
                _coUser.Id
            );

            Assert.Throws<PeriodNotFreeException>(
                () => _activitiesRepo.PlannifyActivity(
                    new PlannifyArgs(activities[1].Id, "29/08/2024", "18:00", "30/08/2024", "18:00"),
                    _coUser.Id
                )
            );
        }

        [Test]
        public void WhenActivityIdIsNotInDataBaseThenThrowException()
        {

            Assert.Throws<ActivityNotFoundException>(
                () => _activitiesRepo.PlannifyActivity(
                    new PlannifyArgs("kjk kj h", "29/08/2024", "18:00", "30/08/2024", "18:00"),
                    _coUser.Id
                )
            );
        }

        [Test]
        public void WhenActivityIsFromPublishedVacationThenThrowException()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            var activities = _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id),
                        new ActivityDTO(ActivityId: null, "dua trie", "Una description por dua activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            );

            _context.Vacations.Where(vac => vacation.Id == vac.Id).First().IsPublished = true;
            _context.SaveChanges();

            Assert.Throws<VacationPublishedException>(
                () => _activitiesRepo.PlannifyActivity(
                    new PlannifyArgs(activities[0].Id, "29/08/2024", "18:00", "30/08/2024", "18:00"),
                    _coUser.Id
                )
            );
        }

        [Test]
        public void WhenActivityEndBeforeItBeginningThenThrowException()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Description = "dummy",
                    Place = "Una place in una paya",
                    PicturePath = null
                },
                _coUser.Id
            );

            var activities = _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id),
                        new ActivityDTO(ActivityId: null, "dua trie", "Una description por dua activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            );

            Assert.Throws<InvalidVacationInformation>(
                () => _activitiesRepo.PlannifyActivity(
                    new PlannifyArgs(activities[0].Id, "29/08/2024", "18:00", "27/08/2024", "18:00"),
                    _coUser.Id
                )
            );
        }

        [Test]
        public void WhenActivityUpdatedHasANewPeriodThenTheActivityIsUPdated()
        {
            var vacation = _vacationRepo.AddVacation(
                new AddVacationModel
                {
                    DateBegin = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy"),
                    DateEnd = DateTime.Now.AddMonths(12).ToString("dd/MM/yyyy"),
                    HourBegin = DateTime.Now.AddMonths(1).ToString("HH:mm"),
                    HourEnd = DateTime.Now.AddMonths(12).ToString("HH:mm"),
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya",
                },
                _coUser.Id
            );

            var activity = _activitiesRepo.AddActivitiesToVacation(
                vacation.Id,
                new List<ActivityDTO>() {
                        new ActivityDTO(ActivityId: null, "una trie", "Una description por una activity", .0, 3.0, vacation.Id)
                },
                _coUser.Id
            )[0];

            _activitiesRepo.PlannifyActivity(
                new PlannifyArgs(activity.Id, "27/08/2024", "18:00", "30/08/2024", "18:00"),
                _coUser.Id
            );
            _activitiesRepo.PlannifyActivity(
                new PlannifyArgs(activity.Id, "29/08/2024", "18:00", "30/08/2024", "18:00"),
                _coUser.Id
            );

            var result = _context.Activities.Where(a => a.Id == activity.Id).First();
            try
            {

                Assert.That(result.Begin, Is.EqualTo(DateTime.Parse("29/08/2024 18:00", provider)));
                Assert.That(result.End, Is.EqualTo(DateTime.Parse("30/08/2024 18:00", provider)));
            }
            catch (FormatException e)
            {
                Assert.Fail();
            }
        }
    }
}
