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
            var payments = await context.PaymentHistories.Include(p => p.UserAccount).ToListAsync();  // Maybe include is not needed?
            List<PaymentDto> toRet = new();
            foreach (var payment in payments)
            {
                toRet.Add(new PaymentDto
                {
                    ID = payment.PaymentID,
                    UserID = payment.UserID,
                    TimeOfPayment = payment.TimeOfPayment,
                    PaymentAmount = payment.PaymentAmount,
                    PrePaymentBalance = payment.PrePaymentBalance
                });
            }

            return toRet;
        }
        public override async Task<PaymentDto> GetByIdAsync(Guid id)
        {
            var payment = await context.PaymentHistories.Where(p => p.PaymentID == id).Include(p => p.UserAccount).FirstOrDefaultAsync();
            if (payment == null)
                throw new KeyNotFoundException(this);

            PaymentDto toRet = new PaymentDto
            {
                ID = payment.PaymentID,
                UserID = payment.UserID,
                TimeOfPayment = payment.TimeOfPayment,
                PaymentAmount = payment.PaymentAmount,
                PrePaymentBalance = payment.PrePaymentBalance
            };
            // Placeholder
            return toRet;
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