using Domain.GeoDBDTO;
using GrpcDaprLib;
using LeafletAlarmsGrpc;
using System.Text.Json;

namespace GrpcTracksClient
{
  internal class UpdateSADTracks
  {
    static GrpcUpdater _client = new GrpcUpdater();
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
        StrVal = "sad"
      });

      for (int j = 0; j < 1000000; j++)
      {
        await Task.Delay(1);
        try
        {
          await MoveGrpc(figs, fig);
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
        }
      }      
    }



    public static async Task MoveGrpc(TrackPointsProto figs, ProtoGeoObject fig)
    {
      var resourceName = $"GrpcTracksClient.JSON.SAD.json";
      var s = await ResourceLoader.GetResource(resourceName);

      var coords = JsonSerializer.Deserialize<GeometryPolylineDTO>(s);
      foreach (var c in coords?.coord)
      {
        foreach (var f in fig.Location.Coord)
        {
          f.Lat = c.Lat;
          f.Lon = c.Lon;
        }
        var newFigs = await _client.UpdateTracks(figs);
        //Console.WriteLine("Tracks GRPC: " + newFigs?.ToString());
        await Task.Delay(1000);
      }
    }
  }
}
