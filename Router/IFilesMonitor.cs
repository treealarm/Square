using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafletAlarmsRouter
{
  public interface IFilesMonitor
  {
    /// <summary>
    /// Starts monitoring.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops monitoring.
    /// </summary>
    void Stop();
  }
}
