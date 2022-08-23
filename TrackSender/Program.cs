using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;

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
   

    static void Main(string[] args)
    {
      try
      {
        RunAsync().GetAwaiter().GetResult();
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
    static async Task RunAsync()
    {
      await TestMovements.RunAsync();
    }
  }
}
