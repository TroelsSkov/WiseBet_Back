using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PaymentHistory
    {
        //GUID for sikkerhed? samt DTO såen vi ik returnerer password via. postman
    [Key]
    public Guid PaymentID{get; private set;} = Guid.NewGuid();
    public Guid UserID{get; set;}
    public UserAccount UserAccount{get; set;}
    public DateTime TimeOfPayment {get; set;} = DateTime.Now;
    public int PaymentAmount{get; set;}
    public int PrePaymentBalance{get; set;}
    
    }
}