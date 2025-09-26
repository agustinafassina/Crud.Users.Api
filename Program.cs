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

builder.Services.AddAutoMapper(typeof(UsersApi.Configurations.Mappers.ContractMapping));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISessionsService, SessionsService>();
builder.Services.AddTransient<IUsersService, UsersService>();

builder.Services.Configure<SecurityAuth>(builder.Configuration.GetSection("SecurityAuth"));

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
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("SecurityAuth", options =>
{
    options.Audience = configuration["SecurityAuth:Audience"] ?? Environment.GetEnvironmentVariable("SecurityAuth.Audience");
    options.Authority = configuration["SecurityAuth:Authority"] ?? Environment.GetEnvironmentVariable("SecurityAuth.Authority");

    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = options.Authority,
        ValidateAudience = true,
        ValidAudience = options.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SecurityAuth:SecretKey"]))
    };
});


builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();