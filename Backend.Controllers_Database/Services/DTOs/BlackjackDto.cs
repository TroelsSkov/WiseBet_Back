using WiseBet.backend.Services.Blackjack;

namespace WiseBet.backend.Services.DTOs;

public class BlackjackDto
{
    public List<Card> PlayerHand { get; set; }
    public List<Card> DealerVisibleHand { get; set; } // kun ét kort
    public GameStatus Status { get; set; }
}