namespace MindfulTime.UI.Models
{
    public static class URL
    {
        public const string BASE_AUTH_URL = "https://localhost:7199";
        public const string BASE_CALENDAR_URL = "https://localhost:7032";


        public static readonly string AUTH_CREATE_USER = $"{BASE_AUTH_URL}/api/CreateUser";
        public static readonly string AUTH_CHECK_USER = $"{BASE_AUTH_URL}/api/CheckUser";
        public static readonly string AUTH_GET_USERS = $"{BASE_AUTH_URL}/api/GetUsers";
        public static readonly string AUTH_DELETE_USER = $"{BASE_AUTH_URL}/api/DeleteUser";
        public static readonly string AUTH_UPDATE_USER = $"{BASE_AUTH_URL}/api/UpdateUser";



        public static readonly string CALENDAR_CREATE_TASK = $"{BASE_CALENDAR_URL}/api/CreateTask";
        public static readonly string CALENDAR_GET_TASK = $"{BASE_CALENDAR_URL}/api/GetTasks";
    }
}
