namespace TodoApi.Models.DTO
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public int ExpiresInMinutes { get; set; }
    }
}
