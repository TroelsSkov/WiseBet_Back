namespace WiseBet.backend.Services.DTOs;

/// <summary>
/// Resultat af roulette hub-handling: evt. ekstra broadcasts (fx færdig runde med vindertal) før aktuel state.
/// </summary>
public sealed record RouletteSessionUpdate(
    IReadOnlyList<RouletteDto> BroadcastFirst,
    RouletteDto Current);
