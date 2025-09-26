using System.ComponentModel.DataAnnotations;

namespace UsersApi.Repository.SqlServer.Dto
{
    public class UserDtoContext
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Password { get; set; }
        public string? OtherProperty { get; set; }
        public int StatusId { get; set; }
        public StatusDtoContext Status { get; set; }
    }
}