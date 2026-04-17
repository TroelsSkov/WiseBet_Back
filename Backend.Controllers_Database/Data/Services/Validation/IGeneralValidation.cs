using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.Data;
namespace WiseBet.backend.Services;

public interface IGeneralValidation
{
    Task<CoinFlipDTO> ValidateBet(Guid userId, int amounts);
}