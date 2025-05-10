using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NewsFeedServer.Services;
using NewsFeedServer.Models;

public class HackerNewsServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;

    public HackerNewsServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _cacheMock = new Mock<IMemoryCache>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object);
    }

    [Fact]
    public async Task GetLatestNewsAsync_ReturnsNews_FromCache()
    {
        // Arrange
        var cachedNews = new List<News> {
            new News { Id = 1, Title = "Cached Story", Url = "", By = "author", Time = "01-01-2024" }
        };

        object dummy = cachedNews;
        _cacheMock.Setup(c => c.TryGetValue("AllNews10", out dummy)).Returns(true);

        var service = new HackerNewsService(_httpClient, _cacheMock.Object, _configMock.Object);

        // Act
        var result = await service.GetLatestNewsAsync(10);

        // Assert
        Assert.Single(result);
        Assert.Equal("Cached Story", result[0].Title);
    }

    [Fact]
    public async Task GetLatestNewsAsync_FetchesFromApi_WhenCacheIsEmpty()
    {
        // Arrange
        object outVal;
        _cacheMock.Setup(c => c.TryGetValue("AllNews5", out outVal)).Returns(false);

        _configMock.Setup(c => c["AppKeys:CachedDurationInMinutes"]).Returns("5");

        var storyIds = new List<int> { 101 };
        var story = new NewStory
        {
            Id = 101,
            Title = "Fresh Story",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Url = "https://example.com/story",
            By = "john",
            Deleted = false,
            Dead = false
        };

        string baseUrl = "https://hacker-news.firebaseio.com/v0/";

        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get && r.RequestUri.ToString() == $"{baseUrl}/newstories.json"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(storyIds))
            });

        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get && r.RequestUri.ToString() == $"{baseUrl}/item/101.json"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(story))
            });

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new HackerNewsService(_httpClient, memoryCache, _configMock.Object);

        // Act
        var result = await service.GetLatestNewsAsync(5);

        // Assert
        Assert.Single(result);
        Assert.Equal("Fresh Story", result[0].Title);
        Assert.Equal("john", result[0].By);
    }

    [Fact]
    public async Task GetLatestNewsAsync_ThrowsException_OnHttpFailure()
    {
        // Arrange
        object outVal;
        _cacheMock.Setup(c => c.TryGetValue("AllNews10", out outVal)).Returns(false);
        _configMock.Setup(c => c["AppKeys:CachedDurationInMinutes"]).Returns("10");

        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("API failed"));

        var service = new HackerNewsService(_httpClient, _cacheMock.Object, _configMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetLatestNewsAsync(10));
        Assert.Contains("Error fetching stories", ex.Message);
    }
}
