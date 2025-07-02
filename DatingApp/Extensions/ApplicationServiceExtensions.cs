using DatingApp.Data;
using DatingApp.Data.Repository;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using DatingApp.Services;
using DatingApp.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration config)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDbContext<DatingDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILikesRepository, LikesRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IPhotoRepository, PhotoRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>(); // Add the UnitOfWork as a scoped service>
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<LogUserActivity>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddSignalR();
            services.AddSingleton<PresenceTracker>();
            return services;
        }
    }
}
