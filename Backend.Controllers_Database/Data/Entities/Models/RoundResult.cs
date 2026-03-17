using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class RoundResult
    {
        [Key]
        public Guid RoundResultID {get; set;} = Guid.NewGuid();
        public string Result {get; set;}

        public ICollection<Round> Round {get; set;}
    }
}