using WiseBet.backend.Data;
using WiseBet.backend.Services;
namespace WiseBet.backend.Controllers.DTOs;

public class CoinFlipDTO
{
    public BetType BetType{get; set;}
    public int LandingSide { get; set; } // denne burde erstates og winnings burde returneres, landesiden kan bestemmes ud fra det.
    public int Winnings { get; set; }
    public bool Fail {get; set;}
    public required string Message {get; set;}
}