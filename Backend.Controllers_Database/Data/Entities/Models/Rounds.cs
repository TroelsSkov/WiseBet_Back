using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
public class Round{
    [Key]
    public Guid RoundID { get; set; } = Guid.NewGuid();
    public DateTime RoundDate { get; init; } = DateTime.Now;

    public int TotalAmount {get; set;}
    public int Payout {get; set;}
    public int Made {get; set;}

    public ICollection<BetHistory> Bets { get; set; }

}
}