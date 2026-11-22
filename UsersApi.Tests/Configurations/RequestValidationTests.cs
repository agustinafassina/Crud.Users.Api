using System.ComponentModel.DataAnnotations;
using UsersApi.Configurations.Mappers.Requests;

namespace UsersApi.Tests.Configurations;

public class RequestValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void UserCreateDtoRequest_InvalidEmail_FailsValidation(string email)
    {
        var errors = Validate(new UserCreateDtoRequest { Email = email, Password = "Password1!" });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserCreateDtoRequest.Email)));
    }

    [Fact]
    public void UserCreateDtoRequest_ShortPassword_FailsValidation()
    {
        var errors = Validate(new UserCreateDtoRequest { Email = "a@example.com", Password = "short" });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserCreateDtoRequest.Password)));
    }

    [Fact]
    public void UserCreateDtoRequest_WithoutPassword_IsValid()
    {
        var errors = Validate(new UserCreateDtoRequest { Email = "a@example.com" });

        Assert.Empty(errors);
    }

    [Fact]
    public void LoginRequest_MissingFields_FailsValidation()
    {
        var errors = Validate(new LoginRequest());

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(LoginRequest.Email)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(LoginRequest.Password)));
    }
}
