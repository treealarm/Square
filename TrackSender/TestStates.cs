using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.States;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrackSender.Models;

namespace TrackSender
{
  internal class TestStates
  {

   
    TestClient _testClient = new TestClient();
    FiguresDTO m_figures = new FiguresDTO();
    
    private Random _random = new Random();
    private string _main_id = null;
    public async Task RunAsync(CancellationToken token, List<Task> tasks)
    {
      await BuildMoscow();

      tasks.Add(EmulateState(token));      
    }

    private void AddStateObjects(Root geoObj, FigurePolygonDTO parentPolygon)
    {
      Random random = new Random();

      if (geoObj.centroid.type == "Point")
      {
        Console.WriteLine($"added state obj:{geoObj.names.name}");
        var start =
            new GeometryCircleDTO(
              new Geo2DCoordDTO() {
                  geoObj.centroid.coordinates[1],
                  geoObj.centroid.coordinates[0] }
              );

        var figure = new FigureCircleDTO()
        {
          name = geoObj.names.name,
          radius = random.Next(150, 300),
          zoom_level = "12",
          geometry = start
        };
        figure.parent_id = parentPolygon.id;

        if (string.IsNullOrEmpty(figure.id))
        {
          figure.id = Program.GenerateBsonId();
        }

        figure.extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "moscow_state",
                str_val = "true"
              }
            };

        m_figures.circles.Add(figure);
      }
    }

    private async Task<FiguresDTO> CreateOrGetDistrict(int osmid)
    {
      Console.WriteLine(osmid);

      var color = 
        $"#{_random.Next(20).ToString("X2")}{_random.Next(256).ToString("X2")}{_random.Next(256).ToString("X2")}";

      FiguresDTO figures = await _testClient.GetByParams("osmid", osmid.ToString());
      
      if (figures != null && !figures.IsEmpty())
      {
        foreach (var figure in figures.polygons)
        {
          figure.extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "osmid",
                str_val = osmid.ToString()
              },
              new ObjExtraPropertyDTO()
              {
                prop_name = "color",
                str_val = color
              }
            };
        }
        return figures;        
      }

      figures = new FiguresDTO();
      figures.circles = new List<FigureCircleDTO>();
      figures.polygons = new List<FigurePolygonDTO>();

      var geoObj = await NominatimProcessor.GetOsmFigureFromDisk(osmid, "PolygonJson");

      if (geoObj == null)
      {
        return null;
      }

      var me = MoscowOsm.osmids.Where(o => o[0] == osmid).FirstOrDefault();
      var parent = me[1];


      var parentPolygon = m_figures.polygons
        .Where(p => p.extra_props
          .Any(e => e.prop_name == "osmid" && e.str_val == parent.ToString())   
        ).FirstOrDefault();

      {
        if (geoObj.geometry.type == "MultiPolygon" ||
          geoObj.geometry.type == "Polygon")
        {
          MultiPolygon coords;

          if (geoObj.geometry.type == "Polygon")
          {
            var polygon =
              JsonSerializer.Deserialize<Polygon>(geoObj.geometry.coordinates.ToString());
            coords = new MultiPolygon()
            {
              polygon
            };
          }
          else
          {
            coords = 
              JsonSerializer.Deserialize<MultiPolygon>(geoObj.geometry.coordinates.ToString());
          }

          foreach (var coord in coords)
          {
            var nameOfpolygon = $"{geoObj.names.name} {coords.IndexOf(coord)}";

            if (coords.Count == 1)
            {
              nameOfpolygon = $"{geoObj.names.name}";
            }

            var start =
              new GeometryPolygonDTO();

            foreach (var c in coord[0])
            {
              //Suffle this shit.
              var temp = c.X;
              c.X = c.Y;
              c.Y = temp;
            }

            start.coord = coord[0];

            var figure = new FigurePolygonDTO()
            {
              name = nameOfpolygon,
              zoom_level = "11",
              geometry = start
            };

            figure.extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "osmid",
                str_val = osmid.ToString()
              },
              new ObjExtraPropertyDTO()
              {
                prop_name = "color",
                str_val = color
              }
            };

            if (string.IsNullOrEmpty(figure.id))
            {
              figure.id = Program.GenerateBsonId();
            }

            if (parentPolygon != null)
            {
              figure.parent_id = parentPolygon.id;

              if (parent == MoscowOsm.osmids.First()[0])
              {
                figure.zoom_level = "10";                
              }
              else
              {
                AddStateObjects(geoObj, figure);
              }
            }
            else
            {
              figure.zoom_level = "9";
              figure.parent_id = _main_id;
            }        

            figures.polygons.Add(figure);
          }
        }       
        else
        {
          // Undefined.
        }
      }

      return figures;
    }
    public async Task BuildMoscow()
    {
      try
      {
        var parents = await _testClient.GetByName("Russia");

        if (parents == null || parents.Count == 0)
        {
          BaseMarkerDTO marker = new BaseMarkerDTO()
          {
            name = "Russia"
          };
          marker = await _testClient.UpdateBase(marker);
          _main_id = marker.id;
        }
        else
        {
          _main_id = parents.FirstOrDefault().id;
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      

      foreach (var osmid in MoscowOsm.osmids)
      {
        var figure = await CreateOrGetDistrict(osmid[0]);

        if (figure != null && !figure.IsEmpty())
        {
          m_figures?.polylines.AddRange(figure?.polylines);
          m_figures?.circles.AddRange(figure?.circles);
          m_figures?.polygons.AddRange(figure?.polygons);
        }
        else
        {
          figure = await CreateOrGetDistrict(osmid[0]);
          // Empty figure?
        }
      }      

      var updated_figures = await _testClient.UpdateFiguresAsync(m_figures);
    }

   
    private async Task EmulateState(CancellationToken token)
    {
      var figures = await _testClient.GetByParams("moscow_state", "true");
      

      while (!token.IsCancellationRequested)
      {
        List<ObjectStateDTO> states = new List<ObjectStateDTO>();
        var random = new Random();

        string[] stateDescrs = new string[]
        {
        "ALARM",
        "INFO",
        "NORM"
        };

        foreach (var figure in figures.circles)
        {
          int stateNum = random.Next(1, 3);
          bool isAlarm = random.Next(0, 21) == 5;

          if (isAlarm)
          {
            stateNum = 0;
          }

          ObjectStateDTO state = new ObjectStateDTO()
          {
            id = figure.id,
            states = new List<string>
          {
            stateDescrs[stateNum]
          }
          };
          states.Add(state);
        }

        await _testClient.UpdateStates(states);
      }
      
    }
  }
}
