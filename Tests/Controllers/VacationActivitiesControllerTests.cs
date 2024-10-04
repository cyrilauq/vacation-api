using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VacationApi.Auth;
using VacationApi.DTO.Vacation;
using VacationApi.DTO;
using VacationApi.Model;
using System.Globalization;
using VacationApi.Domains;
using VacationApi.Repository;
using VacationApi.Controllers;
using Tests.Utils;
using Moq;
using VacationApi.Services;

namespace Tests.Controllers
{
    class VacationActivitiesControllerTests
    {
        private VacationRepository _repository;
        private VacationApiDbContext _context;
        private VacationActivitiesController controller;
        private User _user;
        private User _user2;

        [SetUp]
        public void SetUp()
        {
            _context = DbContextFactory.Create();
            _repository = new VacationRepository(_context);
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

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "John Doe"),
                new Claim(ClaimTypes.Email, "john.doe@example.com"),
                new Claim(AuthConstants.UID_KEY, _user.Id)
                // Add more claims as needed
            }, "mock"));

            mockHttpContext.Setup(x => x.User).Returns(mockUser);

            mockHttpContextAccessor
                .Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(AuthConstants.UID_KEY, "user_id"),
            }, "mock"));
            _repository = new VacationRepository(_context);
            var vacationGetter = new ApiVacationGetter(_context);
            controller = new VacationActivitiesController(new ActivitiesServices(_repository, new ActivitesBDRepository(_context, vacationGetter), mockHttpContextAccessor.Object))
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
        public void WhenAddActivitiesToOneOfHisVacationWithRightInfoThenReturnOkResult()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = "15/10/2024",
                    HourBegin = "15:30",
                    DateEnd = "18/10/2024",
                    HourEnd = "15:30",
                    Country = "Belgique",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya"
                },
                _user.Id
            );


            var response = controller.AddActivities(
                new ActivitiesDTO(
                    vacation.Id,
                    new ActivityDTO[] {
                            new ActivityDTO(ActivityId: null, " kjh  kl", "kl hk g f  h fcvv cchj", .0, 3.0, "kldf gf hgf hj ")
                    }
                )
            );

            Assert.That((response as ObjectResult).StatusCode, Is.EqualTo(200));
        }

        [Test]
        public void WhenUserAddActivityToAVactionHeDoesNotOwnThenReturnBadRequest()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = "15/10/2024",
                    HourBegin = "15:30",
                    DateEnd = "18/10/2024",
                    Country = "Belgique",
                    HourEnd = "15:30",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya"
                },
                _user.Id
            );

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim(ClaimTypes.Name, "username"),
                    new Claim(AuthConstants.UID_KEY, "user_id"),
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };


            var response = controller.AddActivities(
                new ActivitiesDTO(
                    vacation.Id,
                    new ActivityDTO[] {
                            new ActivityDTO(ActivityId: null, " kjh  kl", "kl hk g f  h fcvv cchj", .0, 3.0, "kldf gf hgf hj ")
                    }
                )
            );

            Assert.That((response as ObjectResult).StatusCode, Is.EqualTo(200));
        }

        [Test]
        public void WhenUserAddActivityToAVactionHeDoesNotOwnThenReturnOkAndActivities()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = "15/10/2024",
                    HourBegin = "15:30",
                    DateEnd = "18/10/2024",
                    HourEnd = "15:30",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Country = "Belgique",
                    Longitude = "150",
                    Place = "Una place in una paya"
                },
                _user.Id
            );

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim(ClaimTypes.Name, "username"),
                    new Claim(AuthConstants.UID_KEY, "user_id"),
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };


            var response = controller.AddActivities(
                new ActivitiesDTO(
                    vacation.Id,
                    new ActivityDTO[] {
                            new ActivityDTO(ActivityId: null, "b:;,bndfn", "b:;,bndfnf,bfd,n", .0, 3.0, "b:;,bndfnf,bfd,n")
                    }
                )
            );

            Assert.That((response as ObjectResult).StatusCode, Is.EqualTo(200));
        }

        [Test]
        public void WhenUserPlannifyActivityToAVactionHeDoesNotOwnThenReturnBadRequest()
        {
            var vacation = _repository.AddVacation(
                new AddVacationModel
                {
                    DateBegin = "15/10/2024",
                    Country = "Belgique",
                    HourBegin = "15:30",
                    DateEnd = "18/10/2024",
                    HourEnd = "15:30",
                    Description = "Una descriptione",
                    Title = "Una titro",
                    Latitude = "100",
                    Longitude = "150",
                    Place = "Una place in una paya"
                },
                _user.Id
            );
            var addResponse = controller.AddActivities(
                new ActivitiesDTO(
                    vacation.Id,
                    new ActivityDTO[] {
                            new ActivityDTO(ActivityId: null, "cxvcx bvc", "cd d bv gffh dgfh ghvf gdg ", .0, 3.0, "df  g gffgh fh  h fgh d")
                    }
                )
            );

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "John Doe"),
                new Claim(ClaimTypes.Email, "john.doe@example.com"),
                new Claim(AuthConstants.UID_KEY, "jhjkvg  jk hlj h")
                // Add more claims as needed
            }, "mock"));

            mockHttpContext.Setup(x => x.User).Returns(mockUser);

            mockHttpContextAccessor
                .Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

            var httpContext = new DefaultHttpContext();
            var vacationGetter = new ApiVacationGetter(_context);
            controller = new VacationActivitiesController(new ActivitiesServices(_repository, new ActivitesBDRepository(_context, vacationGetter), mockHttpContextAccessor.Object))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext,
                }
            };

            var response = controller.PlannifyActivity(
                ((addResponse as OkObjectResult).Value as ActivitiesDTO).activities[0].ActivityId, 
                new PlannifyActivityDTO(
                    "22/10/2020 15:50",
                    "22/10/2020 15:50"
                )
            );

            Assert.That((response as StatusCodeResult).StatusCode, Is.EqualTo(403));
        }
    }
}
