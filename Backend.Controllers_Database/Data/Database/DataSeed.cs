using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Models;

namespace WiseBet.backend.Data
{
    public class DataSeed
    {
        private readonly DatabaseContext d_context;

        public DataSeed(DatabaseContext c)
        {
            d_context = c;
        }

        public async void Seed()
        {
            // d_context.Database.EnsureDeleted(); // Kan anvendes hvis databasen driller
            d_context.Database.Migrate();

            // Seeding kommando
            // Brug .Any() til at sikrer du ikke indsætter 2x 
            var users = new List<UserAccount>
    {
        new UserAccount { Username = "Lukas_Bet88", Password = "HashedPassword123", Saldo = 500 },
        new UserAccount { Username = "EmmaJensen", Password = "HashedPassword123", Saldo = 1250 },
        new UserAccount { Username = "OliverWin", Password = "HashedPassword123", Saldo = 0 },
        new UserAccount { Username = "SofiaP", Password = "HashedPassword123", Saldo = 2100 },
        new UserAccount { Username = "Noah_A", Password = "HashedPassword123", Saldo = 75 },
        new UserAccount { Username = "Alma_C", Password = "HashedPassword123", Saldo = 340 },
        new UserAccount { Username = "VictorLarsen", Password = "HashedPassword123", Saldo = 1000 },
        new UserAccount { Username = "FrejaS", Password = "HashedPassword123", Saldo = 15 },
        new UserAccount { Username = "EmilR", Password = "HashedPassword123", Saldo = 600 },
        new UserAccount { Username = "ClaraJ", Password = "HashedPassword123", Saldo = 0 },
        new UserAccount { Username = "Oscar_P", Password = "HashedPassword123", Saldo = 450 },
        new UserAccount { Username = "IdaMadsen", Password = "HashedPassword123", Saldo = 120 },
        new UserAccount { Username = "WilliamK", Password = "HashedPassword123", Saldo = 3000 },
        new UserAccount { Username = "EllaOlsen", Password = "HashedPassword123", Saldo = 55 },
        new UserAccount { Username = "Aksel_T", Password = "HashedPassword123", Saldo = 800 },
        new UserAccount { Username = "NoraC", Password = "HashedPassword123", Saldo = 10 },
        new UserAccount { Username = "MalthePoulsen", Password = "HashedPassword123", Saldo = 225 },
        new UserAccount { Username = "Olivia_K", Password = "HashedPassword123", Saldo = 0 },
        new UserAccount { Username = "ValdemarM", Password = "HashedPassword123", Saldo = 1400 },
        new UserAccount { Username = "KarlaHolm", Password = "HashedPassword123", Saldo = 95 }
    };

            await d_context.UserAccounts.AddRangeAsync(users);
            await d_context.SaveChangesAsync();
        }
    }
}