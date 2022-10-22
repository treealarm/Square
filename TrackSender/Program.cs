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

        var testMove = new TestMovements();
        var testStates = new TestStates();
        var moscowBuilder = new NominatimProcessor();
        List<Task> tasks = new List<Task>();

        var taskMoscowBuild = moscowBuilder.RunAsync(token, tasks);
        Task.WaitAll(taskMoscowBuild);

        var taskStates = testStates.RunAsync(token, tasks);        
        tasks.Add(taskStates);

        //var taskMove = testMove.RunAsync(token, tasks);
        //tasks.Add(taskMove);

        Console.WriteLine("Press any key to stop emulation\n");
        Console.ReadKey();
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
