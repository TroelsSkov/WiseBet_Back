using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Models;
namespace WiseBet.backend.Data
{

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DatabaseContext : DbContext
    {
        private readonly string? DbPath;
        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<BetHistory> BetHistories { get; set; }
        public DbSet<BetPossibility> BetPossibilities { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<RoundResult> Results { get; set; }

        public DatabaseContext()
        {
            // // Koden avnendes til SQLlite databasen
            // var folder = Environment.SpecialFolder.LocalApplicationData;
            // var path = Environment.GetFolderPath(folder);
            // DbPath = System.IO.Path.Join(path, "WiseBet.db");

            // // Henter .env connection string
            // DotNetEnv.Env.Load();
            // string DbPath = Environment.GetEnvironmentVariable("DbConnnectionString");
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured) // Ny løsning; Anvendes i unit test.
            {
                DotNetEnv.Env.Load();
                string? DbPath = Environment.GetEnvironmentVariable("DbConnnectionString");
                if (string.IsNullOrEmpty(DbPath))
                    throw new NullReferenceException("DbConnection string was not found");
                else
                    options.UseSqlServer(DbPath);
            }
        }
    }
}