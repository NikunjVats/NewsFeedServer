using System;

namespace NewsFeedServer.Models;

public class News 
{ 
    public required string Title { get; set; } 
    public string? Url { get; set; } 
    public int Id { get; set; } 
    public string? Time { get; set; } 
    public string? By { get; set; } 
}
