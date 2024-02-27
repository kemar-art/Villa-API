using Villa_Web.Models.Dto;

namespace Villa_Web.Services.IServices
{
    public interface ITokenProvider
    {
        void SetToken(TokenDTO tokenDTO);
        TokenDTO? GetToken();
        void ClearToken();
    }
}
