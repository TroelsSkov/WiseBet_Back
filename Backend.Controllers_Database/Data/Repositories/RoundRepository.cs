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
            return new RoundDto();
        }
        public override async Task PostAsync(RoundDto dto)
        {

        }
        public override async Task PutAsync(Guid id, RoundDto dto)
        {

        }
        public override async Task DeleteAsync(RoundDto dto)
        {

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

            if (round.OutcomeId != null)
            {
                toRet.OutcomeId = (int)round.OutcomeId;
                toRet.OutcomeDescription = round.Outcome.OutcomeDescription;
            }

            foreach (var bet in round.Bets)
            {
                toRet.Bets.Add(bet.BetHistoryID);
            }

            return toRet;
        }
    }
}