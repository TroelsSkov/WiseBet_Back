namespace WiseBet.backend.DTOs;

public class ChatDto : IDto
{
    public Guid ID { get; set; }
    public required Guid UserId { get; set; }
    public required string Message { get; set; }
    public required DateTime TimeOfChat { get; set; }
}
