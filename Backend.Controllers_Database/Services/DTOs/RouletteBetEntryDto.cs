using WiseBet.backend.Data;

namespace WiseBet.backend.Services.DTOs;

public class RouletteBetEntryDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = "";
    public int Amount { get; set; }
    public RouletteBetType BetType { get; set; }
}
