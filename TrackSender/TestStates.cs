﻿using Domain;
using Domain.GeoDBDTO;
using Domain.States;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using TrackSender.Models;

namespace TrackSender
{
  internal class TestStates
  {   
    TestClient _testClient;
    FiguresDTO m_figures = new FiguresDTO();
    
    private Random _random = new Random();
    public TestStates(HttpClient client) 
    { 
      _testClient = new TestClient(client);
    }
    public void RunAsync(CancellationToken token, List<Task> tasks)
    {
      tasks.Add(EmulateState(token));      
    }

    private void AddStateObjectLowLevel(
      FigureGeoDTO parentCircle,
      int kCounter
    )
    {
      var start = parentCircle.geometry as GeometryCircleDTO;

      start = new GeometryCircleDTO()
      {
        coord = new Geo2DCoordDTO()
        {
          X = start.coord.X + _random.Next(-300, 300) * 0.00005,
          Y = start.coord.Y + _random.Next(-300, 300) * 0.00005
        }
      };

      var figure = new FigureGeoDTO()
      {
        name = parentCircle.name + kCounter.ToString(),
        radius = _random.Next(50, 100),
        zoom_level = "14-17",
        geometry = start
      };

      figure.parent_id = parentCircle.id;

      figure.id = Program.GenerateBsonId();

      Console.WriteLine($"mem create lowlevel obj:{figure.name}");


      figure.extra_props = new List<ObjExtraPropertyDTO>()
          {
            new ObjExtraPropertyDTO()
            {
              prop_name = "moscow_state",
              str_val = "true"
            }
          };

      m_figures.figs.Add(figure);
    }

    private void AddStateObjects(Root geoObj, FigureGeoDTO parentPolygon)
    {
      Random random = new Random();

      if (geoObj.centroid.type == "Point")
      {        
        var start =
            new GeometryCircleDTO(
              new Geo2DCoordDTO() {
                  geoObj.centroid.coordinates[1],
                  geoObj.centroid.coordinates[0] }
              );

        var figure = new FigureGeoDTO()
        {
          name = geoObj.names.name,
          radius = random.Next(100, 200),
          zoom_level = "12",
          geometry = start
        };
        
        figure.parent_id = parentPolygon.id;

        figure.id = Program.GenerateBsonId();

        Console.WriteLine($"mem create state obj:{figure.name}");

        figure.extra_props = new List<ObjExtraPropertyDTO>()
        {
          new ObjExtraPropertyDTO()
          {
            prop_name = "moscow_state",
            str_val = "true"
          }
        };

        for (int k = 0; k < 1000; k++)
        {
          AddStateObjectLowLevel(figure, k);
        }
        m_figures.figs.Add(figure);
      }
    }

    private async Task<FiguresDTO> CreateOrGetDistrict(int osmid, string parent_id)
    {
      Console.WriteLine(osmid);

      var color = 
        $"#{_random.Next(20).ToString("X2")}{_random.Next(256).ToString("X2")}{_random.Next(256).ToString("X2")}";


      var figures = new FiguresDTO();
      figures.figs = new List<FigureGeoDTO>();

      var geoObj = await NominatimProcessor.GetOsmFigureFromDisk(osmid, "PolygonJson");

      if (geoObj == null)
      {
        return null;
      }

      var me = MoscowOsm.osmids.Where(o => o[0] == osmid).FirstOrDefault();
      var parent = me[1];


      var parentPolygon = m_figures.figs
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

            var figure = new FigureGeoDTO()
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
                prop_name = "__color",
                str_val = color
              }
            };

            figure.id = Program.GenerateBsonId();

            Console.WriteLine($"mem create district obj:{figure.name}");

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
              figure.parent_id = parent_id;
            }        

            figures.figs.Add(figure);
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
      const string RussiaName = "Russia";
      string RussiaId = "6426a9f30032fcaa5eabcd88";

      try
      {
        var parents = await _testClient.GetByName(RussiaName);

        if (parents == null || parents.Count == 0)
        {
          BaseMarkerDTO marker = new BaseMarkerDTO()
          {
            name = RussiaName,
            id = RussiaId
          };
          marker = await _testClient.UpdateBase(marker);
          RussiaId = marker.id;
        }
        else
        {
          RussiaId = parents.FirstOrDefault().id;
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      var resourceName = $"TrackSender.StatesBD.states.json";
      var s = await NominatimProcessor.GetResource(resourceName);

      FiguresDTO json = null;

      try
      {
        json = JsonSerializer.Deserialize<FiguresDTO>(s);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      if (json == null)
      {
        foreach (var osmid in MoscowOsm.osmids)
        {
          var figure = await CreateOrGetDistrict(osmid[0], RussiaId);

          if (figure != null && !figure.IsEmpty())
          {
            m_figures?.figs.AddRange(figure?.figs);
          }
        }

        var sToSave = JsonSerializer.Serialize(
          m_figures,
          new JsonSerializerOptions()
          {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull
          }
        );

        try
        {
          File.WriteAllText(@"D:\TESTS\Leaflet\leaflet_data\states.json", sToSave.ToString());
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }
      else
      {
        m_figures = json;
      }

      try
      {
        for (int f = 0; f < m_figures.figs.Count; f += 10000)
        {
          FiguresDTO ff = new FiguresDTO()
          {
            figs = m_figures.figs.Skip(f).Take(10000).ToList()
          };

          var updated_figures = await _testClient.UpdateFiguresAsync(ff);
        }
      }
      catch(Exception e)
      {
        Console.WriteLine(e.Message);
      }    
    }

   
    private async Task EmulateState(CancellationToken token)
    {
      string start_id = string.Empty;

      var total_count = 0;
      var start_time = DateTime.Now;

      while (!token.IsCancellationRequested)
      {
        var figures = await _testClient.GetByParams("moscow_state", "true", start_id, 10000);

        if (figures == null)
        {
          Console.WriteLine("GetByParams returned null");
          await Task.Delay(1000);
          continue;
        }

        var count = figures.figs.Count;
        total_count += count;

        if (count == 0)
        {          
          start_id = string.Empty;

          var end_time = DateTime.Now;
          var delay1 = (end_time - start_time).TotalSeconds;
          Console.WriteLine($"");
          Console.WriteLine($"UpdateStates Speed: {total_count / delay1}");
          Console.WriteLine($"");

          continue;
        }

        

        start_id = figures.figs.Last().id;

        List<ObjectStateDTO> states = new List<ObjectStateDTO>();

        string[] stateDescrs = new string[]
        {
        "ALARM",
        "INFO",
        "NORM"
        };

        List<int> iAlarm = new List<int>();

        bool bAlarm = _random.Next(1, 20) == 5;

        if (bAlarm)
        {
          for (int j = 0; j < 2; j++)
          {
            iAlarm.Add(_random.Next(0, figures.figs.Count()));
          }
        }

        foreach (var figure in figures.figs)
        {
          int stateNum = _random.Next(1, 3);
          

          if (iAlarm.Contains(figures.figs.IndexOf(figure)))
          {
            Console.WriteLine($"ALARMED {figure.name}");
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

        var t1 = DateTime.Now;
        await _testClient.UpdateStates(states);
        var t2 = DateTime.Now;
        var delay = (t2 - t1).TotalSeconds;
        Console.WriteLine($"UpdateStates {count}: ->{delay}<-");
        await Task.Delay(1000);
      }
      
    }
  }
}
