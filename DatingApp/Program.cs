using DatingApp.Data;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Middleware;
using DatingApp.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp
{
    public class Program
    {
        public static  async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

           builder.Services.AddApplicationServices(builder.Configuration);
           builder.Services.AddIdentityServices(builder.Configuration);
            var app = builder.Build();

            app.UseMiddleware<ExceptionMiddleware>();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseAuthentication(); // This line enables authentication middleware 1to process incoming requests
            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();

           
            
            app.MapControllers();
            app.MapHub<PresenceHub>("hubs/presence"); // Map the PresenceHub to the "/hubs/presence" endpoint>
            app.MapHub<MessageHub>("hubs/message"); // Map the PresenceHub to the "/hubs/presence" endpoint>
           app.MapFallbackToController("Index","Fallback");
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<DatingDbContext>();
            var userManger= services.GetRequiredService<UserManager<AppUser>>();
            var roleManger=services.GetRequiredService<RoleManager<AppRole>>();
            try
            {
               
                await context.Database.MigrateAsync();
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[Connections]");
                await Seed.SeedUsers(userManger,roleManger);

            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during application startup");
            }
            app.Run();
        }
    }
}
