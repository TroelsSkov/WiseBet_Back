namespace WiseBet.backend.Services.DTOs;

using WiseBet.backend.Data;

public class RouletteBetDto
{
    public int Amount { get; set; }
    public RouletteBetType BetType { get; set; } 
}