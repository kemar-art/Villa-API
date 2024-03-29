﻿using Microsoft.AspNetCore.Http.HttpResults;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Models.Dto;
using Villa_Web.Services.IServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Villa_Web.Services
{
    public class VillaNumberService :  IVillaNumberService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseService _baseService;
        private string villaUrl;

        public VillaNumberService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IBaseService baseService)
        {
            _httpClientFactory = httpClientFactory;
            _baseService = baseService;
            villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }

        public async Task<T> CreateAsync<T>(VillaNumberCreateDTO createDTO)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.POST,
                Data = createDTO,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI",
                //Token = token
            });
        }

        public async Task<T> DeleteAsync<T>(int id)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.DELETE,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/" + id,
                //Token = token
            });
        }

        public async Task<T> GetAllAsync<T>()
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.GET,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI",
                //Token = token
            });
        }

        public async Task<T> GetAsync<T>(int id)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.GET,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/" + id,
                //Token = token
            });
        }

        public async Task<T> UpdateAsync<T>(VillaNumberUpdateDTO updateDTO)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = StaticDetails.ApiType.PUT,
                Data = updateDTO,
                Url = villaUrl + $"/api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/" + updateDTO.VillaNo,
                //Token = token
            }); ;
        }
    }
}
