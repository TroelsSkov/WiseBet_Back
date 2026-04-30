namespace WiseBet.backend.Services.Blackjack;

public class Card
{
    public Suit Suit {get; set;}
    public Rank Rank {get; set;}

    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }
    public int Value => Rank switch
    {
        Rank.Ace => 11,
        Rank.Queen or Rank.Jack or Rank.King => 10,
        _ => (int)Rank
    };
}