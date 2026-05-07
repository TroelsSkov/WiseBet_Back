namespace WiseBet.backend.Services.Blackjack;

public class GameState
{
    public List<Card> PlayerHand { get; set; } = new List<Card>();
    public List<Card> DealerHand { get; set; } = new List<Card>();
    public IDeck Deck { get; set; } = new Deck();
    public GameStatus State { get; set; }
    public int Bet { get; set; }

    public GameState(IDeck deck)
    {
        Deck = deck;
    }

}