using Villa_Utility;
using Villa_Web.Models.Dto;
using Villa_Web.Services.IServices;

namespace Villa_Web.Services
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenProvider(IHttpContextAccessor httpContextAccessor) 
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void ClearToken()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(StaticDetails.AccessToken);
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(StaticDetails.RefreshToken);
        }

        public TokenDTO GetToken()
        {
            try
            {
                bool myAccessToken = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(StaticDetails.AccessToken, out string token);
                bool myRefreshToken = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(StaticDetails.RefreshToken, out string refreshtoken);
                TokenDTO tokenDTO = new()
                {
                    AccessToken = token,
                    RefreshToken = refreshtoken,
                };

                return myAccessToken ? tokenDTO : null;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public void SetToken(TokenDTO tokenDTO)
        {
            var cookieOptios = new CookieOptions { Expires = DateTime.UtcNow.AddDays(60) };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(StaticDetails.AccessToken, tokenDTO.AccessToken, cookieOptios);
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(StaticDetails.RefreshToken, tokenDTO.RefreshToken, cookieOptios);
        }
    }
}
