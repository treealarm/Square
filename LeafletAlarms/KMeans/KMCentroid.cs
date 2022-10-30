using Domain;
using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeafletAlarms.KMeans
{
  public class KMCentroid
  {
    private List<FigureGeoDTO> figs { get; set; } = new List<FigureGeoDTO>();
    public Geo2DCoordDTO center { get; set; } = new Geo2DCoordDTO();
    public double radius { get; set; } = 0;
    public void AddFigure(FigureGeoDTO fig)
    {
      figs.Add(fig);
    }
    public double GetDistanse(FigureGeoDTO fig)
    {
      var c = fig.geometry.GetCentroid();
      var dx = c.X - center.X;
      var dy = c.Y - center.Y;
      return Math.Sqrt(dx * dx + dy * dy);
    }

    public void UpdateCenterAndRadius()
    {
      double x = 0;
      double y = 0;
      radius = 0;

      foreach (var fig in figs)
      {
        var c = fig.geometry.GetCentroid();
        x += c.X;
        y += c.Y;

        if (fig.radius != 0)
        {
          radius += (double)fig.radius;
        }        
      }

      center.X = x/ figs.Count;
      center.Y = y/ figs.Count;
      radius = radius / figs.Count;
    }

    public int GetCount()
    {
      return figs.Count;
    }

    public string GetId()
    {
      return figs.FirstOrDefault().id;
    }
  }
}
