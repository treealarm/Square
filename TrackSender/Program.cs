using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TrackSender.Authentication;

namespace TrackSender
{
  class Program
  {
    static public string GenerateBsonId()
    {
      return MongoDB.Bson.ObjectId.GenerateNewId().ToString();
    }

    static void Main(string[] args)
    {
      try
      {
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;

        HttpClient _client = new HttpClient();
        _client.BaseAddress = new Uri(App.Default.ServerAddress);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        KeyCloakConnectorService kcService = new KeyCloakConnectorService();
        kcService.GetOath2Token().Wait();

        _client.DefaultRequestHeaders.Add("authorization", "Bearer " + kcService.GetToken());
        _client.DefaultRequestHeaders.Add("cache-control", "no-cache");


        var testMove = new TestMovements(_client);
        var testStates = new TestStates(_client);
        var moscowBuilder = new NominatimProcessor();
        List<Task> tasks = new List<Task>();

        var taskMoscowBuild = moscowBuilder.RunAsync(token, tasks);
        Task.WaitAll(taskMoscowBuild);

        Task.WaitAll(testStates.BuildMoscow());

        testStates.RunAsync(token, tasks);        
        

        testMove.RunAsync(token, tasks);

        testMove.TestLogicAsync(token, tasks);

        if (!Console.IsInputRedirected && Console.KeyAvailable)
        {
          Console.WriteLine("Press ESC to stop emulation\n");
          ConsoleKeyInfo key = Console.ReadKey();

          while (key.Key != ConsoleKey.Escape)
          {
            key = Console.ReadKey();
          }

          tokenSource.Cancel();
        }
          

        Task.WaitAll(tasks.ToArray());
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
  }
}
