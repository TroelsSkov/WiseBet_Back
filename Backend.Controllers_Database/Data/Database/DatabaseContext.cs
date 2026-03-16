using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Models;
namespace WiseBet.backend.Data
{
    public class DatabaseContext : DbContext
    {
        public string? DbPath { get; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<BetHistory> BetHistories { get; set; }
        public DbSet<BetPossibility> BetPossibilities { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<RoundResult> Results {get; set;}

        public DatabaseContext()
        {
            // // Koden avnendes til SQLlite databasen
            // var folder = Environment.SpecialFolder.LocalApplicationData;
            // var path = Environment.GetFolderPath(folder);
            // DbPath = System.IO.Path.Join(path, "WiseBet.db");

            // Henter .env connection string
            DotNetEnv.Env.Load();
            DbPath = Environment.GetEnvironmentVariable("DbConnnectionString");

        }
        // // Opretter en lokal SQLlite database
        // protected override void OnConfiguring(DbContextOptionsBuilder options)
        // => options.UseSqlite($"Data Source={DbPath}"); 

        // Anvender et connection string og kobler til en sqlserver
         protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(DbPath);
    }


}