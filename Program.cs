using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using UsersApi.Configurations.ClientsDB.Mongo;
using UsersApi.Repository.MongoDb;
using UsersApi.Repository.SqlServer;
using UsersApi.Services.Implementations;
using UsersApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISessionsService, SessionsService>();
builder.Services.AddTransient<IUsersService, UsersService>();

builder.Services.Configure<SecurityAuth>(builder.Configuration.GetSection("SecurityAuth"));

// Fail fast with a clear message instead of a NullReferenceException deep in the JWT setup.
// Values can also come from env vars: SecurityAuth__Authority, SecurityAuth__Audience, SecurityAuth__SecretKey.
var securityAuth = configuration.GetSection("SecurityAuth").Get<SecurityAuth>();
if (string.IsNullOrWhiteSpace(securityAuth?.Authority) || string.IsNullOrWhiteSpace(securityAuth.Audience))
    throw new InvalidOperationException("SecurityAuth:Authority and SecurityAuth:Audience must be configured.");
if (string.IsNullOrWhiteSpace(securityAuth.SecretKey) || Encoding.UTF8.GetByteCount(securityAuth.SecretKey) < 32)
    throw new InvalidOperationException("SecurityAuth:SecretKey must be configured and at least 32 bytes long (required by HS256).");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// MongoDB Configuration
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// SQL Server Configuration
builder.Services.AddDbContext<UserDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SecurityAuth";
})
.AddJwtBearer("SecurityAuth", options =>
{
    // Tokens are self-issued and signed with a symmetric key, so no Authority is set:
    // setting it would make the handler try to download OIDC metadata from that URL.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = securityAuth.Authority,
        ValidateAudience = true,
        ValidAudience = securityAuth.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityAuth.SecretKey))
    };
});


builder.Services.AddControllers();

var app = builder.Build();

// In the Docker Compose environment the SQL Server container starts empty,
// so apply pending EF Core migrations on startup.
if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<UserDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();