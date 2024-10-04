using Microsoft.AspNetCore.Mvc;
using VacationApi.Model;

namespace VacationApi.Services
{
    public interface IAuthServices
    {
        Task Login(LoginModel model);
    }
}
