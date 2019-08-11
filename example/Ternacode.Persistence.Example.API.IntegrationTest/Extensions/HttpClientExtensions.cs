using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ternacode.Persistence.Example.API.IntegrationTest.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<(TResponse, HttpResponseMessage)> GetWithResponseAsync<TResponse>(
            this HttpClient client,
            string route)
        {
            var httpResponse = await client.GetAsync(route);
            var stringContent = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TResponse>(stringContent);

            return (response, httpResponse);
        }

        public static async Task<(TResponse, HttpResponseMessage)> PostWithResponseAsync<TResponse>(
            this HttpClient client,
            string route,
            object data)
        {
            var postContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, MediaTypeNames.Application.Json);
            var httpResponse = await client.PostAsync(route, postContent);
            var stringContent = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TResponse>(stringContent);

            return (response, httpResponse);
        }
    }
}