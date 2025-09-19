using EPAD_Data.Models.FR05;
using EPAD_Data.Models.WebAPIHeader;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static EPAD_Backend_Core.Controllers.IC_LoginController;

namespace EPAD_Backend_Core.Provider
{
    public class WebAPIUtility
    {
        static public async Task<string> GetToken(FR05Config config)
        {
            List<WebAPIHeader> lstHeader = new List<WebAPIHeader>();

            HttpResponseMessage response = null;
            dynamic result;
            LoginInfo loginInfo = new LoginInfo()
            {
                UserName = Convert.ToString(config.Username),
                Password = Convert.ToString(config.Password),
                ServiceId = Convert.ToString(config.ServiceId)
            };

            string content = ConvertObjectToJson(loginInfo);
            StringContent data = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {

                response = await CallAPI(config.Host + "/api/login", WebAPIMethod.POST, data, "application/json", lstHeader);

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = ConvertJsonToObject(await response.Content.ReadAsStringAsync());

                    LoginApiInfo login = new LoginApiInfo()
                    {
                        AccessToken = result["access_token"],
                        TokenType = result["token_type"],
                        ExpiresIn = result["expires_in"]
                    };

                   return login.AccessToken;
                }
            }
            catch
            {
            }
            return "";
        }

        static public async Task<HttpResponseMessage> CallAPI(string pURI, WebAPIMethod pMethod, HttpContent pData, string pContentType, List<WebAPIHeader> pHeaderList)
        {
            HttpResponseMessage responseMessage = null;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(pContentType));
            foreach (WebAPIHeader header in pHeaderList)
                client.DefaultRequestHeaders.Add(header.Name, header.Value);

            try
            {
                switch (pMethod)
                {
                    case WebAPIMethod.GET: responseMessage = await client.GetAsync(pURI); break;
                    case WebAPIMethod.POST: responseMessage = await client.PostAsync(pURI, pData); break;
                    case WebAPIMethod.PUT: responseMessage = await client.PutAsync(pURI, pData); break;
                    case WebAPIMethod.DELETE: responseMessage = await client.DeleteAsync(pURI); break;
                }
                if (responseMessage?.IsSuccessStatusCode == false && responseMessage.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    string errorMessage = $"Call api not success: \n URL: {pURI} \n Data: {JsonConvert.SerializeObject(pData)} \n ContentType: {pContentType} \n Headers: {JsonConvert.SerializeObject(pHeaderList)} \n HttpStatusCode: {responseMessage.StatusCode} \n ResponseContent: {await responseMessage.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                responseMessage.Content = new StringContent(ex.ToString(), Encoding.UTF8, "application/json");
             
            }

            return responseMessage;
        }

        static public async Task<HttpResponseMessage> GetResponseMessage(string pContent, WebAPIMethod pMethod, string pUri, FR05Config config, IMemoryCache cache)
        {
            string mToken = "";
            int numOfTryAgain = 0;
            mToken = cache.Get<string>($"tokenFR05");
        TRY_AGAIN:
            if (string.IsNullOrEmpty(mToken))
            {
                mToken = await GetToken(config);
                cache.Set("tokenFR05", mToken);
            }

            List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
            {
                new WebAPIHeader("Authorization", "Bearer " + mToken)
            };
            StringContent data = new StringContent(pContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response =  await CallAPI(pUri, pMethod, data, "application/json", lstHeader);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await GetToken(config);
                cache.Set("tokenFR05", mToken);
                lstHeader = new List<WebAPIHeader>
                {
                    new WebAPIHeader("Authorization", "Bearer " + mToken)
                };
                data = new StringContent(pContent, Encoding.UTF8, "application/json");
                response = await CallAPI(pUri, pMethod, data, "application/json", lstHeader);
            }
            if (response.IsSuccessStatusCode == false && numOfTryAgain < 3)
            {
                numOfTryAgain++;
                goto TRY_AGAIN;
            }
            return response;
        }

        static public dynamic ConvertJsonToObject(string pResult)
        {
            dynamic data = JsonConvert.DeserializeObject<object>(pResult, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return data;
        }

        static public string ConvertObjectToJson(object pObject)
        {
            string data = JsonConvert.SerializeObject(pObject);
            return data;
        }


        public enum WebAPIMethod
        {
            GET,
            POST,
            PUT,
            DELETE
        }

    }
}
