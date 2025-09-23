namespace UsersApi.Mappers.Requests
{
    public class UserRequest
    {
        public int Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Created { get; set; }
        public string PasswordHash { get; set; }
    }
}