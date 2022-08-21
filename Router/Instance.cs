using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Algorithms;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Itinero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itinero.Algorithms.Networks.Analytics.Trees;

namespace LeafletAlarmsRouter
{
  public class Instance : IInstance
  {
    private readonly Router _router;
    private readonly Dictionary<string, int> _defaultSeconds;

    /// <summary>
    /// Creates a new routing instances.
    /// </summary>
    public Instance(Router router, int carTime = 15 * 60,
        int pedestrianTime = 10 * 60, int bicycleTime = 5 * 60)
    {
      _router = router;

      _defaultSeconds = new Dictionary<string, int>();
      _defaultSeconds.Add("car", carTime);
      _defaultSeconds.Add("pedestrian", pedestrianTime);
      _defaultSeconds.Add("bicycle", bicycleTime);
    }

    /// <summary>
    /// Gets the routerdb.
    /// </summary>
    public RouterDb RouterDb
    {
      get
      {
        return _router.Db;
      }
    }

    /// <summary>
    /// Gets meta-data about this instance.
    /// </summary>
    /// <returns></returns>

    /// <summary>
    /// Returns true if the given profile is supported.
    /// </summary>
    public bool Supports(string profile)
    {
      return _router.Db.SupportProfile(profile);
    }

    /// <summary>
    /// Calculates a routing along the given coordinates.
    /// </summary>
    public Result<Route> Calculate(string profileName, Coordinate[] coordinates)
    {
      var profile = _router.Db.GetSupportedProfile(profileName);

      var points = new RouterPoint[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        var result = _router.TryResolve(profile, coordinates[i], 200);
        if (result.IsError)
        {
          result = _router.TryResolve(profile, coordinates[i], 2000);
        }

        points[i] = result.Value;
      }

      if (!_router.Db.HasContractedFor(profile))
      {
        Itinero.Logging.Logger.Log("Instance", Itinero.Logging.TraceEventType.Warning,
            "RouterDb is not optimized for profile {0}, it doesn't contain a contracted graph for this profile.", profileName);
      }

      return _router.TryCalculate(profile, points);
    }

    /// <summary>
    /// Calculates a heatmap.
    /// </summary>
    public Result<HeatmapResult> CalculateHeatmap(string profileName, Coordinate coordinate, int max)
    {
      var profile = _router.Db.GetSupportedProfile(profileName);

      var point = _router.Resolve(profile, coordinate, 200);

      return new Result<HeatmapResult>(_router.CalculateHeatmap(profile, point, max));
    }


    /// <summary>
    /// Calculates a tree.
    /// </summary>
    public Result<Itinero.Algorithms.Networks.Analytics.Trees.Models.Tree> CalculateTree(string profileName, Coordinate coordinate, int max)
    {
      var profile = _router.Db.GetSupportedProfile(profileName);

      lock (_router)
      {
        var point = _router.Resolve(profile, coordinate, 200);

        return new Result<Itinero.Algorithms.Networks.Analytics.Trees.Models.Tree>(
          _router.CalculateTree(profile, point, max));
      }
    }
  }
}
