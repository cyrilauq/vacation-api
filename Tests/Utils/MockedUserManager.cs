using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationApi.Domains;

namespace Tests.Utils
{
    internal class MockedUserManager
    {
        public static Mock<UserManager<TIDentityUser>> GetUserManagerMock<TIDentityUser>(VacationApiDbContext dbContext) 
            where TIDentityUser : IdentityUser
        {
            var mock = new Mock<UserManager<TIDentityUser>>(
                    new Mock<IUserStore<TIDentityUser>>().Object,
                    new Mock<IOptions<IdentityOptions>>().Object,
                    new Mock<IPasswordHasher<TIDentityUser>>().Object,
                    new IUserValidator<TIDentityUser>[0],
                    new IPasswordValidator<TIDentityUser>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<IServiceProvider>().Object,
                    new Mock<ILogger<UserManager<TIDentityUser>>>().Object); 
            
            var usersDbSetMock = new Mock<DbSet<TIDentityUser>>(MockBehavior.Default);

            // Set up the UserManager mock to use the DbSet mock
            mock.Setup(x => x.Users).Returns((IQueryable<TIDentityUser>)dbContext.Users);

            // Set up UserManager to modify the DbSet (and thus the DbContext) when a new user is created
            mock
                .Setup(x => x.CreateAsync(It.IsAny<TIDentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<TIDentityUser, string>((user, password) =>
                {
                    var result = usersDbSetMock.Object.Add(user);
                    dbContext.Users.Add(user as User);
                    dbContext.SaveChanges();
                });

            return mock;
        }
    }
}
