using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.Models;
namespace WiseBet.backend.IRepository
{
    public class BetRepository : BaseRepository<BetDto, Guid>
    {
        public BetRepository(DatabaseContext c) : base(c)
        {
        }
        public override async Task<List<BetDto>> GetAllAsync() // Noget i vores kode er ass 
        {
            var bets = await context.BetHistories.Include(b => b.OutcomeBet).ToListAsync();
            List<BetDto> toRet = new();

            foreach (var bet in bets)
                toRet.Add(CreateBetDtoFromBetHistory(bet));

            return toRet;
        }
        public override async Task<BetDto> GetByIdAsync(Guid id)
        {
            var bet = await context.BetHistories.Where(b => b.BetHistoryID == id).FirstOrDefaultAsync();
            if (bet == null)
                throw new KeyNotFoundException(this);
            BetDto toRet = CreateBetDtoFromBetHistory(bet);
            return toRet;
        }
        // public override async Task PostAsync(BetHistory BetEntity)
        // {
        //     await context.BetHistories.AddAsync(BetEntity);
        //     await context.SaveChangesAsync();
        // }
        

        public override async Task PostAsync(BetDto dto)
        {
            BetHistory toAdd = CreateBetHistoryFromBetDto(dto);
            await context.BetHistories.AddAsync(toAdd);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Denne funktion ændrer indsats beløbet og ingen andre aspekter af indsats.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public override async Task PutAsync(Guid id, BetDto dto)
        {
            var bet = await context.BetHistories.Where(b => b.BetHistoryID == id).FirstOrDefaultAsync();
            if (bet == null)
                throw new KeyNotFoundException(this);

            var PutRound = CreateBetHistoryFromBetDto(dto);

            bet.Amount = PutRound.Amount;

            await context.SaveChangesAsync();
        }
        public override async Task DeleteAsync(BetDto dto)
        {
            var bet = await context.BetHistories.Where(b => b.BetHistoryID == dto.ID).FirstOrDefaultAsync();
            if (bet == null)
                throw new KeyNotFoundException(this);

            context.BetHistories.Remove(bet);
            await context.SaveChangesAsync();
        }

        public async Task<List<BetDto>> GetAllBetsForRound(Guid id)
        {
            var bets = await context.BetHistories
                .Where(b => b.RoundID == id)
                .Include(b => b.OutcomeBet)
                .ToListAsync();

            List<BetDto> toRet = new();
            foreach (var bet in bets)
                toRet.Add(CreateBetDtoFromBetHistory(bet));

            return toRet;
        }

        private BetDto CreateBetDtoFromBetHistory(BetHistory bet)
        {
            BetDto toRet = new BetDto
            {
                ID = bet.BetHistoryID,
                UserId = bet.UserID,
                RoundId = bet.RoundID,
                Amount = bet.Amount,
                OutcomeID = bet.OutcomeId,
                OutcomeDescription = bet.OutcomeBet.OutcomeDescription
            };
            return toRet;
        }
        private BetHistory CreateBetHistoryFromBetDto(BetDto bet)
        {
            BetHistory toRet = new BetHistory
            {
                BetHistoryID = bet.ID,
                UserID = bet.UserId,
                RoundID = bet.RoundId,
                Amount = bet.Amount,
                OutcomeId = bet.OutcomeID
            };
            return toRet;
        }
    }
}