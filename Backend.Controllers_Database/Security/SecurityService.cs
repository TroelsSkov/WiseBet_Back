using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.Data;

namespace WiseBet.backend.Security;

public class SecurityService
{
    private readonly SecurityDbContext _context;
    public SecurityService(SecurityDbContext context)
    {
        _context = context;
    }
    public async Task<bool> ValidateRegisterRequest(AuthRegisterDto reg)
    {


        return true;
    }

    public async Task<bool> ValidateLoginRequest(AuthLoginDto login)
    {


        return true;
    }
}