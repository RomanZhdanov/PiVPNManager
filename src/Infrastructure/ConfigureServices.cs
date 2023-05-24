using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PiVPNManager.Application.Common.Interfaces;
using PiVPNManager.Infrastructure.Bot;
using PiVPNManager.Infrastructure.Bot.Handlers;
using PiVPNManager.Infrastructure.Data;
using PiVPNManager.Infrastructure.Services;

namespace PiVPNManager.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<ApplicationDbContextInitialiser>();

            services
                .AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddSingleton<IBot, TelegramBot>();
            services.AddSingleton<IBotHandlers, BotHandlers>();
            services.AddScoped<IUpdateHandlers, UpdateHandlers>();
            services.AddSingleton<UsersActionsManagerService>();

            services.AddTransient<IDateTime, DateTimeService>();
            services.AddTransient<IPiVPNService, SSHPiVPNService>();
            services.AddTransient<IQrCodeService, QrCodeService>();

            return services;
        }
    }
}
