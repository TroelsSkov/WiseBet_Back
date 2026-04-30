using WiseBet.backend.Services.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
namespace WiseBet.backend.Services.Coinflip;

public class CoinFlipService : ICoinflipService
{
    private readonly UserAccountRepository _userRepo;

    public CoinFlipService(UserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }


    public async Task<CoinFlipDTO> PlayRound(Guid UserId, int Amount, CoinSide ChosenSide)
    {
        var user = await _userRepo.GetByIdAsync(UserId);
        CoinSide CoinResult = (CoinSide)Random.Shared.Next(0, 2);
        string res = CoinResult == 0 ? "Wise" : "Coin";
        Console.WriteLine($"[CoinflipService] Coin side: {res}");
        bool IsWin = CoinResult == ChosenSide;
        int Winnings = IsWin ? Amount : -Amount;
        user.Saldo += Winnings;
        await _userRepo.PutAsync(UserId, user);
        var usertjek = await _userRepo.GetByIdAsync(UserId);
        Console.WriteLine(usertjek.Saldo);
        return new CoinFlipDTO
        {
            LandingSide = CoinResult,
            Winnings = IsWin ? 2 * Amount : 0,
            Fail = false,
            Message = IsWin ? "You Won" : "You almost won try again quickly"
        };

    }
}