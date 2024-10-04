using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationApi.Domains;
using VacationApi.Repository;
using Tests.Utils;

namespace Tests.Repositories
{
    public class BDUserRepositoryTest
    {
        private UserManager<User> userManager;
        private BDUserRepository _repository;
        private VacationApiDbContext _context;
        private User _coUser;

        [SetUp]
        public async Task SetUpAsync()
        {
            _context = DbContextFactory.Create();
            var mockedUserManager = MockedUserManager.GetUserManagerMock<User>(_context);

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
            _coUser = userManager.Users.Where(x => x.UserName == "touka_ki").FirstOrDefault()!;
        }

        [TearDown]
        public void TearDown()
        {
            DbContextFactory.Destroy(_context);
        }

        [Test]
        public async Task whenSearchForUserAndUsersMatchQueryThenReturnThem()
        {
            Assert.That(_repository.Search("to", "dflmgjdlmgjdf").Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task whenSearchForUserAndNoUsersMatchQueryThenReturnEmptyResult()
        {
            Assert.That(_repository.Search("z", "ldksjflsdhfkldhf").Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task whenSearchForUserAndNoUsersMatchQueryAndCoUserIsInMatchThenReturnAllButNotCoUsert()
        {
            var result = _repository.Search("t", _coUser.Id);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.AreNotEqual(result[0].UserName, _coUser.UserName);
            Assert.AreNotEqual(result[1].UserName, _coUser.UserName);
        }
    }
}
