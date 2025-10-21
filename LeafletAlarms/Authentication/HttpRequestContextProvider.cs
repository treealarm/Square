using Domain;

namespace LeafletAlarms.Authentication
{
  public class HttpRequestContextProvider : IRequestContextProvider
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestContextProvider(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public string? GetRealm()
    {
      return _httpContextAccessor.HttpContext?.User.FindFirst("iss")?.Value?
          .Split('/', StringSplitOptions.RemoveEmptyEntries)
          .LastOrDefault(); // извлекаем имя реалма из Issuer URL
    }

    public string? GetUserName()
    {
      return _httpContextAccessor.HttpContext?.User.Identity?.Name;
    }
  }
}
