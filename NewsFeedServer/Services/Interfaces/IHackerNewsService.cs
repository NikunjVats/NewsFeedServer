using System;
using NewsFeedServer.Models;

namespace NewsFeedServer.Services.Interfaces;

public interface IHackerNewsService
{
    public Task<List<News>> GetLatestNewsAsync(int limit);
}
