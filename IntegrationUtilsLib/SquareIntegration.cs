using Domain;
using SquareIntegrationClient;

namespace IntegrationUtilsLib
{
  /// <summary>
  /// Ambient default sink for Square's own internal producers (AASubService, GrpcTracksClient) —
  /// points at this Square instance's own integration host via LEAFLETALARM_APP_ID/APP_ID.
  /// External producers (e.g. vms_rec) construct SquareIntegrationGrpcClient directly with their
  /// own sink/app id instead — see docs/square-integration-plan.md in vms_rec.
  /// </summary>
  public static class SquareIntegration
  {
    // Read once, fail fast and stay failed: a missing env var is a permanent misconfiguration, not
    // a transient startup race — it's just as missing on the next attempt as the first, so unlike
    // the Dapr-sidecar-readiness check below, it must NOT be silently retried forever. A throwing
    // static field initializer gives exactly that: the CLR caches the failure as
    // TypeInitializationException and every subsequent access to this class throws it again.
    private static readonly string LeafletAlarmAppId = EnvConfig.Require("LEAFLETALARM_APP_ID");
    private static readonly string AppId = EnvConfig.Require("APP_ID");

    private static ISquareIntegration? _default;
    private static readonly object _lock = new object();

    // На старте sidecar dapr может быть ещё не готов — конструктор может бросить исключение.
    // Здесь намеренно не используется Lazy<T>: Lazy<T> кэширует исключение навсегда, что навсегда
    // ломает переподключение. Оставляя поле null при неудаче, следующий доступ повторит попытку.
    public static ISquareIntegration Default
    {
      get
      {
        if (_default != null) { return _default; }
        lock (_lock)
        {
          _default ??= new SquareIntegrationGrpcClient(LeafletAlarmAppId, AppId);
          return _default;
        }
      }
    }
  }
}
