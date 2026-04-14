using WiseBet.backend.Data;
using WiseBet.backend.Configs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseContext>(); // Configurationen sker i DatabaseContext.cs

builder.Services.AddControllers();

builder.Services.AddCustomSecurityService();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

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

app.AddCustomSecurityWebapplication();

app.Run();


