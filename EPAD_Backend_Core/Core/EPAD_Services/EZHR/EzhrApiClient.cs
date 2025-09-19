using EPAD_Data.HTTPClient;
using EPAD_Services.Interface.EZHR;
using EPAD_Common.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EPAD_Common.HTTPClient;

namespace EPAD_Services.EZHR
{
    public class EzhrApiClient : IEzhrApiClient
    {
        private readonly HttpClient _client;
        private readonly IEzhrConfigurationService _configuration;
        private readonly ILogger<EzhrApiClient> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;
        private readonly IMemoryCache _cache;
        private readonly string _EZHRTokenKey = "urn:ezhr.api_token_authen";

        public EzhrApiClient(HttpClient client, IEzhrConfigurationService configuration, ILogger<EzhrApiClient> logger, IMemoryCache cache)
        {
            _client = client;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _policy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(1, (message, retryCount, context) =>
                {
                    var exception = message.Exception;
                    if (exception is null)
                    {
                        _logger.LogInformation($"EZHR client retry login {retryCount}");
                    }
                    else
                    {
                        _logger.LogInformation(exception, $"EZHR client retry login {retryCount}");
                    }
                    var token = GetToken(true).Result;
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                });
        }

        public async Task<ResultCheckHoursReponse> CheckMaximumWorkingHoursAndOT(string employeeATID, DateTime date)
        {
            try
            {
                var response = await _policy.ExecuteAsync(() => _client.GetAsync("/api/TA_DailyShift/CheckMaximumWorkingHoursAndOT?employeeATID=" + employeeATID + "&date=" + date.Date.ToString("yyyy-MM-dd")));
                response.EnsureSuccessStatusCode();
                return await response.ReceiveJsonAsync<ResultCheckHoursReponse>();
            }
            catch { return new ResultCheckHoursReponse(); }
        }

        public async Task<LeaveDayBasicInfoReponse> GetListEmployeeLeaveDay(string employeeATID, DateTime date)
        {
            try
            {
                var content = new GetListLeaveRequest()
                {
                    ID = 0,
                    FromDate = date.Date,
                    ToDate = date.Date,
                    ListEmployeeATID = new List<string> { employeeATID },
                    LeaveTypeIndex = new List<int> { 19, 20, 22, 26, 27, 29, 32, 31, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 49, 54 },
                    PageSize = 100
                };
                var response = await _policy.ExecuteAsync(() => _client.PostJsonAsync("/api/TA_EmployeeLeaveDay/GetListEmployeeLeaveDay", content));
                response.EnsureSuccessStatusCode();
                return await response.ReceiveJsonAsync<LeaveDayBasicInfoReponse>();
            }
            catch
            {
                return new LeaveDayBasicInfoReponse();
            }
        }


        public async Task<MissionApprovedResultReponse> GetListMissionApproved(string employeeATID, DateTime date)
        {
            try
            {
                var content = new GetListMissionRequest()
                {
                    ID = "0",
                    FromDate = date.Date,
                    ToDate = date.Date,
                    ListEmployeeATID = new List<string> { employeeATID },
                    PageSize = 100
                };
                var response = await _policy.ExecuteAsync(() => _client.PostJsonAsync("/api/HR_Mission/GetListMissionApproved", content));
                response.EnsureSuccessStatusCode();
                return await response.ReceiveJsonAsync<MissionApprovedResultReponse>();
            }
            catch
            {
                return new MissionApprovedResultReponse();
            }
        }

        public async Task<OAuth2ResponseEz> GetToken(OAuth2Request request)
        {
            var response = await _client.PostJsonAsync("/api/Login/LoginFromWeb", request);
            response.EnsureSuccessStatusCode();
            return await response.ReceiveJsonAsync<OAuth2ResponseEz>();
        }


        public async Task<string> GetToken(bool pForceGetToken = false)
        {
            var ctoken = _cache.Get<string>(_EZHRTokenKey);
            if (pForceGetToken || string.IsNullOrEmpty(ctoken))
            {
                var oauthReq = new OAuth2Request()
                {
                    username = _configuration.ClientID,
                    password = _configuration.ClientSecret,
                    lang = "vi"
                };
                var oauthRes = await GetToken(oauthReq);
                var token = oauthRes.Token;

                if (!string.IsNullOrEmpty(token))
                {
                    _cache.Set(_EZHRTokenKey, token, DateTime.Now.AddHours(1));
                }

                return token;
            }

            return ctoken;
        }

        public async Task<LateInEarlyOutApprovedResultReponse> LateInEarlyOutApprovedResult(string employeeATID, DateTime date)
        {
            try
            {
                var content = new GetListMissionRequest()
                {
                    ID = "0",
                    FromDate = date.Date,
                    ToDate = date.Date,
                    ListEmployeeATID = new List<string> { employeeATID },
                    PageSize = 100
                };
                var response = await _policy.ExecuteAsync(() => _client.PostJsonAsync("/api/TA_LateInEarlyOut/GetListLateInEarlyOutApproved", content));
                response.EnsureSuccessStatusCode();
                return await response.ReceiveJsonAsync<LateInEarlyOutApprovedResultReponse>();
            }
            catch (Exception ex) { return new LateInEarlyOutApprovedResultReponse(); }
        }
    }
}
