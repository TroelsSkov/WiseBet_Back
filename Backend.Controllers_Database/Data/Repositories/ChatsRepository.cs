using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.Models;

namespace WiseBet.backend.IRepository
{
    public class ChatsRepository : BaseRepository<ChatDto, Guid>
    {
        public ChatsRepository(DatabaseContext c) : base(c)
        {
        }

        public override async Task<List<ChatDto>> GetAllAsync()
        {
            return new List<ChatDto>();
        }
        public override async Task<ChatDto> GetByIdAsync(Guid id)
        {
            return new ChatDto
            {
                Username = "Placeholder",
                Message = "Placeholder",
                TimeOfChat = DateTime.Now
            };
        }
        public override async Task PostAsync(ChatDto dto)
        {

        }
        public override async Task PutAsync(Guid id, ChatDto dto)
        {

        }
        public override async Task DeleteAsync(ChatDto dto)
        {

        }
    }
}
