using Xunit;
using Moq;
using NewsFeedServer.Controllers;
using NewsFeedServer.Services.Interfaces;
using NewsFeedServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NewsFeedControllerTests
{
    private readonly Mock<IHackerNewsService> _hackerNewsServiceMock;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly NewsFeedController _controller;

    public NewsFeedControllerTests()
    {
        _hackerNewsServiceMock = new Mock<IHackerNewsService>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _controller = new NewsFeedController(_hackerNewsServiceMock.Object, _authenticationServiceMock.Object);
    }

    [Fact]
    public async Task GetLatestNews_ReturnsOk_WithStories()
    {
        // Arrange
        int limit = 5;
        var mockStories = new List<News>
        {
            new News { Id = 1, Title = "Title 1", Url = null },
            new News { Id = 2, Title = "Title 2", Url = "URL 2" }
        };

        _hackerNewsServiceMock.Setup(s => s.GetLatestNewsAsync(limit)).ReturnsAsync(mockStories);

        // Act
        var result = await _controller.GetLatestNews(limit);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(mockStories, okResult.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public async Task GetLatestNews_InvalidLimit_ReturnsBadRequest(int limit)
    {
        // Act
        var result = await _controller.GetLatestNews(limit);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Limit must be between 1 and 200.", badRequest.Value);
    }

    [Fact]
    public async Task GetLatestNews_Exception_ReturnsOk()
    {
        // Arrange
        _hackerNewsServiceMock.Setup(s => s.GetLatestNewsAsync(It.IsAny<int>())).ThrowsAsync(new System.Exception());

        // Act
        var result = await _controller.GetLatestNews(10);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void Login_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = new User { Name = "test" };
        var expectedToken = "fake-jwt-token";

        _authenticationServiceMock.Setup(s => s.Authenticate(user)).Returns(expectedToken);

        // Act
        var result = _controller.Login(user);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        var tokenProperty = value.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);

        var actualToken = tokenProperty.GetValue(value) as string;
        Assert.Equal(expectedToken, actualToken);
    }

    [Fact]
    public void Login_Exception_ReturnsEmptyToken()
    {
        // Arrange
        var user = new User { Name = "test" };
        _authenticationServiceMock.Setup(s => s.Authenticate(user)).Throws(new System.Exception());

        // Act
        var result = _controller.Login(user);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        var tokenProperty = value.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);

        var actualToken = tokenProperty.GetValue(value) as string;
        Assert.Equal("", actualToken);
    }
}
