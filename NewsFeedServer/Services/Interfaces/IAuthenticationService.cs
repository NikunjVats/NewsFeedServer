using System;
using NewsFeedServer.Models;

namespace NewsFeedServer.Services.Interfaces;

public interface IAuthenticationService
{
    public string Authenticate(User user);
}
