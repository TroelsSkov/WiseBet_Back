using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class BetPossibility
    {
    [Key]
    public Guid BetPossibilityID {get; set;} = Guid.NewGuid();
    public string BetDescription {get; set;}

    public ICollection<BetHistory> Bets {get; set;}
    
    }
}