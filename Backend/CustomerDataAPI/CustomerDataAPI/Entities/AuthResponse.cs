namespace CustomerDataAPI.Entities
{
    public class AuthResponse
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }

        public int CustomerId { get; set; } 

        public string Role { get; set; }
    }
}
