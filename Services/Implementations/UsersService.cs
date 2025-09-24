using AutoMapper;
using UsersApi.ClientsDB.SqlServer;
using UsersApi.ClientsDB.SqlServer.Dto;
using UsersApi.Mappers.Requests;
using UsersApi.Services.Dto;
using UsersApi.Services.Interfaces;

namespace UsersApi.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        public UsersService(UserDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity entity)
        {
            var passwordHash = HashPassword(entity.Password);

            var user = new UserDtoContext
            {
                Name = entity.Name,
                Email = entity.Email,
                Password = passwordHash,
                OtherProperty = entity.OtherProperty,
                CreatedDate = DateTime.UtcNow,
                StatusId = 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return entity;
        }

        private string HashPassword(string password)
        {
            // Usar BCrypt o PBKDF2
            return password; // placeholder, reemplazar con hashing real
        }
    }
}