
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFeedServer.Models;
using NewsFeedServer.Services.Interfaces;

namespace NewsFeedServer.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsFeedController : ControllerBase
{
    private IHackerNewsService _hackerNewsService;
    private IAuthenticationService _authenticationService;

    public NewsFeedController(IHackerNewsService hackerNewsService, IAuthenticationService authenticationService)
    {
        _hackerNewsService = hackerNewsService;
        _authenticationService = authenticationService;
    }

    [Authorize]
    [HttpGet("GetLatestNews/{limit:int}")]
    public async Task<IActionResult> GetLatestNews(int limit = 10)
    {
        try
        {
            if (limit <= 0 || limit > 200)
            {
                return BadRequest("Limit must be between 1 and 200.");
            }

            var stories = await _hackerNewsService.GetLatestNewsAsync(limit);
            return Ok(stories);
        }
        catch
        {
            // log the error
            return Ok();
        }
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] User user) {
        try
        {
            var jwt = _authenticationService.Authenticate(user);
            return Ok(new { token = jwt });
        }
        catch (System.Exception)
        {
            // log the error
            return Ok(new { token = "" });
        }
    }

}
