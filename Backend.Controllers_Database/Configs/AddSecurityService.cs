using Microsoft.AspNetCore.Identity;
using WiseBet.backend.Security.Models;
using WiseBet.backend.Security;
namespace WiseBet.backend.Configs;

public static class SecurityServiceConfiguration
{
    public static IServiceCollection AddCustomSecurityService(this IServiceCollection services)
    {

        services.AddDbContext<SecurityDbContext>();

        services.AddAuthentication();
        services.AddAuthorization();

        services.AddIdentityApiEndpoints<AppUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SecurityDbContext>();

        return services;
    }

    public static WebApplication AddCustomSecurityWebapplication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}


