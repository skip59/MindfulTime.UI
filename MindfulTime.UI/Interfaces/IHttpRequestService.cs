namespace MindfulTime.UI.Interfaces
{
    public interface IHttpRequestService
    {
        public Task<string> HttpRequest(string URL, StringContent content);
    }
}
