using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Polly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EPAD_Common.Clients
{
    public class EzHRUri
    {
        public static string GetAllNannyInfo = "api/EM_Nanny/GetAllNanny";
    }
    public class EZHRClient
    {
        public HttpClient Client { get; }
        private readonly IConfiguration Configuration;
        private AppConfiguration _AppConfig;

        protected readonly IMemoryCache _Cache;
        private readonly string _EzTokenKey = "urn:ezhr.api_token_authen";
        private readonly Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> _policy;
        public EZHRClient(HttpClient client, IConfiguration configuration, IMemoryCache pCache)
        {
            this.Configuration = configuration;
            _Cache = pCache;
            _AppConfig = Configuration.GetSection("EZHR").Get<AppConfiguration>();
            if (_AppConfig == null) _AppConfig = new AppConfiguration() { Api = "http://localhost:80", CompanyIndex = 2 };
            if (_AppConfig.Api == "") _AppConfig.Api = "http://localhost";
            client.BaseAddress = new Uri(_AppConfig.Api);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            Client = client;

            //_policy = Policy.Handle<HttpRequestException>()
            //    .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
            //    .RetryAsync(1, onRetry: async (exception, retryCount, context) =>
            //    {
            //        var token = await GetToken(true);
            //        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            //    });
        }

        public async Task<List<HR_NannyInfoResult>> GetAllNannyInfo(string[] pEmployeeATID, int pCompanyIndex)
        {
            Client.DefaultRequestHeaders.Add(_AppConfig.TokenHeader, _AppConfig.Token);

            var response = await Client.GetAsync(EzHRUri.GetAllNannyInfo);
            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<List<HR_NannyInfoResult>>();
            }
            catch (Exception)
            {
                return new List<HR_NannyInfoResult>();
            }
        }

        public async Task<HR_NannyInfoResult> GetNannyInfo(string pEmployeeATID, int pCompanyIndex)
        {
            Client.DefaultRequestHeaders.Add(_AppConfig.TokenHeader, _AppConfig.Token);

            var response = await Client.GetAsync($"{EzHRUri.GetAllNannyInfo}?e={pEmployeeATID}");
            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<HR_NannyInfoResult>();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
