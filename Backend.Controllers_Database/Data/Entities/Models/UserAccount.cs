using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UserAccount
    {
        [Key]
        public Guid UserID { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public int Saldo { get; set; }

        public ICollection<BetHistory> BetHistories { get; set; }
        public ICollection<Chat> ChatHistories { get; set; }
    }
}