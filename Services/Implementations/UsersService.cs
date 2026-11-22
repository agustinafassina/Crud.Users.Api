using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UsersApi.Configurations.Mappers;
using UsersApi.Repository.SqlServer;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;
using UsersApi.Services.Exceptions;
using UsersApi.Services.Interfaces;

namespace UsersApi.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private const int EnabledStatusId = 2;
        private const int RandomPasswordLength = 16;
        private const string PasswordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_-+=[]{}|;:,.<>?";
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

        private readonly UserDbContext _context;
        private readonly Configurations.ClientsDB.Mongo.SecurityAuth _jwtSettings;
        private readonly ISessionsService _sessionsService;

        public UsersService(UserDbContext context, IOptions<Configurations.ClientsDB.Mongo.SecurityAuth> jwtSettings, ISessionsService sessionsService)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
            _sessionsService = sessionsService;
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity entity)
        {
            string email = entity.Email.Trim();
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new DuplicateEmailException(email);

            if (string.IsNullOrWhiteSpace(entity.Password))
            {
                entity.Password = GenerateRandomPassword();
            }

            UserDtoContext user = new UserDtoContext
            {
                Name = entity.Name,
                LastName = entity.LastName,
                Email = email,
                Password = HashPassword(entity.Password),
                CreatedDate = DateTime.UtcNow,
                StatusId = EnabledStatusId
            };

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Another request may have inserted the same email between the check and the insert (unique index).
                _context.Entry(user).State = EntityState.Detached;
                if (await _context.Users.AnyAsync(u => u.Email == email))
                    throw new DuplicateEmailException(email);
                throw;
            }

            entity.Id = user.Id;
            entity.Email = user.Email;
            entity.CreatedDate = user.CreatedDate;
            entity.StatusId = user.StatusId;
            return entity;
        }

        private static string GenerateRandomPassword() =>
            RandomNumberGenerator.GetString(PasswordChars, RandomPasswordLength);

        private static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentNullException(nameof(plainPassword));

            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        public async Task<List<UserEntity>> GetUsers()
        {
            List<UserDtoContext> users = await _context.Users.ToListAsync();
            return users.Select(u => u.ToEntity()).ToList();
        }

        public async Task<UserEntity?> ValidateUserAsync(string email, string password)
        {
            UserDtoContext? userDto = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.StatusId == EnabledStatusId);
            if (userDto == null)
                return null;

            if (BCrypt.Net.BCrypt.Verify(password, userDto.Password))
                return userDto.ToEntity();

            return null;
        }

        public async Task<string> GenerateJwtTokenAsync(UserLoginEntity user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expiration = DateTime.UtcNow.Add(TokenLifetime);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (!string.IsNullOrWhiteSpace(user.Name))
                claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.Name));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtSettings.Authority,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            await _sessionsService.CreateSessionAsync(user, expiration, tokenString);

            return tokenString;
        }
    }
}
