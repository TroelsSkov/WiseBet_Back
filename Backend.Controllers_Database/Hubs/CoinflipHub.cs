using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.DTOs;
using WiseBet.backend.Controllers.DTOs;
using Microsoft.AspNetCore.SignalR;
using Sprache;
namespace WiseBet.backend.Hubs;
public enum CoinSide
{
    plat = 0,
    krone = 1
}
    public class CoinFlipHub : Hub
    {
        public async Task PlayRound(int Amount, CoinSide ChosenSide)
        {
            await Clients.Caller.SendAsync("Recieve round bet", Amount, ChosenSide);
            if (Amount <=0)
                {
                    await Clients.Caller.SendAsync("RecieveError", "Amount is less or equal to zero");
                    return;
                };
            //todo check saldo
            CoinSide CoinResult = (CoinSide)Random.Shared.Next(0,2);
            bool IsWin = CoinResult==ChosenSide;
            
            await Clients.Caller.SendAsync("RecieveResult", new {
                LandingSide = CoinResult.ToString(),
                IsWinner = IsWin,
                Winnings = IsWin ? 2*Amount: 0
            });
            //todo connect to db

        
        }
        //does something when a user connects
        // public override Task OnConnectedAsync()
        // {
        //     return base.OnConnectedAsync();
        // }

        //does something when user disconnects
        // public override Task OnDisconnectedAsync(Exception? exception)
        // {
        //     return base.OnDisconnectedAsync(exception);
        // }
    }