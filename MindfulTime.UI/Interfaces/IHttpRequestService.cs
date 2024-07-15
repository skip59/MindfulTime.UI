namespace MindfulTime.UI.Interfaces
{
    public interface IHttpRequestService
    {
        public Task<string> HttpRequestPost(string URL, StringContent content = null);

        public Task<string> HttpRequestGet(string URL);
    }
}
