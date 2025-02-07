using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IMapUpdateService
  {
    public Task<ObjPropsDTO?> UpdateProperties(ObjPropsDTO updatedMarker);
    public Task<FiguresDTO?> UpdateFigures(FiguresDTO statMarkers);
    public Task<HashSet<string>> Delete(List<string> ids);
  }
}
