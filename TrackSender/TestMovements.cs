using Domain.GeoDBDTO;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Itinero.Profiles;
using Itinero;

namespace TrackSender
{
  internal class TestMovements
  {
    public static async Task RunAsync()
    {
      var start =
        new GeometryCircleDTO(new Geo2DCoordDTO() { 51.51467784097949, -0.1486710157204226 });

      var end =
        new GeometryCircleDTO(new Geo2DCoordDTO() { 51.1237f, 1.3134f });

      var markers = await TestClient.GetByName("test_track");
      FiguresDTO figures;
      FigureCircleDTO figure;

      if (markers == null || markers.Count == 0)
      {
        figures = new FiguresDTO();
        figures.circles = new List<FigureCircleDTO>();
        figure = new FigureCircleDTO()
        {
          name = "test_track",
          radius = 333,
          zoom_level = "13"
        };
        figures.circles.Add(figure);        
      }
      else
      {
        figures = await TestClient.GetByIds(new List<string>() { markers.FirstOrDefault().id });
      }

      figure = figures.circles.FirstOrDefault();

      if (figure == null)
      {
        return;
      }

      figure.geometry = start;
      await TestClient.UpdateFiguresAsync(figures, "AddTracks");

      markers = await TestClient.GetByName("test_track");
      figures = await TestClient.GetByIds(new List<string>() { markers.FirstOrDefault().id });
      figure = figures.circles.FirstOrDefault();

      figure.geometry = end;
      await TestClient.UpdateFiguresAsync(figures, "UpdateTracks");

      //var stat_y = figure.geometry.coord[0];
      //var stat_x = figure.geometry.coord[1];
      //Random rand = new Random();

      //for (double dy = 0; dy < 0.1; dy += 0.01)
      //{
      //  var dx = rand.NextDouble() / 500;
      //  figure.geometry.coord[0] = stat_y + dy;
      //  figure.geometry.coord[1] = stat_x + dx;
      //  figure.zoom_level = "13";
      //  Console.WriteLine(JsonSerializer.Serialize(figure.geometry));
      //  await TestClient.UpdateFiguresAsync(figures, "UpdateTracks");
      //  await Task.Delay(1000);
      //}
    }
  }
}
