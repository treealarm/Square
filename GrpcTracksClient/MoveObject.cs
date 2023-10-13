using Dapr.Client;
using Domain.GeoDBDTO;
using Google.Protobuf.WellKnownTypes;
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
      List<Task> listCarsTasks = new List<Task>();
      int index = 0;
      for (long carId = 0; carId < 50; carId++)
      {
        index += 5;
        var taskCar = MoveCar(index);
        listCarsTasks.Add(taskCar);
      }
      

      await MovePolygon();

      Task.WaitAll(listCarsTasks.ToArray());
    }

    static string LongTo24String(long number)
    {
      return "1111" + number.ToString("D20");
    }
    private static async Task MoveCar(long number)
    {
      var figs = new ProtoFigures();
      var fig = new ProtoFig();
      figs.Figs.Add(fig);

      fig.Id = LongTo24String(number); //"6423e54d513bfe83e9d59794";
      fig.Name = "TestCar" + number.ToString();
        
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

      await MoveGrpcCar(figs, fig, number);
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

      var grpcTask = MoveGrpc(figs, fig);

      fig.Geometry.Coord.Clear();

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
      await MoveDapr(figs, fig);

      Task.WaitAll(grpcTask);
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
            //Console.WriteLine("Fig DAPR: " + reply?.ToString());
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
      Random random = new Random();

      var center = new ProtoCoord()
      {
        Lat = 55.753201,
        Lon = 37.621130
      };

      using var client = new GrpcMover();
      client.Connect(null);
      var step = 0.0001;

      for (int i = 0; i < 1000000; i++)
      {
        center.Lat += random.Next(-5, +5)*step;
        center.Lon += random.Next(-5, +5)*step;

        fig.Geometry.Coord.Clear();
        var rad = 400;

        var aStep = random.Next(20, 90);

        for (double a = 0; a < 360; a+= aStep)
        {
          rad += random.Next(-20, +20);
          var pt = CalculateCoordinates(center.Lat, center.Lon, rad, a);
          fig.Geometry.Coord.Add(new ProtoCoord()
          {
            Lat = pt.latitude,
            Lon = pt.longitude
          });
        }        

        var newFigs = await client.Move(figs);
        //Console.WriteLine("Fig GRPC: " + newFigs?.ToString());
        await Task.Delay(1000);
      }
    }

    public static (double latitude, double longitude) 
      CalculateCoordinates(double startingLatitude, double startingLongitude, double distance, double azimuth)
    {
      // Переводим координаты из градусов в радианы
      double phi1 = startingLatitude * Math.PI / 180.0;
      double lambda1 = startingLongitude * Math.PI / 180.0;
      double alpha = azimuth * Math.PI / 180.0;

      // Радиус Земли (приближенное значение в метрах)
      double earthRadius = 6371000;

      // Вычисляем новую широту и долготу
      double delta = distance / earthRadius;
      double phi2 = Math.Asin(Math.Sin(phi1) * Math.Cos(delta) + Math.Cos(phi1) * Math.Sin(delta) * Math.Cos(alpha));
      double lambda2 = lambda1 + Math.Atan2(Math.Sin(alpha) * Math.Sin(delta) * Math.Cos(phi1), Math.Cos(delta) - Math.Sin(phi1) * Math.Sin(phi2));

      // Переводим координаты обратно в градусы
      double newLatitude = phi2 * 180.0 / Math.PI;
      double newLongitude = lambda2 * 180.0 / Math.PI;

      return (newLatitude, newLongitude);
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

    public static async Task MoveGrpcCar(ProtoFigures figs, ProtoFig fig, long startPt)
    {
      var center = new ProtoCoord()
      {
        Lat = 55.753201,
        Lon = 37.621130
      };

      var rotate = new ProtoObjExtraProperty()
      {
        PropName = @"__image_rotate",
        StrVal = @"0"
      };
      fig.ExtraProps.Add(rotate);

      var resourceName = $"GrpcTracksClient.JSON.SAD.json";
      var s = await ResourceLoader.GetResource(resourceName);

      var coords = JsonSerializer.Deserialize<GeometryPolylineDTO>(s);
      //var coords = new GeometryPolylineDTO();
      //coords.coord = new List<Geo2DCoordDTO>();

      //for (double a = 0; a < 360; a += 0.3)
      //{
      //  var pt = CalculateCoordinates(center.Lat, center.Lon, 3000, a);
      //  coords.coord.Add(new Geo2DCoordDTO()
      //  {
      //    Lat = pt.latitude,
      //    Lon = pt.longitude
      //  });
      //}

      using var client = new GrpcMover();
      client.Connect(null);
      var prev = new Geo2DCoordDTO() { 0, 0 };

      for(int h = 0; h <  10;h++)
      {
        for (long track = startPt; track < coords?.coord.Count; track++)
        {
          var c = coords?.coord[(int)track];
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
          //Console.WriteLine("Car GRPC: " + newFigs?.ToString());
          await Task.Delay(500);
        }
        startPt = 0;
      }      
    }
  }
}
