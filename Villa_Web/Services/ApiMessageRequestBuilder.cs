using Newtonsoft.Json;
using System.Text;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Services.IServices;

namespace Villa_Web.Services
{
    public class ApiMessageRequestBuilder : IApiMessageRequestBuilder
    {
        public HttpRequestMessage Build(APIRequest apiRequest)
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
        }
    }
}
