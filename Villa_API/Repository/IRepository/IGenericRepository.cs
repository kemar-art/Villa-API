using System.Linq.Expressions;
using Villa_API.Models;

namespace Villa_API.Repository.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true);
        Task CreateAsync(T villa);
        Task DeleteAsync(T villa);
        Task SaveAsync();
    }
}
