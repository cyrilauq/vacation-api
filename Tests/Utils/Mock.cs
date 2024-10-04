using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VacationApi.Auth;
using VacationApi.Domains;

namespace Tests.Utils
{
    internal class MockUtils
    {
        public static IHttpContextAccessor GetMockHttpAccessorFor(User user)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(AuthConstants.UID_KEY, user.Id)
                // Add more claims as needed
            }, "mock"));

            mockHttpContext.Setup(x => x.User).Returns(mockUser);

            mockHttpContextAccessor
                .Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
            return mockHttpContextAccessor.Object;
        }
    }
}
