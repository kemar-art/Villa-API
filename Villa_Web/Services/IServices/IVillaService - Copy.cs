using Villa_Web.Models.Dto;

namespace Villa_Web.Services.IServices
{
    public interface IVillaserviceService
    {
        Task<T> GetAllAsync<T>();
        Task<T> GetAsync<T>(int id);
        Task<T> CreateAsync<T>(VillaCreateDTO createDTO);
        Task<T> UpdateAsync<T>(VillaUpdateDTO updateDTO);
        Task<T> DeleteAsync<T>(int id);
    }
}
