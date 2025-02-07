
using System.Collections.Generic;

using System.Threading.Tasks;

namespace Domain
{
  public interface ILevelService
  {
    Task<List<string>> GetLevelsByZoom(double? zoom);
    Task<LevelDTO> GetByZoomLevel(string name);
    Task Init();
    Task<Dictionary<string, LevelDTO>> GetAllZooms();
  }
}
