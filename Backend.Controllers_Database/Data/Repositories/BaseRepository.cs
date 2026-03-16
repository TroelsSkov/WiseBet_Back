using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
namespace WiseBet.backend.IRepository
{

    public abstract class BaseRepository<T> where T : IDto
    {
        protected DatabaseContext context;
        public BaseRepository(DatabaseContext c)
        {
            context = c;
        }
        public abstract Task<List<T>> GetAllAsync();
        public abstract Task<T> GetByIdAsync(int id);
        public abstract Task PostAsync(T dto);
        public abstract Task PutAsync(int id, T dto);
        public abstract Task DeleteAsync(T dto);
    }
}