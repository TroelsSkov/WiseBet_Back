using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;

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
        public override async Task<UserAccountDto> GetByIdAsync(Guid id)
        {
            
            var user = context.UserAccounts.Where( u => u.UserID == id);
            // Placeholder
            return new UserAccountDto
            {
                ID = Guid.NewGuid(),
                Username = "Test",
                Password = "Test",
                Saldo = 12345
            };
        }
        public override async Task PostAsync(UserAccountDto dto)
        {

        }
        public override async Task PutAsync(Guid id, UserAccountDto dto)
        {

        }
        public override async Task DeleteAsync(UserAccountDto dto)
        {

        }
    }
}