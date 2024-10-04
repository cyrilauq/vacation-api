using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net.Http;
using System.Security.Claims;
using Tests.Utils;
using VacationApi.Auth;
using VacationApi.Controllers;
using VacationApi.Domains;
using VacationApi.DTO;
using VacationApi.DTO.Vacation;
using VacationApi.Infrastructure;
using VacationApi.Model;
using VacationApi.Repository;
using VacationApi.Services;

namespace Tests.Controllers
{
    public class VacationControllerTests
    {
        private VacationRepository _repository;
        private VacationApiDbContext _context;
        private VacationsController controller;
        private User _user;
        private User _user2;

        [SetUp]
        public void SetUp()
        {
            _context = DbContextFactory.Create();
            _context.Users.Add(new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "touka_ki@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Kirishima",
                FirstName = "Touka",
                UserName = "touka_ki",
                PicturePath = "url"
            });
            _context.Users.Add(new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "touka_ki2@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Kirishima2",
                FirstName = "Touka2",
                UserName = "touka_ki2",
                PicturePath = "url"
            });
            _context.SaveChanges();
            _user = _context.Users.Where(x => x.UserName == "touka_ki").FirstOrDefault()!;
            _user2 = _context.Users.Where(x => x.UserName == "touka_ki2").FirstOrDefault()!;

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(AuthConstants.UID_KEY, "user_id"),
            }, "mock"));

            _repository = new VacationRepository(_context);
            controller = new VacationsController(new VacationsServices(_context, _repository, null, MockUtils.GetMockHttpAccessorFor(_user), new Mock<FileService>().Object, new Mock<IEmailSender>().Object))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext,
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            DbContextFactory.Destroy(_context);
        }

        [Test]
        public async Task WhenAddVacationIsCalledThenVacationIsAddedToDataHolder()
        {
            var response = controller.Vacation(new AddVacationModel
            {
                DateBegin = "15/10/2024",
                HourBegin = "15:30",
                DateEnd = "18/10/2024",
                HourEnd = "15:30",
                Description = "Una descriptione",
                Country = "Belgique",
                Title = "Una titro",
                Latitude = "100",
                Longitude = "150",
                Place = "Una place in una paya"
            });

            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(1));
            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task WhenDateTimeBeginAndEndAreSameThenReturnBadRequest()
        {
            var response = controller.Vacation(new AddVacationModel
            {
                DateBegin = "15/10/2024",
                HourBegin = "15:30",
                DateEnd = "15/10/2024",
                Country = "Belgique",
                HourEnd = "18:30",
                Description = "Una descriptione",
                Title = "Una titro",
                Latitude = "100",
                Longitude = "150",
                Place = "Una place in una paya"
            });

            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddVacationThatAlreadyExistsThenReturnErrorMessage()
        {
            var response = controller.Vacation(new AddVacationModel
            {
                DateBegin = "15/10/2024",
                HourBegin = "15:30",
                DateEnd = "18/10/2024",
                HourEnd = "15:30",
                Description = "Una descriptione",
                Title = "Una titro",
                Country = "Belgique",
                Latitude = "100",
                Longitude = "150",
                Place = "Una place in una paya"
            });
            var response2 = controller.Vacation(new AddVacationModel
            {
                DateBegin = "15/10/2024",
                HourBegin = "15:30",
                Country = "Belgique",
                DateEnd = "18/10/2024",
                HourEnd = "15:30",
                Description = "Una descriptione",
                Title = "Una titro",
                Latitude = "100",
                Longitude = "150",
                Place = "Una place in una paya"
            });

            Assert.That((await _repository.GetVacations()).Count(), Is.EqualTo(1));
        }
    }
}