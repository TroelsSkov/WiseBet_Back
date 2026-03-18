using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.Models;
namespace WiseBet.backend.IRepository
{
    public class PaymentsRepository : BaseRepository<PaymentDto, Guid>
    {
        public PaymentsRepository(DatabaseContext c) : base(c)
        {
        }
        public override async Task<List<PaymentDto>> GetAllAsync()
        {
            // Placeholder
            return new List<PaymentDto>();
        }
        public override async Task<PaymentDto?> GetByIdAsync(Guid id)
        {
            // Placeholder
            return new PaymentDto
            {
                UserID = Guid.NewGuid()
            };
        }
        public override async Task PostAsync(PaymentDto dto)
        {

        }
        public override async Task PutAsync(Guid id, PaymentDto dto)
        {

        }
        public override async Task DeleteAsync(PaymentDto dto)
        {
            
        }
    }
}