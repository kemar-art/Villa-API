using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Repository.IRepository;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Villa_API.Repository
{
    public class VillaRepository : GenericRepository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VillaRepository(ApplicationDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<Villa> UpdateAsync(Villa villa)
        {
            villa.UpdatedDate = DateTime.Now;
             _dbContext.Villas.Update(villa);
            await _dbContext.SaveChangesAsync();
            return villa;
        }
    }
}
