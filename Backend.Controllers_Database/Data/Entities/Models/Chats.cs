using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Chat{
    [Key]
    public Guid ChatID{get; set;} = Guid.NewGuid();
    public Guid UserID{get; set;}
    public string chat{get; set;} 
    public DateTime TimeOfChat {get; set;} = DateTime.Now;//dansk tid utc er internationalt
   //foreignkey
    public UserAccount UserAccount {get; set;}
    }
}