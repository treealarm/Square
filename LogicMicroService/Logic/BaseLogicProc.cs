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
    public HashSet<string> PropFilterObjIds { get; set; }

    public BaseLogicProc(StaticLogicDTO logicDto)
    {
      LogicDTO = logicDto;

      TextObjectsIds = logicDto.figs
          .Where(f => f.group_id == "gr_text")
          .Select(f => f.id)
          .ToHashSet();
    }

    public virtual async Task InitFromDb(IGeoService geoService, IMapService mapService)
    {
      await SetIdsByProperties(LogicDTO.property_filter, mapService);
    }

    protected virtual async Task SetIdsByProperties(
      ObjPropsSearchDTO property_filter,
      IMapService mapService
    )
    {
      // This methods means filter by properties if exists.
      PropFilterObjIds = null;

      if (property_filter != null && property_filter.props.Count > 0)
      {
        var props = await mapService.GetPropByValuesAsync(
          property_filter,
          null,
          1,
          1000
        );

        PropFilterObjIds = props.Select(i => i.id).ToHashSet();
      }
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
