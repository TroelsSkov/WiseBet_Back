namespace WiseBet.backend.Controllers.DTOs;

public class CoinFlipDTO
{
    public int LandingSide { get; set; }
    public int Winnings { get; set; }
    public bool Fail {get; set;}
    public required string Message {get; set;}
}