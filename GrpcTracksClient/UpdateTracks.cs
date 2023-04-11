using Domain.GeoDBDTO;
using Grpc.Net.Client;
using LeafletAlarmsGrpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static LeafletAlarmsGrpc.TracksGrpcService;

namespace GrpcTracksClient
{
  internal class UpdateTracks
  {
    public static async Task Move()
    {
      var figs = new TrackPointsProto();
      var track = new TrackPointProto();
      figs.Tracks.Add(track);

      track.Figure = new ProtoGeoObject();
      var fig = track.Figure;

      fig.Location = new ProtoGeometry();

      fig.Location.Type = "Point";

      fig.Location.Coord.Add(new ProtoCoord()
      {
        Lat = 55.755864,
        Lon = 37.617698
      });


      track.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert"
      });

      track.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert1"
      });

      while (true)
      {
        try
        {
          await MoveGrpc(figs, fig);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }
      }      
    }

    static public async Task<string> GetResource(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();

      string s = string.Empty;
      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      using (StreamReader reader = new StreamReader(stream))
      {
        s = await reader.ReadToEndAsync();
      }
      return s;
    }

    public static async Task MoveGrpc(TrackPointsProto figs, ProtoGeoObject fig)
    {
      var resourceName = $"GrpcTracksClient.JSON.SAD.json";
      var s = await GetResource(resourceName);

      var coords = JsonSerializer.Deserialize<GeometryPolylineDTO>(s);

      using var channel = GrpcChannel.ForAddress(ProgramConstants.GRPC_DST);
      var client = new TracksGrpcServiceClient(channel);

      foreach (var c in coords?.coord)
      {
        foreach (var f in fig.Location.Coord)
        {
          f.Lat = c.Lat;
          f.Lon = c.Lon;
        }
        var newFigs = await client.UpdateTracksAsync(figs);
        Console.WriteLine("Tracks GRPC: " + newFigs?.ToString());
        await Task.Delay(1000);
      }
    }
  }
}
