using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Security.Models;

namespace WiseBet.backend.Security;

public class SecurityDbContext : IdentityDbContext<AppUser>
{
    public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options) { }
}