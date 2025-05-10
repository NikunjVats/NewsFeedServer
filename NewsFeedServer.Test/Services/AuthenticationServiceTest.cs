using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using NewsFeedServer.Models;
using NewsFeedServer.Services;
using Xunit;

public class AuthenticationServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForJwtToken123!LongValueJustRandomString");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configurationMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

        _authService = new AuthenticationService(_configurationMock.Object);
    }

    [Fact]
    public void Authenticate_ValidUser_ReturnsJwtToken()
    {
        // Arrange
        var user = new User { Name = "TestUser" };

        // Act
        var token = _authService.Authenticate(user);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));

        var handler = new JwtSecurityTokenHandler();

        var jwtToken = handler.ReadJwtToken(token);
        Assert.Contains(jwtToken.Claims, c => c.Type == "Name" && c.Value == "TestUser");
    }

    [Fact]
    public void Authenticate_InvalidConfig_ThrowsException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns<string>(null); // Missing key
        var authService = new AuthenticationService(configMock.Object);
        var user = new User { Name = "TestUser" };

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => authService.Authenticate(user));
        Assert.Contains("Error Authenticating the user", ex.Message);
    }
}
