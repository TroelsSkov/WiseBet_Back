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
            var chats = await context.Chats.Include(c => c.UserAccount).ToListAsync();
            List<ChatDto> toRet = new();

            foreach (var chat in chats)
            {
                toRet.Add(new ChatDto
                {
                    ID = chat.ChatID,
                    UserId = chat.UserAccount.UserID,
                    Message = chat.chat,
                    TimeOfChat = chat.TimeOfChat
                });
            }

            return toRet;
        }
        public override async Task<ChatDto> GetByIdAsync(Guid id)
        {
            var chat = await context.Chats.Where(c => c.ChatID == id).Include(c => c.UserAccount).FirstOrDefaultAsync();
            if (chat == null)
                throw new KeyNotFoundException(this);

            return new ChatDto
            {
                ID = chat.ChatID,
                UserId = chat.UserAccount.UserID,
                Message = chat.chat,
                TimeOfChat = chat.TimeOfChat
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
