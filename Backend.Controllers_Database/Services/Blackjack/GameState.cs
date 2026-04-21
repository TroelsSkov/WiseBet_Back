namespace WiseBet.backend.Services.Blackjack;

public class GameState
{
    public List<Card> PlayerHand {get; set;} = new List<Card>();
    public List<Card> DealerHand {get; set; } = new List<Card>();
    public Deck Deck {get; set;}
    public GameStatus State{get; set;}
    public int Bet{get; set;}

}