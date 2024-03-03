using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Models.Dto;
using Villa_Web.Services.IServices;

namespace Villa_Web.Services
{
    public class AuthService :  IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseService _baseService;
        private string villaUrl;

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IBaseService baseService) 
        {
            _httpClientFactory = httpClientFactory;
            _baseService = baseService;
            villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }
        public async Task<T> LoginAsync<T>(LoginRequestDTO requestDTO)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = requestDTO,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/UserAuth/login"
            }, withBearer: false);
        }

        public async Task<T> LogoutAsync<T>(TokenDTO obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = obj,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/UserAuth/revoke"
            });
        }

        public async Task<T> RegisterAsync<T>(RegisterationRequestDTO requestDTO)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = requestDTO,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/UserAuth/register"
            }, withBearer: false);
        }
    }
}
