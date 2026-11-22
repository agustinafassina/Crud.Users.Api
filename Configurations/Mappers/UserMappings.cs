using UsersApi.Configurations.Mappers.Requests;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;

namespace UsersApi.Configurations.Mappers
{
    public static class UserMappings
    {
        public static UserEntity ToEntity(this UserCreateDtoRequest request) => new()
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password
        };

        // The password hash is intentionally not copied: nothing above the repository needs it.
        public static UserEntity ToEntity(this UserDtoContext dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            LastName = dto.LastName,
            Email = dto.Email,
            CreatedDate = dto.CreatedDate,
            StatusId = dto.StatusId
        };

        public static UserLoginEntity ToLoginEntity(this UserEntity entity, UserDevice device) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            LastName = entity.LastName,
            Email = entity.Email,
            CreatedDate = entity.CreatedDate,
            StatusId = entity.StatusId,
            Device = device
        };

        public static UserResponse ToResponse(this UserEntity entity) => new()
        {
            Name = entity.Name,
            LastName = entity.LastName,
            Email = entity.Email
        };
    }
}
