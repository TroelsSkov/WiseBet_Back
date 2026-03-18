namespace WiseBet.backend.DTOs;

public class BetDto : IDto
{
    public  Guid ID { get; set; }
    public int Amount { get; set; }
    public Guid UserId { get; set; }
    public int OutcomeBet { get; set; }
    public string OutcomeDescription { get; set; }
    public Guid RoundId { get; set; }
}