using Domain;
using Domain.GeoDBDTO;
using LeafletAlarms.KMeans;
using Reminiscence.Collections;
using System;

namespace LeafletAlarms.Controllers
{
  public class KMeans
  {
    static private Random _rnd = new Random();
    public static FiguresDTO GetCentroids(FiguresDTO figures, int number)
    {
      FiguresDTO retVal = new FiguresDTO();

      var figs = figures.figs;
      KMCentroid trashCentroid = new KMCentroid();

      List<KMCentroid> centroids = GetInitialCentroids(figures, number);

      foreach(var f in figs)
      {
        var circle = f.geometry.GetCentroid();

        if (circle ==  null)
        {
          trashCentroid.AddFigure(f);
        }

        var minDist = double.MaxValue;
        KMCentroid centroidFound = null;

        foreach (KMCentroid centroid in centroids)
        {
          var dist = centroid.GetDistanse(f);

          if (dist < minDist)
          {
            minDist = dist;
            centroidFound = centroid;
          }
        }

        if (centroidFound != null)
        {
          centroidFound.AddFigure(f);
        }        
      }

      foreach (KMCentroid centroid in centroids)
      {
        centroid.UpdateCenter();
      }

      foreach (KMCentroid centroid in centroids)
      {
        if (centroid.GetCount() == 0)
        {
          continue;
        }

        var g = new GeometryCircleDTO();
        g.coord = centroid.center;

        retVal.figs.Add(new FigureGeoDTO()
        {
          id = centroid.GetId(),
          geometry = g,
          radius = 200
        });
       }

      return retVal;
    }

    static private List<KMCentroid> GetInitialCentroids(FiguresDTO figures, int number)
    {
      var figs = figures.figs;

      List<KMCentroid> centroids = new List<KMCentroid>();

      for (int i = 0; i < number; i++)
      {
        int index = _rnd.Next(0, figs.Count);
        var centroid = figs[index].geometry.GetCentroid();

        if (centroid == null)
        {
          continue;
        }

        KMCentroid c = new KMCentroid();
        c.center.X = centroid.X;
        c.center.Y = centroid.Y;
        centroids.Add(c);
      }

      return centroids;
    }
  }
}
