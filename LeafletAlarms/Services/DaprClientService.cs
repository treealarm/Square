using Dapr.Client;
using Grpc.Core;

namespace LeafletAlarms.Services
{
  public interface IDaprClientService
  {
    CallInvoker GetDaprClient(string appId);
  }

  public class DaprClientService : IDaprClientService
  {
    private readonly Dictionary<string, CallInvoker> _clientCache = new Dictionary<string, CallInvoker>();

    public CallInvoker GetDaprClient(string appId)
    {
      if (!_clientCache.ContainsKey(appId))
      {
        // Создаём новый DaprClient, если его ещё нет в кэше
        var client = DaprClient.CreateInvocationInvoker(appId);
        _clientCache[appId] = client;
      }

      // Возвращаем уже созданный или кэшированный клиент
      return _clientCache[appId];
    }
  }

}
