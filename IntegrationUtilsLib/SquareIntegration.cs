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
          _default ??= new SquareIntegrationGrpcClient(
            EnvConfig.Require("LEAFLETALARM_APP_ID"),
            EnvConfig.Require("APP_ID"));
          return _default;
        }
      }
    }
  }
}
