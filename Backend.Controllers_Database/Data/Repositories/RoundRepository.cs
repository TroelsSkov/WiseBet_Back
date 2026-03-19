using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.Models;
namespace WiseBet.backend.IRepository
{
    public class RoundRepository : BaseRepository<RoundDto, Guid>
    {
        public RoundRepository(DatabaseContext c) : base(c)
        {
        }
        public override async Task<List<RoundDto>> GetAllAsync() // Noget i vores kode er ass 
        {
            var rounds = await context.Rounds.Include(r => r.Outcome).Include(r => r.Bets).ToListAsync();
            List<RoundDto> toRet = new();

            foreach (var round in rounds)
            {
                toRet.Add(CreateRoundDtoFromRound(round));
            }

            return toRet;
        }
        public override async Task<RoundDto> GetByIdAsync(Guid id)
        {
            var round = await context.Rounds.Where(r => r.RoundID == id).Include(r => r.Outcome).Include(r => r.Bets).FirstOrDefaultAsync();
            if (round == null)
                throw new KeyNotFoundException(this);
            RoundDto toRet = CreateRoundDtoFromRound(round);
            return toRet;
        }
        public override async Task PostAsync(RoundDto dto)
        {
            var RoundToAdd = await CreateRoundFromDto(dto);
            await context.AddAsync(RoundToAdd);
            await context.SaveChangesAsync();
        }
        public override async Task PutAsync(Guid id, RoundDto dto)
        {
            var round = await context.Rounds.Where(r => r.RoundID == id).FirstOrDefaultAsync();
            if (round == null)
                throw new KeyNotFoundException(this);

            var putRound = await CreateRoundFromDto(dto);

            round.Bets = putRound.Bets;
            round.Outcome = putRound.Outcome;
            round.TotalAmount = putRound.TotalAmount;
            round.Payout = putRound.Payout;
            round.Made = putRound.Made;
            round.RoundDate = putRound.RoundDate;
            
            await context.SaveChangesAsync();
        }
        public override async Task DeleteAsync(RoundDto dto)
        {
            var round = await context.Rounds.Where(r => r.RoundID == dto.ID).FirstOrDefaultAsync();
            if (round == null)
                throw new KeyNotFoundException(this);

            context.Rounds.Remove(round);
        }

        private RoundDto CreateRoundDtoFromRound(Round round)
        {
            RoundDto toRet = new RoundDto
            {
                ID = round.RoundID,
                RoundPlayDate = round.RoundDate,
                TotalAmount = round.TotalAmount,
                Payout = round.Payout,
                Earnings = round.Made
            };

            if (round.Outcome != null)
            {
                toRet.OutcomeId = round.OutcomeId;
                toRet.OutcomeDescription = round.Outcome.OutcomeDescription;
            }

            if (round.Bets != null)
                foreach (var bet in round.Bets)
                {
                    toRet.Bets.Add(bet.BetHistoryID);
                }

            return toRet;
        }
        private async Task<Round> CreateRoundFromDto(RoundDto dto)
        {
            Round toRet = new Round
            {
                RoundID = dto.ID,
                RoundDate = dto.RoundPlayDate,
                OutcomeId = dto.OutcomeId,
                TotalAmount = dto.TotalAmount,
                Payout = dto.Payout,
                Made = dto.Earnings,
                Bets = new List<BetHistory>()
            };
            foreach (var bet in dto.Bets)
            {
                var toAdd = await context.BetHistories.Where(b => bet == b.BetHistoryID).FirstOrDefaultAsync();

                if (toAdd == null)
                    throw new KeyNotFoundException(this);

                toRet.Bets.Add(toAdd);
            }
            return toRet;
        }

    }
}