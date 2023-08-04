using Dapr.Client;
using Domain.GeoDBDTO;
using Grpc.Net.Client;
using GrpcDaprClientLib;
using GrpcDaprLib;
using LeafletAlarmsGrpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Dapr.Client.Autogen.Grpc.v1.Dapr;
using static LeafletAlarmsGrpc.TracksGrpcService;

namespace GrpcTracksClient
{
  internal class MoveObject
  { 
    public static async  Task Move()
    {
      var figs = new ProtoFigures();
      var fig = new ProtoFig();
      figs.Figs.Add(fig);

      fig.Id = "6423e54d513bfe83e9d59793";
      fig.Name = "Test";
      fig.Geometry = new ProtoGeometry();
      fig.Geometry.Type = "Polygon";
      figs.AddTracks = true;

      fig.Geometry.Coord.Add(new ProtoCoord()
      {
        Lat = 55.7566737398449,
        Lon = 37.60722931951715
      });

      fig.Geometry.Coord.Add(new ProtoCoord()
      {
        Lat = 55.748852242908995,
        Lon = 37.60259563134112
      });

      fig.Geometry.Coord.Add(new ProtoCoord()
      {
        Lat = 55.75203896803514,
        Lon = 37.618727730916895
      });

      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert"
      });

      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert2"
      });

      await MoveGrpc(figs, fig);
      await MoveDapr(figs, fig);
    }

    public static async Task MoveDapr(ProtoFigures figs, ProtoFig fig)
    {
      using var daprClient = new DaprMover();

      double step = 0.001;

      for (int i = 0; i < 100; i++)
      {
        try
        {
          if (i > 50)
          {
            step = -0.001;
          }
          foreach (var f in fig.Geometry.Coord)
          {
            f.Lat += step;
            f.Lon += step;
          }
          var reply =
            await daprClient.Move(figs);
          Console.WriteLine("Fig DAPR: " + reply?.ToString());
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          break;
        }
        await Task.Delay(1000);
      }
    }

    public static async Task MoveGrpc(ProtoFigures figs, ProtoFig fig)
    {
      using var client = new GrpcMover();
      client.Connect(null);
      var step = 0.001;

      for (int i = 0; i < 100; i++)
      {
        if (i > 50)
        {
          step = -0.001;
        }
        foreach (var f in fig.Geometry.Coord)
        {
          f.Lat += step;
          f.Lon += step;
        }
        var newFigs = await client.Move(figs);
        Console.WriteLine("Fig GRPC: " + newFigs?.ToString());
        await Task.Delay(1000);
      }
    }
  }
}
