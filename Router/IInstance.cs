using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Algorithms;
using Itinero.LocalGeo;
using Itinero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafletAlarmsRouter
{
  public interface IInstance
  {
    /// <summary>
    /// Returns true if the given profile is supported.
    /// </summary>
    bool Supports(string profile);

    /// <summary>
    /// Gets the routerdb.
    /// </summary>
    RouterDb RouterDb
    {
      get;
    }

    /// <summary>
    /// Calculates a route along the given coordinates.
    /// </summary>
    Result<Route> Calculate(string profile, Coordinate[] coordinates);

    /// <summary>
    /// Calculates a heatmap.
    /// </summary>
    Result<HeatmapResult> CalculateHeatmap(string profile, Coordinate coordinate, int max);
    /// <summary>
    /// Calculates a tree.
    /// </summary>
    Result<Itinero.Algorithms.Networks.Analytics.Trees.Models.Tree> CalculateTree(string profile, Coordinate coordinate, int max);
  }
}
