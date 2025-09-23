namespace UsersApi.Mappers.Requests
{
    public class SessionCreateRequest
    {
        public int UserId { get; set; }
        public string ExpirationToken { get; set; }
        public string Token { get; set; }
    }
}