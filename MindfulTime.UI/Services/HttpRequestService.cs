using MindfulTime.UI.Interfaces;

namespace MindfulTime.UI.Services
{
    public class HttpRequestService : IHttpRequestService
    {
        public async Task<string> HttpRequestPost(string URL, StringContent content = null) 
        {
            try
            {
                using HttpClient httpClient = new();
                var response = await httpClient.PostAsync($"{URL}", content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                httpClient.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                return $"FALSE: {ex.Message}";
            }
        }

        public async Task<string> HttpRequestGet(string URL)
        {
            try
            {
                using HttpClient httpClient = new();
                var response = await httpClient.GetAsync($"{URL}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                httpClient.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                return $"FALSE: {ex.Message}";
            }
        }
    }

}
