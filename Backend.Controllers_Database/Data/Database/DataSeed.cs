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
            // Sikrer at databasen er opdateret
            await d_context.Database.MigrateAsync();

            // Tjek om der allerede er data (undgå duplikering)
            if (await d_context.UserAccounts.AnyAsync()) return;

            // 1. Opret alle 20 oprindelige brugere
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

            // 2. Opret Bet-muligheder (skal bruges til repositories)
            var possibilities = new List<BetPossibility>
    {
        new BetPossibility { BetDescription = "Rød" },
        new BetPossibility { BetDescription = "Sort" },
        new BetPossibility { BetDescription = "Grøn (0)" }
    };
            await d_context.BetPossibilities.AddRangeAsync(possibilities);

            // 3. Opret et par runder
            var rounds = new List<Round>
    {
        new Round { TotalAmount = 5000, Payout = 4500, Made = 500 },
        new Round { TotalAmount = 2000, Payout = 1800, Made = 200 }
    };
            await d_context.Rounds.AddRangeAsync(rounds);
            await d_context.SaveChangesAsync();

            // 4. Opret test-data for Repositories (Kobler de første par brugere til bets og chat)
            var testBets = new List<BetHistory>
    {
        new BetHistory { Amount = 100, UserID = users[0].UserID, RoundID = rounds[0].RoundID, BetPossibilityID = possibilities[0].BetPossibilityID },
        new BetHistory { Amount = 250, UserID = users[1].UserID, RoundID = rounds[0].RoundID, BetPossibilityID = possibilities[1].BetPossibilityID },
        new BetHistory { Amount = 500, UserID = users[3].UserID, RoundID = rounds[1].RoundID, BetPossibilityID = possibilities[0].BetPossibilityID }
    };
            await d_context.BetHistories.AddRangeAsync(testBets);

            var testChats = new List<Chat>
    {
        new Chat { UserID = users[0].UserID, chat = "Håber på rød i denne runde!" },
        new Chat { UserID = users[12].UserID, chat = "William her, jeg går all-in!" }
    };
            await d_context.Chats.AddRangeAsync(testChats);

            var testPayments = new List<PaymentHistory>
    {
        new PaymentHistory { UserID = users[0].UserID, PaymentAmount = 500, PrePaymentBalance = 0 },
        new PaymentHistory { UserID = users[3].UserID, PaymentAmount = 1000, PrePaymentBalance = 1100 }
    };
            await d_context.PaymentHistories.AddRangeAsync(testPayments);

            await d_context.SaveChangesAsync();
        }
    }
}