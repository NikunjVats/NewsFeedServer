using System;
using System.Text.Json.Serialization;

namespace NewsFeedServer.Models;

public class NewStory
{
    [JsonPropertyName("title")]
    public required string Title { get; set; } 

    [JsonPropertyName("url")]
    public string? Url { get; set; } 

    [JsonPropertyName("id")]
    public int Id { get; set; } 

    [JsonPropertyName("by")]
    public required string By { get; set; } 

    [JsonPropertyName("time")]
    public long Time { get; set; } 

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; } 

    [JsonPropertyName("dead")]
    public bool Dead { get; set; } 
}
