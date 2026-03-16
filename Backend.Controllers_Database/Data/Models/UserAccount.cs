using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    public class UserAccount
    {
    [Key]
    public Guid UserID{get; private set;} = Guid.NewGuid();
    public string Username{get; set;}
    public string Password{get; set;}
    public int Saldo {get; private set;}

    public ICollection<BetHistory> BetHistories {get; set;}
    public ICollection<Chat> ChatHistories {get; set;}
    }
}