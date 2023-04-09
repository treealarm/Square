using Grpc.Net.Client;
using LeafletAlarmsGrpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

      await MoveGrpc(figs, fig);
    }

    public static async Task MoveGrpc(TrackPointsProto figs, ProtoGeoObject fig)
    {
      using var channel = GrpcChannel.ForAddress("http://localhost:5000");
      var client = new TracksGrpcServiceClient(channel);
      var step = 0.001;

      for (int i = 0; i < 100; i++)
      {
        if (i > 50)
        {
          step = -0.001;
        }
        foreach (var f in fig.Location.Coord)
        {
          f.Lat += step;
          //f.Lon += step;
        }
        var newFigs = await client.UpdateTracksAsync(figs);
        Console.WriteLine("Tracks GRPC: " + newFigs?.ToString());
        await Task.Delay(1000);
      }
    }
  }
}
