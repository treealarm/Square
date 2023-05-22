using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Itinero.Route;

namespace LeafletAlarmsRouter
{
  public static class InstanceManager
  {
    /// <summary>
    /// Holds the routing service instances.
    /// </summary>
    private static readonly Dictionary<string, Instance> _items =
        new Dictionary<string, Instance>();


    /// <summary>
    /// Returns true if the given instance is active.
    /// </summary>
    public static bool IsActive(string name)
    {
      return _items.ContainsKey(name);
    }

    /// <summary>
    /// Returns true if there is at least one instance.
    /// </summary>
    public static bool HasInstances => _items.Count > 0;

    /// <summary>
    /// Returns the routing module instance with the given name.
    /// </summary>
    public static bool TryGet(string name, out Instance instance)
    {
      if (_items.TryGetValue(name, out instance))
      { return true; }

      if (_items.Count == 0)
      {
        return false;
      }

      var keyVal = _items.FirstOrDefault();

      instance = keyVal.Value;


      return true;
    }

    /// <summary>
    /// Registers a new instance.
    /// </summary>
    public static void Register(string name, Instance instance)
    {
      _items[name] = instance;
    }
  }
}
