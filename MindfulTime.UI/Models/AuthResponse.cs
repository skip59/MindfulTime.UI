namespace MindfulTime.UI.Models
{
    public class AuthResponse
    {
        public Guid Id { get; set; }
        public bool isOk => !string.IsNullOrEmpty(Role);
        public string Role { get; set; }

    }
}
