using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.Models;
namespace WiseBet.backend.IRepository
{
    public class PaymentsRepository : BaseRepository<PaymentDto, Guid>
    {
        UserAccountRepository m_userRepo;
        public PaymentsRepository(DatabaseContext c) : base(c)
        {
            m_userRepo = new UserAccountRepository(c);
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

            return toRet;
        }
        /// <summary>
        /// Indsætter en ny betaling i databasen og opdaterer brugerens saldo med dto'ens 'PaymentAmount'. 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Returns nothing</returns>
        /// <exception cref="InvalidParameterException"></exception>
        public override async Task PostAsync(PaymentDto dto)
        {
            if (dto.PaymentAmount < 0)
                throw new InvalidParameterException(this);

            var user = await m_userRepo.GetByIdAsync(dto.UserID);
            dto.PrePaymentBalance = user.Saldo;

            // Updating usersaldo
            user.Saldo += dto.PaymentAmount;
            await m_userRepo.PutAsync(user.ID, user);


            PaymentHistory toAdd = new PaymentHistory
            {
                PaymentID = dto.ID,
                UserID = dto.UserID,
                TimeOfPayment = dto.TimeOfPayment,
                PaymentAmount = dto.PaymentAmount,
                PrePaymentBalance = dto.PrePaymentBalance
            };

            await context.PaymentHistories.AddAsync(toAdd);
            await context.SaveChangesAsync();
        }


        /// <summary>
        /// Changes everything in the PaymentHistory entity.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns>Return nothing</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidParameterException"></exception>
        public override async Task PutAsync(Guid id, PaymentDto dto)
        {
            var payment = await context.PaymentHistories.Where(p => id == p.PaymentID).Include(p => p.UserAccount).FirstOrDefaultAsync();
            if (payment == null)
                throw new KeyNotFoundException(this);
            if (dto.PaymentAmount < 0 || dto.PrePaymentBalance < 0)
                throw new InvalidParameterException(this);

            payment.PaymentAmount = dto.PaymentAmount;
            payment.PrePaymentBalance = dto.PrePaymentBalance;
            payment.TimeOfPayment = dto.TimeOfPayment;
            payment.UserID = dto.UserID;

            await context.SaveChangesAsync();
        }
        public override async Task DeleteAsync(PaymentDto dto)
        {
            var payment = context.PaymentHistories.Where(p => dto.ID == p.PaymentID).FirstOrDefaultAsync();
            if (payment == null)
                throw new KeyNotFoundException(this);

            context.Remove(payment);
            await context.SaveChangesAsync();
        }
    }
}