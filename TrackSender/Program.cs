using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using LeafletAlarmsRouter;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

        //TestClient testClient = new TestClient();

        //var t1 = DateTime.Now;

        //for (int i = 0; i < 10000; i++)
        //{
        //  testClient.Empty("test").Wait();
        //}

        //Console.WriteLine($"10K calls:{(DateTime.Now - t1).TotalSeconds}");
        //return;

        var taskMoscowBuild = moscowBuilder.RunAsync(token, tasks);
        Task.WaitAll(taskMoscowBuild);

        var taskStates = testStates.RunAsync(token, tasks);        
        tasks.Add(taskStates);

        var taskMove = testMove.RunAsync(token, tasks);
        tasks.Add(taskMove);

        testMove.TestLogicAsync(token, tasks);

        Console.WriteLine("Press any key to stop emulation\n");
        ConsoleKeyInfo key = Console.ReadKey();

        while (key.Key != ConsoleKey.Escape)
        {
          key = Console.ReadKey();
        }
        
        tokenSource.Cancel();

        Task.WaitAll(tasks.ToArray());
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
  }
}
