using Domain;
using Grpc.Core;

namespace LeafletAlarms.Authentication
{
  public class GrpcRequestContextProvider : IRequestContextProvider
  {
    private static readonly AsyncLocal<ServerCallContext?> _current = new();

    public static void SetContext(ServerCallContext context)
    {
      _current.Value = context;
    }

    public static void ClearContext()
    {
      _current.Value = null;
    }

    public string? GetRealm()
    {
      var ctx = _current.Value;
      return ctx?.GetHttpContext()?.User.FindFirst("iss")?.Value?
          .Split('/', StringSplitOptions.RemoveEmptyEntries)
          .LastOrDefault();
    }

    public string? GetUserName()
    {
      var ctx = _current.Value;
      return ctx?.GetHttpContext()?.User.Identity?.Name;
    }
  }
}
