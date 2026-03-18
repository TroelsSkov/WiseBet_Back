using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class BetPossibility
    {
    [Key]
    public int BetPossibilityID {get; set;}
    public string BetDescription {get; set;}

    public ICollection<BetHistory> Bets {get; set;}
    
    }
}