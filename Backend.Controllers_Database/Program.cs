using WiseBet.backend.Data;
using WiseBet.backend.Configs;
using Scalar.AspNetCore;
using WiseBet.backend.Hubs;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.Services.Roulette;
using WiseBet.backend.Services.Coinflip;
using WiseBet.backend.Services.Coinflip.Validation;
using Microsoft.Extensions.Options;
using WiseBet.backend.IRepository;

var builder = WebApplication.CreateBuilder(args);
var FrontEndUrl = builder.Configuration.GetValue<string>("FrontendSettings:baseUrl");
builder.Services.AddDbContext<DatabaseContext>(); // Configurationen sker i DatabaseContext.cs

builder.Services.AddControllers();

// I din Program.cs

builder.Services.AddCustomSecurityService();

builder.Services.AddScoped<RoundRepository>();
builder.Services.AddScoped<BetRepository>();
builder.Services.AddScoped<UserAccountRepository>();
builder.Services.AddSingleton<RouletteSessionStore>();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

builder.Services.AddScoped<ICoinflipService, CoinFlipService>();
builder.Services.AddScoped<IBlackjackService, BlackjackService>();
builder.Services.AddScoped<IRouletteService, RouletteService>();
builder.Services.AddScoped<IGeneralValidation, GeneralValidation>();

builder.Services.AddCors(Options =>
{
    Options.AddPolicy("FrontEndPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("FrontEndPolicy");
app.AddCustomSecurityWebapplication();

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


app.MapHub<GameHub>("/GameHub");
app.Run();