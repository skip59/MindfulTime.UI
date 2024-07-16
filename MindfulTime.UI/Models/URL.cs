namespace MindfulTime.UI.Models
{
    public static class URL
    {
        public const string BASE_AUTH_URL = "https://localhost:7199";
        public const string BASE_CALENDAR_URL = "https://localhost:7032";
        public const string BASE_ML_URL = "https://localhost:7170";

        public static readonly string AUTH_CREATE_USER = $"{BASE_AUTH_URL}/api/User/RegistrationAsync";
        public static readonly string AUTH_CHECK_USER = $"{BASE_AUTH_URL}/api/User/LoginAsync";
        public static readonly string AUTH_GET_USERS = $"{BASE_AUTH_URL}/api/User/UserList";
        public static readonly string AUTH_DELETE_USER = $"{BASE_AUTH_URL}/api/DeleteUser";
        public static readonly string AUTH_UPDATE_USER = $"{BASE_AUTH_URL}/api/UpdateUser";

        public static readonly string CALENDAR_CREATE_TASK = $"{BASE_CALENDAR_URL}/api/CreateTask";
        public static readonly string CALENDAR_GET_TASK = $"{BASE_CALENDAR_URL}/api/GetTasks";

        public static readonly string NOTIFICATION_CREATE_MSG = $"{BASE_AUTH_URL}/api/Notifications/SendMessage";

        public static readonly string TRAIN_ML = $"{BASE_ML_URL}/api/TrainML";
    }
}
