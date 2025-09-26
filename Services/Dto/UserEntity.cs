namespace UsersApi.Services.Dto
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Password { get; set; }
        public string? OtherProperty { get; set; }
        public int StatusId { get; set; }
    }
}