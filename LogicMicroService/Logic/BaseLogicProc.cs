using Domain;
using Domain.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogicMicroService
{
  public class BaseLogicProc
  {
    public StaticLogicDTO LogicDTO { get; set; }
    public string LogicId 
    {
      get { return LogicDTO.id; }
    }
    public HashSet<string> TextObjectsIds { get; set; }

    public BaseLogicProc(StaticLogicDTO logicDto)
    {
      LogicDTO = logicDto;

      TextObjectsIds = logicDto.figs
          .Where(f => f.group_id == "text")
          .Select(f => f.id)
          .ToHashSet();
    }

    public virtual async Task InitFromDb(IGeoService geoService)
    {
      await Task.Delay(0);
    }

    public virtual async Task<bool> ProcessTracks(
      ITrackService tracksService,
      DateTime? time_start,
      DateTime? time_end
      )
    {
      await Task.Delay(0);
      return false;
    }
    
    public virtual string GetUpdatedResult()
    {
      return string.Empty;
    }
  }
}
