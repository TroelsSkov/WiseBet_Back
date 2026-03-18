using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Outcome
    {
    [Key]
    public int OutcomeId {get; set;}
    public required string OutcomeDescription {get; set;}    
    }
}