using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IMapUpdateService
  {
    public Task<ObjPropsDTO?> UpdateProperties(ObjPropsDTO updatedMarker);
    public Task<FiguresDTO?> UpdateFigures(FiguresDTO statMarkers);
  }
}
