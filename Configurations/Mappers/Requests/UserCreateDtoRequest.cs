using System.ComponentModel.DataAnnotations;

namespace UsersApi.Configurations.Mappers.Requests
{
    public class UserCreateDtoRequest
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        // Optional: if empty, a random password is generated and returned once in the response.
        [MinLength(8)]
        [MaxLength(128)]
        public string? Password { get; set; }
    }
}
