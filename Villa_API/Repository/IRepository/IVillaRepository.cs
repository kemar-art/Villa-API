using System.Linq.Expressions;
using Villa_API.Models;

namespace Villa_API.Repository.IRepository
{
    public interface IVillaRepository : IGenericRepository<Villa>
    {
        Task<Villa> UpdateAsync(Villa villa);
    }
}
