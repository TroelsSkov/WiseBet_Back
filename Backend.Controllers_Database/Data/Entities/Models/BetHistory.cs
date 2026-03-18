using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class BetHistory
    {
        [Key]
        public Guid BetHistoryID { get; set; } = Guid.NewGuid();
        public int Amount { get; set; }

        public Guid UserID { get; set; }
        public UserAccount UserAccount { get; set; }

        public int? OutcomeId { get; set; }
        [ForeignKey("OutcomeId")]
        public Outcome OutcomeBet { get; set; }

        public Guid RoundID { get; set; }
        public Round Round { get; set; }
    }
}