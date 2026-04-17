using Microsoft.AspNetCore.Identity;
using WiseBet.backend.Security.Models;
using WiseBet.backend.Security;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.IRepository;
using System.Diagnostics.Tracing;
namespace WiseBet.backend.Configs;

public static class SecurityServiceConfiguration
{
    public static IServiceCollection AddCustomSecurityService(this IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddAuthorization();

        services.AddScoped<SecurityService>();
        services.AddScoped<UserAccountRepository>();

        services.AddIdentityApiEndpoints<AppUser>()
            // .AddRoles<IdentityRole>() // Skal indsættes senere
            .AddEntityFrameworkStores<DatabaseContext>();

            return services;
    }

    public static WebApplication AddCustomSecurityWebapplication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}


