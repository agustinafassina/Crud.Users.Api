using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UsersApi.Configurations.ClientsDB.SqlServer;
using UsersApi.Configurations.ClientsDB.SqlServer.Dto;
using UsersApi.Services.Dto;
using UsersApi.Services.Interfaces;

namespace UsersApi.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly Configurations.ClientsDB.Mongo.SecurityAuth _jwtSettings;
        private readonly ISessionsService _sessionsService;
        public UsersService(UserDbContext context, IMapper mapper, IOptions<Configurations.ClientsDB.Mongo.SecurityAuth> jwtSettings, ISessionsService sessionsService)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
            _mapper = mapper;
            _sessionsService = sessionsService;
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity entity)
        {
            string passwordHash = HashPassword(entity.Password);

            UserDtoContext user = new UserDtoContext
            {
                Name = entity.Name,
                Email = entity.Email,
                Password = passwordHash,
                OtherProperty = entity.OtherProperty,
                CreatedDate = DateTime.UtcNow,
                StatusId = 2
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return entity;
        }

        private string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword);

        }

        public async Task<List<UserEntity>> GetUsers()
        {
            List<UserDtoContext>? users = _context.Users.ToList();
            List<UserEntity>? userEntities = _mapper.Map<List<UserEntity>>(users);
            return userEntities;
        }

        public async Task<UserEntity> ValidateUserAsync(string email, string password)
        {
            UserDtoContext userDto = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.StatusId == 2);
            if (userDto == null)
                return null;

            if (BCrypt.Net.BCrypt.Verify(password, userDto.Password))
            {
                UserEntity userEntity = _mapper.Map<UserEntity>(userDto);
                return userEntity;
            }

            return null;
        }

        public string GenerateJwtToken(UserEntity user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtSettings.Authority,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Add saving of the token in the mongodb database
            var session = _sessionsService.CreateSessionAsync(user.Id, DateTime.UtcNow.AddHours(1), tokenString);

            return tokenString;
        }
    }
}