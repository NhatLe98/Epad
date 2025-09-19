using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Common.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> ReceiveJsonAsync<T>(this HttpResponseMessage response, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent, jsonSerializerSettings);
        }

        public static async Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string endpoint, object payload)
        {
            var content = CreateJsonPayload(payload);
            return await client.PostAsync(endpoint, content);
        }

        private static StringContent CreateJsonPayload(object payload)
        {
            var jsonString = JsonConvert.SerializeObject(payload);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }
    }
}
