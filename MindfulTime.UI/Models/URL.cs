namespace MindfulTime.UI.Models
{
    public static class URL
    {
        public const string BASE_AUTH_URL = "https://localhost:7199";
        public const string BASE_CALENDAR_URL = "https://localhost:7032";
        public static readonly string AUTH_CHECK_USER = $"{BASE_AUTH_URL}/api/CheckUser";
        public static readonly string CALENDAR_CREATE_TASK = $"{BASE_CALENDAR_URL}/api/CreateTask";
    }
}
