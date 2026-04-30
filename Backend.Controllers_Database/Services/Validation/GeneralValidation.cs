using WiseBet.backend.Services.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.DTOs;
using WiseBet.backend.Data;
using System.Net;
namespace WiseBet.backend.Services;

public class GeneralValidation : IGeneralValidation
{
    private readonly UserAccountRepository _userRepo;

    public GeneralValidation(UserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }


    public async Task<CoinFlipDTO> ValidateBet(Guid UserId, int Amount)
    {
        var user = await _userRepo.GetByIdAsync(UserId);
        Console.WriteLine(user);
        if (user == null)
        {
            return new CoinFlipDTO
            {
                Fail = true,
                Message = "User doesnt exist"
            };
        }

        if (Amount <= 0)
        {
            return new CoinFlipDTO
            {
                Fail = true,
                Message = "Amount is less or equal to zero"
            };
        }
        ;

        if (Amount > user.Saldo)
        {
            return new CoinFlipDTO
            {
                Fail = true,
                Message = "You cant afford this bet"
            };
        }
        else
        {
            return new CoinFlipDTO
            {
                Fail = false,
                Message = "Bet accepted"
            };
            
        }

    }
}