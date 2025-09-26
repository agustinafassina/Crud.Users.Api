using System.ComponentModel.DataAnnotations;

namespace UsersApi.Repository.SqlServer.Dto
{
    public class StatusDtoContext
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}