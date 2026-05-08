namespace WiseBet.backend.Data;

public enum RouletteSessionStatus
{
    WaitingForPlayers = 0,
    BettingOpen = 1,
    Spinning = 2,
    RoundFinished = 3
}

public enum RouletteBetType
{
    Green = 0, 
    Red = 1,
    Black = 2
}
