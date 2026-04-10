using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.DTOs;
using WiseBet.backend.Data;
namespace WiseBet.backend.Services;

public class CoinFlipService : ICoinflipService
{
    private UserAccountRepository _userRepo;

    public CoinFlipService(UserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<CoinFlipDTO> PlayRound(Guid UserId, int Amount, CoinSide ChosenSide)
    {
        var user = await _userRepo.GetByIdAsync(UserId);
       
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
        };

        if (Amount > user.Saldo)
        {
            return new CoinFlipDTO
            {
                Fail = true,
                Message = "You cant afford this bet"
            };
        };

        CoinSide CoinResult = (CoinSide)Random.Shared.Next(0, 2);
        bool IsWin = CoinResult == ChosenSide;
        int Winnings = IsWin? Amount:-Amount;
        user.Saldo += Winnings;
        await _userRepo.PutAsync(UserId, user);

            return new CoinFlipDTO
            {
            LandingSide = (int)CoinResult,
            Winnings = IsWin ? 2 * Amount : 0,
            Fail = false,
            Message = IsWin? "You Won" : "You almost won try again quickly"
            };
    }
}