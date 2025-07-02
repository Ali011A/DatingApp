using DatingApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DatingApp.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager, ILogger logger = null)
         {
            if (await userManager.Users.AnyAsync())
            {
                logger?.LogInformation("⚠️  Users table already populated – seeding skipped.");
                return;
            }

            var path = Path.Combine(AppContext.BaseDirectory, "Data", "UserSeedData.json");
            if (!File.Exists(path))
            {
                logger?.LogError("❌ UserSeedData.json not found at {Path}", path);
                return;
            }

            var userData = await File.ReadAllTextAsync(path);
            logger?.LogInformation("📦 JSON length {Length} chars", userData.Length);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

            if (users == null || users.Count == 0)
            {
                logger?.LogError("❌ Deserialization returned no users");
                return;
            }
            logger?.LogInformation("✅ Parsed {Count} users", users.Count);

            using var hmac = new HMACSHA512();          // إعادة استعمال يحسّن الأداء
            var roles = new List<AppRole> 
            { 
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
                 
            };
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            foreach (var user in users)
            {
                user.Photos.First().IsApproved = true;
                user.UserName = user.UserName!.ToLower();
                //user.PasswordSalt = hmac.Key;
                //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser
            {
                UserName = "admin",
                KnownAs = "Admin",
                Gender = "",
                City = "",
                Country = ""
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });

            //await 

            var sw = Stopwatch.StartNew();
           // await context.SaveChangesAsync();
            sw.Stop();

            logger?.LogInformation("✅ Seeded {Count} users in {Ms} ms", users.Count, sw.ElapsedMilliseconds);
        }
    }
}
