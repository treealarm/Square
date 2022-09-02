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
   

    static void Main(string[] args)
    {
      try
      {
        var testMove = new TestMovements();
        testMove.RunAsync().GetAwaiter().GetResult();
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
  }
}
