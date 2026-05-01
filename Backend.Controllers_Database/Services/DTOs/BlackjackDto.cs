using WiseBet.backend.Services.Blackjack;

namespace WiseBet.backend.Services.DTOs;

public class BlackjackDto
{
    public List<Card> PlayerHand { get; set; }
    public List<Card> DealerVisibleHand { get; set; } 
    public GameStatus Status { get; set; }
    public int PlayerScore { get; set; }
    public int DealerScore { get; set; }
}