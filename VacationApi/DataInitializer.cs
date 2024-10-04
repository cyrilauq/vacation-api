using Microsoft.AspNetCore.Identity;
using VacationApi.Auth;
using VacationApi.Domains;

namespace VacationApi
{
    public static class DataInitializer
    {
        public static void SeedRole(RoleManager<IdentityRole> roleManager)
        {
            if (roleManager.RoleExistsAsync(UserRoles.Admin).Result == false)
            {
                IdentityRole admin = new IdentityRole() { Name = UserRoles.Admin };
                var resultAdmin = roleManager.CreateAsync(admin);
                resultAdmin.Wait();
            }
            if (roleManager.RoleExistsAsync(UserRoles.User).Result == false)
            {
                IdentityRole user = new IdentityRole() { Name = UserRoles.User };
                var resultUser = roleManager.CreateAsync(user);
                resultUser.Wait();
            }
        }
        public static void Seed(UserManager<User> userManager)
        {
            var user = new User
            {
                Email = "touka_ki@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = "Kirishima",
                FirstName = "Touka",
                UserName = "touka_ki",
                PicturePath = "url"
            };
            var resultUser = userManager.CreateAsync(user, "Password123@").Result;
            if (resultUser.Succeeded)
            {
                var resultAddUserToProd = userManager.AddToRoleAsync(user, UserRoles.User).Result;
            }

        }
    }
}
