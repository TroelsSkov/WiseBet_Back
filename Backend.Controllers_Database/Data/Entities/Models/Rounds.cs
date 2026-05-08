using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Round
    {
        [Key]
        public Guid RoundID { get; set; } = Guid.NewGuid();
        public DateTime RoundDate { get; set; } = DateTime.Now;
        public int? OutcomeId { get; set; }
        public int? TotalAmount { get; set; }
        public int? Payout { get; set; }
        public int? Made { get; set; }

        public ICollection<BetHistory>? Bets { get; set; }
        [ForeignKey("OutcomeId")]
        public Outcome? Outcome { get; set; }
    }
}