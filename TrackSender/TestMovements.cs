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
using System.Reflection;

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

      var arr = new List<Geo2DCoordDTO>(){

      new Geo2DCoordDTO() { 51.495073250207454,-0.09910842912095097},
      new Geo2DCoordDTO() { 51.489442587472304,-0.08674137515201298},
      new Geo2DCoordDTO() { 51.48020988778832,-0.07271508090717972},
      new Geo2DCoordDTO() { 51.47408776490977,-0.04717537252039384},
      new Geo2DCoordDTO() { 51.46459705992192,-0.014692935387021768},
      new Geo2DCoordDTO() { 51.45379220805116,0.023852708531175718},
      new Geo2DCoordDTO() { 51.428481730180884,0.08471876986305917},
      new Geo2DCoordDTO() { 51.39950760193541,0.15812816896559667},
      new Geo2DCoordDTO() { 51.319762897207085,0.28257579952874595},
      new Geo2DCoordDTO() { 51.309835036411144,0.4230368643846383},
      new Geo2DCoordDTO() { 51.32603686286704,0.6587805195935516},
      new Geo2DCoordDTO() { 51.11042711691025,1.1636906018400597},
      new Geo2DCoordDTO() { 51.10407463108633,1.2476501253820251},
      };

      foreach(var coord in arr)
      {
        await Task.Delay(5000);
        figure.geometry.coord = coord;
        await TestClient.UpdateFiguresAsync(figures, "UpdateTracks");
        Console.WriteLine(coord.ToString());
      }
    }
  }
}
