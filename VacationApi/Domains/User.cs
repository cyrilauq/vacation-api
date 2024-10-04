using Microsoft.AspNetCore.Identity;

namespace VacationApi.Domains
{
    public class User: IdentityUser
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string PicturePath { get; set; }
    }
}
