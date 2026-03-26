using WiseBet.backend.Controllers;
using WiseBet.backend.Models;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WiseBet.backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseContext>(); // Configurationen sker i DatabaseContext.cs

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = new DatabaseContext();

    var seed = new DataSeed(context);
    seed.Seed();
}
app.MapHub<CoinFlipHub>("/CoinFlipHub");
app.Run();


