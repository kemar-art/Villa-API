using Villa_API.Models;

namespace Villa_API.Repository.IRepository
{
    public interface IVillaNumberRepository : IGenericRepository<VillaNumber>
    {
        Task<VillaNumber> UpdateAsync(VillaNumber villaNumber);
    }
}
