using Microsoft.AspNetCore.Identity;
using WiseBet.backend.Security.Models;
using WiseBet.backend.Security;
using Microsoft.EntityFrameworkCore;
namespace WiseBet.backend.Configs;

public static class SecurityServiceConfiguration
{
    public static IServiceCollection AddCustomSecurityService(this IServiceCollection services)
    {

        services.AddDbContext<SecurityDbContext>(options =>
        {
            DotNetEnv.Env.Load();
            string? DbPath = Environment.GetEnvironmentVariable("DbConnnectionString");
            if (string.IsNullOrEmpty(DbPath))
                throw new NullReferenceException("[AddCustomSecurityService] DbConnection string was not found");
            else
                options.UseSqlServer(DbPath);
        });

        services.AddAuthentication();
        services.AddAuthorization();

        services.AddScoped<SecurityService>();

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


