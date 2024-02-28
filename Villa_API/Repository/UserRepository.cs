using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext dbContext, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _dbContext.ApplicationUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _dbContext.ApplicationUsers
                    .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.Username.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);


            if (user == null || isValid == false)
            {
                return new TokenDTO()
                {
                    AccessToken = ""
                };
            }

            var jwtTokenId = $"JTI{Guid.NewGuid()}";

            var myAccessToken = await GetAccessToken(user, jwtTokenId);

            var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);

            TokenDTO tokenDto = new()
            {
                AccessToken = myAccessToken,
                RefreshToken = refreshToken
            };
            return tokenDto;
        }

        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email = registerationRequestDTO.UserName,
                NormalizedEmail = registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if(!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult()){
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                    var userToReturn = _dbContext.ApplicationUsers
                        .FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return new UserDTO();
        }

        private async Task<string> GetAccessToken( ApplicationUser user, string jwtTokenId)
        {
            //if user was found generate JWT Token
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id)
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var myToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(myToken);
            return tokenString;
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            var existingRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(e => e.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null)
            {
                return new TokenDTO();
            }

            var compareAccessTokenData = GetAccessTokenData(tokenDTO.AccessToken);
            if (!compareAccessTokenData.isSuccessful || compareAccessTokenData.userId != existingRefreshToken.UserId || compareAccessTokenData.tokenId != existingRefreshToken.JwtTokenId)
            {
                existingRefreshToken.IsValid = false;
                _dbContext.SaveChanges();
                return new TokenDTO();
            }

            //When someone tries to use a invalid refresh token, possible fraud
           if (!existingRefreshToken.IsValid)
           {
                var chainRecords = _dbContext.RefreshTokens.Where(u => u.UserId == existingRefreshToken.UserId && u.JwtTokenId == existingRefreshToken.JwtTokenId)
                .ExecuteUpdateAsync(c => c.SetProperty(refreshToken => refreshToken.IsValid, false));

                return new TokenDTO();
           }

            //
            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                existingRefreshToken.IsValid = false;
                _dbContext.SaveChanges();
                return new TokenDTO();
            }

            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            //Make the existing token invalid
            existingRefreshToken.IsValid = false;
            _dbContext.SaveChanges();

            //Create a new access token
            var applicationUser =  _dbContext.ApplicationUsers.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
            if (applicationUser == null)
            {
                return new TokenDTO();
            }

            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
        {
            RefreshToken refreshToken = new()
            {
                IsValid = true,
                UserId = userId,
                JwtTokenId = tokenId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(2),
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
            };

            await _dbContext.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        private (bool isSuccessful, string userId, string tokenId) GetAccessTokenData(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(accessToken);
                var jwtTokenId = jwtToken.Claims.FirstOrDefault(t => t.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return (true, userId, jwtTokenId);
            }
            catch
            {

                return (false, null, null);
            }
        }
    }
}
