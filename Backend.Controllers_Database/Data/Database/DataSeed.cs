using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Models;

namespace WiseBet.backend.Data
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DataSeed
    {
        private readonly DatabaseContext d_context;

        public DataSeed(DatabaseContext c)
        {
            d_context = c;
        }

        public async void Seed()
        {
            // Sikrer at databasen er opdateret
            await d_context.Database.MigrateAsync();

            // Tjek om der allerede er data (undgå duplikering)
            if (await d_context.UserAccounts.AnyAsync()) return;

            // 1. Opret alle 20 oprindelige brugere
            var users = new List<UserAccount>
    {
        new UserAccount { Username = "Lukas_Bet88", Saldo = 500 },
        new UserAccount { Username = "EmmaJensen", Saldo = 1250 },
        new UserAccount { Username = "OliverWin", Saldo = 0 },
        new UserAccount { Username = "SofiaP", Saldo = 2100 },
        new UserAccount { Username = "Noah_A", Saldo = 75 },
        new UserAccount { Username = "Alma_C", Saldo = 340 },
        new UserAccount { Username = "VictorLarsen", Saldo = 1000 },
        new UserAccount { Username = "FrejaS", Saldo = 15 },
        new UserAccount { Username = "EmilR", Saldo = 600 },
        new UserAccount { Username = "ClaraJ", Saldo = 0 },
        new UserAccount { Username = "Oscar_P", Saldo = 450 },
        new UserAccount { Username = "IdaMadsen", Saldo = 120 },
        new UserAccount { Username = "WilliamK", Saldo = 3000 },
        new UserAccount { Username = "EllaOlsen", Saldo = 55 },
        new UserAccount { Username = "Aksel_T", Saldo = 800 },
        new UserAccount { Username = "NoraC", Saldo = 10 },
        new UserAccount { Username = "MalthePoulsen", Saldo = 225 },
        new UserAccount { Username = "Olivia_K", Saldo = 0 },
        new UserAccount { Username = "ValdemarM", Saldo = 1400 },
        new UserAccount { Username = "KarlaHolm", Saldo = 95 }
    };
            await d_context.UserAccounts.AddRangeAsync(users);
            await d_context.SaveChangesAsync();
        }
    }
}