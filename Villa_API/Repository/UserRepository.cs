using Microsoft.EntityFrameworkCore;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _dbContext.LocalUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            LocalUser user = new()
            {
                Name = registerationRequestDTO.Name,
                Password = registerationRequestDTO.Password,
                UserName = registerationRequestDTO.UserName,
                Role = registerationRequestDTO.Role,
            };

            _dbContext.LocalUsers.Add(user);
            await _dbContext.SaveChangesAsync();
            user.Password = "";
            return user;
        }
    }
}
