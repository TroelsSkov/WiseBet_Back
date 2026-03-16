using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
namespace WiseBet.backend.IRepository
{
// Nøglen for entititerne kan nu varierer
    public abstract class BaseRepository<T, TKey> where T : IDto
    {
        protected DatabaseContext context;
        public BaseRepository(DatabaseContext c)
        {
            context = c;
        }
        public abstract Task<List<T>> GetAllAsync();
        public abstract Task<T> GetByIdAsync(TKey id);
        public abstract Task PostAsync(T dto);
        public abstract Task PutAsync(TKey id, T dto);
        public abstract Task DeleteAsync(T dto);
    }
}