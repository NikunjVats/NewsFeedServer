using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using NewsFeedServer.Models;
using NewsFeedServer.Services.Interfaces;

namespace NewsFeedServer.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";
    private const string cacheKey = "AllNews";

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache, IConfiguration config)
    {
        _httpClient = httpClient;
        _cache = cache;
        _config = config;
    }

    public async Task<List<News>> GetLatestNewsAsync(int limit = 10)
    {
        try
        {
            var allNews = new List<News>();
            if (_cache.TryGetValue(cacheKey + limit, out List<News>? allCachedNews) && allCachedNews != null)
            {
                allNews = allCachedNews;
            }
            else
            {
                // Get IDs of Latest News
                var response = await _httpClient.GetStringAsync($"{BaseUrl}/newstories.json");
                var storyIds = JsonSerializer.Deserialize<List<int>>(response);

                if (storyIds != null)
                {
                    var tasks = new List<Task>();

                    await Parallel.ForEachAsync(storyIds.Take(limit), async (id, cancellationToken) =>
                    {
                        var storyJson = await _httpClient.GetStringAsync($"{BaseUrl}/item/{id}.json");
                        var story = JsonSerializer.Deserialize<NewStory>(storyJson);

                        if (story != null && !story.Deleted && !story.Dead)
                        {
                            lock (allNews)
                            {
                                allNews.Add(new News
                                {
                                    Id = story.Id,
                                    Title = story.Title,
                                    Url = string.IsNullOrEmpty(story.Url) ? "" : story.Url,
                                    Time = DateTimeOffset.FromUnixTimeSeconds(story.Time).DateTime.Date.ToString("dd-MM-yyyy"),
                                    By = string.IsNullOrEmpty(story.By) ? "" : story.By
                                });
                            }
                        }
                    });
                }
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(int.Parse(_config["AppKeys:CachedDurationInMinutes"])));
                _cache.Set(cacheKey + limit, allNews);
            }

            return allNews;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching stories from Hacker News", ex);
        }
    }
}
