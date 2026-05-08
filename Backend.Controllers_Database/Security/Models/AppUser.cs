using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Security.Models;
public class AppUser : IdentityUser
{
    [Required]
    public Guid UserRepoConnect { get; set; }
}
