namespace GrpcDaprLib
{
  public class GrpcUpdaterClient<TClient> : GrpcBaseUpdater where TClient : class
  {
    public TClient? Client { get; protected set; }

    public GrpcUpdaterClient() : base()
    {
      // Универсальный способ создания клиента через рефлексию
      Client = Activator.CreateInstance(typeof(TClient), _daprClient) as TClient;

      if (Client == null)
      {
        Console.Error.WriteLine($"Failed to create client of type {typeof(TClient).Name}");
      }
    }

    public bool IsDead => Client == null;
  }
}
