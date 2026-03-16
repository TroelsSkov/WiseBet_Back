using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
            //d_context.Database.EnsureDeleted(); // Kan anvendes hvis databasen driller
            d_context.Database.Migrate();

            // Seeding kommando
            // Brug .Any() til at sikrer du ikke indsætter 2x 

            await d_context.SaveChangesAsync();
        }
    }
}