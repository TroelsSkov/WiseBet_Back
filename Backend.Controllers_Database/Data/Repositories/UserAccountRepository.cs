using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.Models;
namespace WiseBet.backend.IRepository
{
    public class UserAccountRepository : BaseRepository<UserAccountDto, Guid>
    {
        public UserAccountRepository(DatabaseContext c) : base(c)
        {
        }
        public override async Task<List<UserAccountDto>> GetAllAsync()
        {
            var Users = await context.UserAccounts.ToListAsync();
            List<UserAccountDto> UserDtos = new();

            foreach (var user in Users)
            {
                UserDtos.Add(new UserAccountDto
                {
                    ID = user.UserID,
                    Username = user.Username,
                    Password = user.Password,
                    Saldo = user.Saldo
                });
            }

            return UserDtos;
        }
        public override async Task<UserAccountDto?> GetByIdAsync(Guid id)
        {

            var user = await context.UserAccounts.Where(u => u.UserID == id).FirstOrDefaultAsync();
            if (user == null)
                return null;

            return new UserAccountDto
            {
                ID = user.UserID,
                Username = user.Username,
                Password = user.Password,
                Saldo = user.Saldo
            };
        }
        public override async Task PostAsync(UserAccountDto dto)
        {
            UserAccount userToAdd = new UserAccount
            {
                UserID = dto.ID,
                Username = dto.Username,
                Password = dto.Password,
                Saldo = dto.Saldo
            };

            try
            {
                await context.UserAccounts.AddAsync(userToAdd);
                await context.SaveChangesAsync();
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"[UserAccountRepository] Caught an exception: {e.Message}");
            }

        }
        public override async Task PutAsync(Guid id, UserAccountDto dto)
        {
            var user = await context.UserAccounts.Where(u => u.UserID == id).FirstOrDefaultAsync();

            if(user == null)
                throw new KeyNotFoundException(this);
            
            user.UserID = dto.ID;
            user.Username = dto.Username;
            user.Password = dto.Password;
            user.Saldo = dto.Saldo;

            await context.SaveChangesAsync();
        }
        public override async Task DeleteAsync(UserAccountDto dto)
        {
            var user = await context.UserAccounts.Where(u => dto.ID == u.UserID).FirstOrDefaultAsync();

            if(user == null)
                throw new KeyNotFoundException(this);

            context.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}