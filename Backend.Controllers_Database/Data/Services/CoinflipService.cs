using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.DTOs;
using WiseBet.backend.Data;
using WiseBet.backend.Models;
namespace WiseBet.backend.Services;

public class CoinFlipService : ICoinflipService
{
    private readonly UserAccountRepository _userRepo;
    private readonly BetRepository _betRepo;
    private readonly RoundRepository _roundRepo;



    public CoinFlipService(UserAccountRepository userRepo, BetRepository betRepo, RoundRepository roundRepo)
    {
        _userRepo = userRepo;
        _betRepo = betRepo;
        _roundRepo = roundRepo;
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

        var roundDto = new RoundDto{OutcomeId = IsWin? 0:1 , TotalAmount = Winnings, Payout = IsWin? Amount : -Amount,  };
        var BetDto = new BetDto{RoundId = roundDto.ID, UserId = UserId, Amount = Amount, OutcomeDescription = IsWin? "Won":"Lost"};
        await _roundRepo.PostAsync(roundDto);
        await _betRepo.PostAsync(BetDto);
        
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