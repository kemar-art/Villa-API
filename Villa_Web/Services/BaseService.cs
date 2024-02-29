using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Services.IServices;
using Villa_Web.Models.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net;
using System;

namespace Villa_Web.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly string VillaApiUrl;

        public APIResponse responseModel { get; set; }

        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.responseModel = new();
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
            _httpContextAccessor = httpContextAccessor;
            VillaApiUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }

        public async Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("VillaAPI");

                var messageFactory = () =>
                {
                    HttpRequestMessage message = new();
                    if (apiRequest.ContentType == StaticDetails.ContentType.MultipartFormData)
                    {
                        message.Headers.Add("Accept", "*/*");
                    }
                    else
                    {
                        message.Headers.Add("Accept", "application/json");
                    }

                    message.RequestUri = new Uri(apiRequest.Url);

                    

                    if (apiRequest.ContentType == StaticDetails.ContentType.MultipartFormData)
                    {
                        var content = new MultipartFormDataContent();

                        foreach (var item in apiRequest.Data.GetType().GetProperties())
                        {
                            var value = item.GetValue(apiRequest.Data);
                            if (value is FormFile)
                            {
                                var file = (FormFile)value;
                                if (file != null)
                                {
                                    content.Add(new StreamContent(file.OpenReadStream()), item.Name, file.FileName);
                                }
                            }
                            else
                            {
                                content.Add(new StringContent(value == null ? "" : value.ToString()), item.Name);
                            }
                        }

                        message.Content = content;
                    }
                    else
                    {
                        if (apiRequest.Data != null)
                        {
                            message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                                Encoding.UTF8, "application/json");
                        }
                    }

                    message.Method = apiRequest.ApiType switch
                    {
                        StaticDetails.ApiType.POST => HttpMethod.Post,
                        StaticDetails.ApiType.PUT => HttpMethod.Put,
                        StaticDetails.ApiType.DELETE => HttpMethod.Delete,
                        _ => HttpMethod.Get,
                    };

                    //switch (apiRequest.ApiType)
                    //{
                    //    case StaticDetails.ApiType.POST:
                    //        message.Method = HttpMethod.Post;
                    //        break;
                    //    case StaticDetails.ApiType.PUT:
                    //        message.Method = HttpMethod.Put;
                    //        break;
                    //    case StaticDetails.ApiType.DELETE:
                    //        message.Method = HttpMethod.Delete;
                    //        break;
                    //    default:
                    //        message.Method = HttpMethod.Get;
                    //        break;
                    //}

                    return message;
                };

                HttpResponseMessage httpResponseMessage = null;

                //if (!string.IsNullOrEmpty(apiRequest.Token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Token);
                //}


                httpResponseMessage = await SendWithRefreshTokenAsync(client, messageFactory, withBearer);


                APIResponse FinalApiResponse = new()
                {
                    IsSuccess = false
                };




                //var apiContent = await httpResponseMessage.Content.ReadAsStringAsync();
                try
                {
                    switch (httpResponseMessage.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            FinalApiResponse.ErrorsMessages = new List<string>() { "Not Found" };
                            break;
                        case HttpStatusCode.Forbidden:
                            FinalApiResponse.ErrorsMessages = new List<string>() { "Access Denied" };
                            break;
                        case HttpStatusCode.Unauthorized:
                            FinalApiResponse.ErrorsMessages = new List<string>() { "Unauthorized" };
                            break;
                        case HttpStatusCode.InternalServerError:
                            FinalApiResponse.ErrorsMessages = new List<string>() { "Internal Server Error" };
                            break;
                        default:
                            var apiContent = await httpResponseMessage.Content.ReadAsStringAsync();
                            FinalApiResponse.IsSuccess = true;
                            FinalApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                            break;
                    }
                }
                catch (Exception ex)
                {

                    FinalApiResponse.ErrorsMessages = ["Error Encountered", ex.Message.ToString()];
                }
                var res = JsonConvert.SerializeObject(FinalApiResponse);
                var returnObj = JsonConvert.DeserializeObject<T>(res);
                return returnObj;
            }
            catch (Exception ex)
            {
                var dto = new APIResponse
                {
                    ErrorsMessages = new List<string> { Convert.ToString(ex.Message) },
                    IsSuccess = false
                };

                var res = JsonConvert.SerializeObject(dto);
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;
            }
        }

        private async Task<HttpResponseMessage> SendWithRefreshTokenAsync(HttpClient httpClient, Func<HttpRequestMessage> httpRequestMessageFactory, bool withBearer = true)
        {
            if (!withBearer)
            {
                return await httpClient.SendAsync(httpRequestMessageFactory());
            }
            else
            {
                TokenDTO tokenDTO = _tokenProvider.GetToken();
                if (tokenDTO != null && !string.IsNullOrEmpty(tokenDTO.AccessToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDTO.AccessToken);
                }

                try
                {
                    var response = await httpClient.SendAsync(httpRequestMessageFactory());
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //GENERATE NEW Token from Refresh token / Sign in with that new token and then retry
                        await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                        response = await httpClient.SendAsync(httpRequestMessageFactory());
                        return response;
                    }
                    return response;


                }
                catch (HttpRequestException httpRequestException)
                {
                    if (httpRequestException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // refresh token and retry the request
                        await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                        return await httpClient.SendAsync(httpRequestMessageFactory());
                    }
                    throw;
                }
            }
            
        }

        private async Task InvokeRefreshTokenEndpoint(HttpClient httpClient, string existingAccessToken, string existingRefreshToken)
        {
            HttpRequestMessage message = new();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri($"{VillaApiUrl}/api/{StaticDetails.CurrentAPIVersion}/UsersAuth/refresh");
            message.Method = HttpMethod.Post;
            message.Content = new StringContent(JsonConvert.SerializeObject(new TokenDTO()
            {
                AccessToken = existingAccessToken,
                RefreshToken = existingRefreshToken
            }), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(message);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<APIResponse>(content);

            if (apiResponse?.IsSuccess != true)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync();
                _tokenProvider.ClearToken();
                //throw new AuthException();
            }
            else
            {
                var tokenDataStr = JsonConvert.SerializeObject(apiResponse.Result);
                var tokenDto = JsonConvert.DeserializeObject<TokenDTO>(tokenDataStr);

                if (tokenDto != null && !string.IsNullOrEmpty(tokenDto.AccessToken))
                {
                    //New method to sign in with the new token that we receive
                    await SignInWithNewTokens(tokenDto);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDto.AccessToken);
                }
            }
        }

        private async Task SignInWithNewTokens(TokenDTO tokenDTO)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenDTO.AccessToken);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "unique_name").Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));
            var principal = new ClaimsPrincipal(identity);
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            _tokenProvider.SetToken(tokenDTO);
        }
    }
}
