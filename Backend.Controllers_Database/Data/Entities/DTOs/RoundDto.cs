namespace WiseBet.backend.DTOs;

public class RoundDto : IDto
{
    public Guid ID { get; set; }
    public DateTime RoundPlayDate { get; set; }
    public int TotalAmount { get; set; }
    public int Payout { get; set; }
    public int Earnings { get; set; }
    // public List<BetDtos> Bets { get; set; }
}