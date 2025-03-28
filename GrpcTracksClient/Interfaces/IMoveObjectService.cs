using Grpc.Core;
using ObjectActions;

namespace GrpcTracksClient
{
  [Flags]
  public enum E_CarStates
  {
    Free = 0,
    Occupated = 1
  }

  public interface IMoveObjectService
  {
    public static int MaxCars = 50;

    public Task MoveCars(CancellationToken token);
    public Task MovePolygons(CancellationToken token);
  }
}
