namespace GrpcDaprLib
{
  public class Utils
  {
    // Статическое поле для хранения клиента
    private static GrpcUpdater? _client;
    private static Dictionary<string,string> _idsCash = new Dictionary<string, string>();
    private static readonly object _lock = new object(); // Для синхронизации

    // Метод для доступа к клиенту
    public static GrpcUpdater Client
    {
      get
      {
        // Проверяем, если клиент не существует или мертв, создаем новый
        if (_client == null || _client.IsDead)
        {
          lock (_lock)
          {
            if (_client == null || _client.IsDead)
            {
              _client = new GrpcUpdater();  // Инициализация нового клиента
            }
          }
        }
        return _client;
      }
    }

    public static async Task<string?> GenerateObjectId(string prefix, long number)
    {
      //return "1111" + number.ToString("D20");
      var object_string = $"{prefix}_{number}";
      lock (_lock)
      {
        if (_idsCash.TryGetValue(object_string, out var id))
        {
          return id;
        }
      }
      var obj_id = await Client.GenerateObjectId(object_string)  ?? null;
      lock (_lock)
      {
        if (!string.IsNullOrEmpty(obj_id))
        {
          _idsCash[object_string] = obj_id;
        }             
      }
      return obj_id;
    }

    public static async Task RunTaskWithRetry(Func<Task> taskFunc, string taskName, CancellationToken token)
    {
      while (!token.IsCancellationRequested)
      {
        try
        {
          await taskFunc();
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
          await Task.Delay(1000, token); // Задержка перед повторной попыткой
        }
      }
    }
  }

}
