using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.Security.Models;

namespace WiseBet.backend.Security;

public class SecurityService
{
    private readonly SecurityDbContext _context;
    private readonly UserAccountRepository _userRepo;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userMananager;
    public SecurityService(SecurityDbContext context, DatabaseContext c, SignInManager<AppUser> signInMan, UserManager<AppUser> userMan)
    {
        _context = context;
        _userRepo = new(c);
        _signInManager = signInMan;
        _userMananager = userMan;
    }
    public async Task<bool> ValidateRegisterRequest(AuthRegisterDto reg)
    {
        UserAccountDto newUser = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = reg.UserName,
            Password = reg.Password,
            Saldo = 10000 // start saldo?
        };

        try
        {
            await _userRepo.PostAsync(newUser);
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"[SecurityService] Unable to regsiter account: {e.Message}");
            return false;
        }

        var result = await _userMananager.CreateAsync(new AppUser
        {
            UserName = reg.UserName,
            UserRepoConnect = newUser.ID,
        }, reg.Password);

        if (!result.Succeeded)
        {
            try
            {
                await _userRepo.DeleteAsync(newUser);
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"[SecurityService] Was not able to clean up user: {e.Message}");
                return false;
            }
        }

        return true;
    }

    public async Task<bool> ValidateLoginRequest(AuthLoginDto login)
    {


        return true;
    }
}