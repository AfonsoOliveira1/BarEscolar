using BarEscolar.Models;
using BarEscolar.Services;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace BarEscolar.Data
{
    public class DataSeeder
    {
        public static void SeedAdmin(JsonUserStore userStore)
        {
            List<Users> users = userStore.GetAll();
            if (users.Any())
                return;

            PasswordHasher<Users> passwordHasher = new PasswordHasher<Users>();

            Users admin = new Users
            {
                ID = Guid.NewGuid().ToString(),
                UserName = "Afonsus",
                Email = "fonso@gmail.com",
                role = UserRole.Admin
            };

            admin.passwordhash = passwordHasher.HashPassword(admin, "1234");

            userStore.AddUser(admin);
        }
    }
}
