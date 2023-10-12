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
      var taskCar = MoveCar();

      await MovePolygon();

      Task.WaitAll(taskCar);
    }

    private static async Task MoveCar()
    {
      var figs = new ProtoFigures();
      var fig = new ProtoFig();
      figs.Figs.Add(fig);

      fig.Id = "6423e54d513bfe83e9d59794";
      fig.Name = "TestCar";
      fig.Geometry = new ProtoGeometry();
      fig.Geometry.Type = "Point";
      fig.Radius = 50;
      figs.AddTracks = true;

      fig.Geometry.Coord.Add(new ProtoCoord()
      {
        Lat = 55.7566737398449,
        Lon = 37.60722931951715
      });

      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert"
      });

      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = @"__image",
        StrVal = @"images/car_red_256.png"
      });

      await MoveGrpcCar(figs, fig);
    }
    private static async Task MovePolygon()
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
          try
          {
            var reply =
            await daprClient.Move(figs);
            Console.WriteLine("Fig DAPR: " + reply?.ToString());
          }
          catch(Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
          
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

    public static double CalculateAzimuth(double latitude1, double longitude1, double latitude2, double longitude2)
    {
      // Переводим координаты из градусов в радианы
      double phi1 = latitude1 * Math.PI / 180.0;
      double lambda1 = longitude1 * Math.PI / 180.0;
      double phi2 = latitude2 * Math.PI / 180.0;
      double lambda2 = longitude2 * Math.PI / 180.0;

      // Вычисляем разницу в долготе
      double deltaLambda = lambda2 - lambda1;

      // Вычисляем азимут
      double azimuth = Math.Atan2(Math.Sin(deltaLambda), Math.Cos(phi1) * Math.Tan(phi2) - Math.Sin(phi1) * Math.Cos(deltaLambda));

      // Переводим азимут из радианов в градусы
      azimuth = azimuth * 180.0 / Math.PI;

      return azimuth;
    }

    public static async Task MoveGrpcCar(ProtoFigures figs, ProtoFig fig)
    {
      var rotate = new ProtoObjExtraProperty()
      {
        PropName = @"__image_rotate",
        StrVal = @"0"
      };
      fig.ExtraProps.Add(rotate);

      var resourceName = $"GrpcTracksClient.JSON.SAD.json";
      var s = await ResourceLoader.GetResource(resourceName);

      var coords = JsonSerializer.Deserialize<GeometryPolylineDTO>(s);

      using var client = new GrpcMover();
      client.Connect(null);
      var prev = new Geo2DCoordDTO() { 0, 0 };

      foreach (var c in coords?.coord)
      {
        var degrees = CalculateAzimuth(prev.Lat, prev.Lon, c.Lat, c.Lon);

        int rot = (int)(degrees);
        rotate.StrVal = rot.ToString();
        prev = c;

        foreach (var f in fig.Geometry.Coord)
        {          
          f.Lat = c.Lat;//y
          f.Lon = c.Lon;//x
        }

        
        var newFigs = await client.Move(figs);
        Console.WriteLine("Car GRPC: " + newFigs?.ToString());
        await Task.Delay(100);
      }
    }
  }
}
