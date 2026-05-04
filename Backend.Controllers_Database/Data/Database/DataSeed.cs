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

            await d_context.SaveChangesAsync();
        }
    }
}