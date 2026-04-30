using WiseBet.backend.Data;
namespace WiseBet.backend.Services.DTOs;

public class RouletteDTO
{
    public Guid SessionId {get; set;}
    public RouletteSessionStatus Status{get; set;}

    public int ActiveUsers {get; set;}
    public int MaxUsers {get; set;} = 5; 

    public int SecondsLeft {get; set;}

    public int? WinningNumber { get; set; }
    
    public RouletteBetType? WinningColor { get; set; }
    public List<Guid> Participants { get; set; } = new();
}