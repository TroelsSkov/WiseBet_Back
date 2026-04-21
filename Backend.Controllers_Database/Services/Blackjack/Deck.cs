namespace WiseBet.backend.Services.Blackjack;

public class Deck
{
    private List<Card> _cards;

    public Deck()
    {
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                _cards.Add(new Card(suit, rank));
            }
        }
        Random random = new Random();
        _cards = _cards.OrderBy(card => random.Next()).ToList();
    }

    public Card draw()
    {
        Card TopCard = _cards[0];
        _cards.RemoveAt(0);
        return TopCard;
    }
}