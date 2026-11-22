namespace UsersApi.Configurations.Mappers.Responses
{
    public class UserCreatedResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? GeneratedPassword { get; set; }
    }
}
