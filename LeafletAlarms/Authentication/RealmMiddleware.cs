namespace LeafletAlarms.Authentication
{
  public class RealmMiddleware
  {
    private readonly RequestDelegate _next;

    public RealmMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var realmClaim = context.User.FindFirst("iss")?.Value;
      if (!string.IsNullOrEmpty(realmClaim))
      {
        var realm = realmClaim.Split('/').Last();
        context.Items["realm"] = realm;
      }

      await _next(context);
    }
  }

}
