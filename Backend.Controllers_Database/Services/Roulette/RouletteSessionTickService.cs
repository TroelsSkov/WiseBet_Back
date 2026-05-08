using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WiseBet.backend.Hubs;

namespace WiseBet.backend.Services.Roulette;

// Afvikler roulette-rundetimer uafhængigt af hub-kald og broadcaster state til hver aktiv session-gruppe.

public class RouletteSessionTickService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<RouletteSessionTickService> _logger;

    public RouletteSessionTickService(
        IServiceScopeFactory scopeFactory,
        IHubContext<GameHub> hubContext,
        ILogger<RouletteSessionTickService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var roulette = scope.ServiceProvider.GetRequiredService<IRouletteService>();
                var dtos = await roulette.ProcessSessionTimersAsync();

                foreach (var dto in dtos)
                {
                    await _hubContext.Clients
                        .Group(dto.SessionId.ToString())
                        .SendAsync("UpdateClient", dto, cancellationToken: stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RouletteSessionTickService tick failed");
            }
        }
    }
}
