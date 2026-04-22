namespace WiseBet.backend.Services.Blackjack;

public interface IDeck
{
    void Shuffle();
    Card draw();
}