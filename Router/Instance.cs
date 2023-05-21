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
using Itinero.Attributes;
using System.Diagnostics.Metrics;
using System.Security.AccessControl;
using Itinero.Algorithms.Search;
using Itinero.Data.Network;

namespace LeafletAlarmsRouter
{
  public class Instance : IInstance
  {
    private readonly Router _router;
    private uint _speed0;
    /// <summary>
    /// Creates a new routing instances.
    /// </summary>
    public Instance(Router router, int carTime = 15 * 60,
        int pedestrianTime = 10 * 60, int bicycleTime = 5 * 60)
    {
      _router = router;
      _speed0 = _router.Db.EdgeProfiles.Add(new AttributeCollection(
                new Itinero.Attributes.Attribute("access", "no"),
                new Itinero.Attributes.Attribute("highway", "footway")
                ));
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

    public void RemoveEdges(string profileName, HashSet<uint> toRemove)
    {
      var profile = _router.Db.GetSupportedProfile(profileName);

      foreach (var edgeId in toRemove)
      {
        var edgeInst = _router.Db.Network.GetEdge(edgeId);

        if (edgeInst != null)
        {
          var edgeData = edgeInst.Data;
          var attrColl = _router.Db.EdgeProfiles.Get(edgeData.Profile);
          var attrColl1 = _router.Db.EdgeProfiles.Get(_speed0);
          //attrColl.AddOrReplace("max_speed", "0");
          //_router.Db.Network.RemoveEdge(edgeId);
          // update the speed profile of this edge.

          edgeData.Profile = (ushort)_speed0;
          _router.Db.Network.UpdateEdgeData(edgeId, edgeData);
        }
      }
      
      //var attrColl = _router.Db.EdgeProfiles.Get(edgeData.Profile);
      //foreach (var attr in attrColl)
      //{
      //  Console.WriteLine($"{attr.Key}={attr.Value}");
      //}
    }
    /// <summary>
    /// Calculates a routing along the given coordinates.
    /// </summary>
    public Route Calculate(
      string profileName,
      List<Coordinate> coordinates
    )
    {
      var profile = _router.Db.GetSupportedProfile(profileName);

       var points = new List<RouterPoint>();

      foreach (var coordinate in coordinates)
      {
        var result = _router.TryResolve(profile, coordinate, 200);

        if (result.IsError)
        {
          result = _router.TryResolve(profile, coordinate, 2000);
        }
        else
        {
          
        }

        if (result.IsError)
        {
        }
        else
        {
          points.Add(result.Value);
        }
      }

      if (!_router.Db.HasContractedFor(profile))
      {
        Itinero.Logging.Logger.Log("Instance", Itinero.Logging.TraceEventType.Warning,
            "RouterDb is not optimized for profile {0}, it doesn't contain a contracted graph for this profile.", profileName);
      }

      if (points.Count < 2)
      {
        return null;
      }
        
      return _router.Calculate(profile, points.ToArray());
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
    public Result<Itinero.Algorithms.Networks.Analytics.Trees.Models.Tree> CalculateTree(
      string profileName,
      Coordinate coordinate,
      int max)
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
