using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Services.IServices;

namespace Villa_Web.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public APIResponse responseModel { get; set; }

        public BaseService(IHttpClientFactory httpClientFactory)
        {
            this.responseModel = new();
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T> SendAsync<T>(APIRequest apiRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("VillaAPI");
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

                HttpResponseMessage apiResponse = null;

                if (!string.IsNullOrEmpty(apiRequest.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Token);
                }


                apiResponse = await client.SendAsync(message);

                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                try
                {
                    APIResponse APIResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                    if (APIResponse != null && (apiResponse.StatusCode == System.Net.HttpStatusCode.BadRequest || apiResponse.StatusCode == System.Net.HttpStatusCode.NotFound))
                    {
                        APIResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        APIResponse.IsSuccess = false;
                        var res = JsonConvert.SerializeObject(APIResponse);
                        var returnObj = JsonConvert.DeserializeObject<T>(res);
                        return returnObj;
                    }
                }
                catch (Exception ex)
                {

                    var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
                    return APIResponse;
                }

                var exception = JsonConvert.DeserializeObject<T>(apiContent);
                return exception;
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

    }
}
