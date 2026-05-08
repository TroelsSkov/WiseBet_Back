using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Data;
namespace WiseBet.backend.Services.Coinflip.Validation;

public interface IGeneralValidation
{
    Task<CoinFlipDTO> ValidateBet(Guid userId, int amounts);
}