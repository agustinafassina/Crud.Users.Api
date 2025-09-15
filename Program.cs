using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using UsersApi.Repository.MongoClient.Settings;
using UsersApi.Services.Implementations;
using UsersApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddAutoMapper(typeof(UsersApi.Mappers.ContractMapping));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISessionsService, SessionsService>();

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Auth0App1", options =>
{
    options.Audience = configuration["Auth0App1:Audience"] ?? Environment.GetEnvironmentVariable("Auth0App1.Audience");
    options.Authority = configuration["Auth0App1:Issuer"] ?? Environment.GetEnvironmentVariable("Auth0App1.Issuer");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = configuration["Auth0App1:Issuer"] ?? Environment.GetEnvironmentVariable("Auth0App1.Issuer")
    };
})
.AddJwtBearer("Auth0App2", options =>
{
    options.Audience = configuration["Auth0App2:Audience"] ?? Environment.GetEnvironmentVariable("Auth0App2.Audience");
    options.Authority = configuration["Auth0App2:Issuer"] ?? Environment.GetEnvironmentVariable("Auth0App2.Issuer");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = configuration["Auth0App2:Issuer"] ?? Environment.GetEnvironmentVariable("Auth0App2.Issuer")
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

app.MapControllers();

app.Run();