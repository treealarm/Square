using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Mvc;
using OsmSharp.API;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MapController : ControllerBase
  {
    private readonly IMapService _mapService;
    private readonly IGeoService _geoService;
    private ITrackConsumer _stateService;
    private readonly ITrackService _tracksService;
    public MapController(
      IMapService mapsService,
      IGeoService geoService,
      ITrackConsumer stateService,
      ITrackService tracksService
    )
    {
      _mapService = mapsService;
      _stateService = stateService;
      _geoService = geoService;
      _tracksService = tracksService;
    }        

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<BaseMarkerDTO>> Get(string id)
    {
      var marker = await _mapService.GetAsync(id);

      if (marker is null)
      {
        return NotFound();
      }

      return marker;
    }

    [HttpGet()]
    [Route("GetByParent")]
    public async Task<GetByParentDTO> GetByParent(
      string parent_id,
      string start_id,
      string end_id,
      int count
    )
    {
      GetByParentDTO retVal = new GetByParentDTO();
      retVal.parent_id = parent_id;

      // Fill out parents.
      var parents = await _mapService.GetByChildIdAsync(parent_id);

      retVal.parents = new List<BaseMarkerDTO>();

      foreach (var parent in parents)
      {
        retVal.parents.Insert(0, parent);
      }

      // Get children.
      var markers = await _mapService.GetByParentIdAsync(parent_id, start_id, end_id, count);

      retVal.children = new List<MarkerDTO>();

      List<string> parentIds = new List<string>();

      foreach (var marker in markers)
      {
        var markerDto = DTOConverter.GetMarkerDTO(marker);
        retVal.children.Add(markerDto);
        parentIds.Add(marker.id);
      }

      
      retVal.start_id = retVal.children.FirstOrDefault()?.id;
      retVal.end_id = retVal.children.LastOrDefault()?.id;

      if (markers.Count < count)
      {
        //retVal.end_id = null;
      }

      // Set flag has_children.
      var withChildren = await _mapService.GetTopChildren(parentIds);

      foreach (var item in withChildren)
      {
        var markerDto = retVal.children.Where(x => x.id == item.id).FirstOrDefault();

        if (markerDto != null)
        {
          markerDto.has_children = true;
        }
      }

      return retVal;
    }

    private async Task<List<ObjPropsDTO>> GetPropObjsByFilter(SearchFilterDTO filter)
    {
      List<ObjPropsDTO> retVal = new List<ObjPropsDTO>();

      if (filter.property_filter != null && filter.property_filter.props.Count > 0)
      {
        var props = await _mapService.GetPropByValuesAsync(
          filter.property_filter,
          filter.start_id,
          filter.forward,
          filter.count
        );
        retVal.AddRange(props);
      }

      return retVal;
    }

    [HttpPost]
    [Route("GetByFilter")]
    public async Task<ActionResult<List<GetBySearchDTO>>> GetByFilter(
      SearchFilterDTO filter
    )
    {
      GetBySearchDTO retVal = new GetBySearchDTO();
      retVal.list = new List<BaseMarkerDTO>();
      retVal.search_id = filter.search_id;

      while (retVal.list.Count < filter.count)
      {
        var propsObjs = await GetPropObjsByFilter(filter);

        if (propsObjs.Count == 0)
        {
          break;
        }

        var ids = propsObjs.Select(i => i.id).ToList();

        if (filter.time_start != null || filter.time_end != null)
        {
          // History, so limit by time.
          var tracks = await _tracksService.GetTracksByTime(
            filter.time_start,
            filter.time_end,
            ids
          );

          ids = tracks.Select(t => t.figure.id).ToList();
        }

        var tree = await _mapService.GetAsync(ids);

        retVal.list.AddRange(tree);

        if (!string.IsNullOrEmpty(filter.start_id))
        {
          if (filter.forward)
          {
            filter.start_id = propsObjs.LastOrDefault().id;
          }
          else
          {
            filter.start_id = propsObjs.FirstOrDefault().id;
          }
        }
        else
        {
          break;
        }        
      }

      return CreatedAtAction(nameof(GetByFilter), retVal);
    }

    [HttpGet()]
    [Route("GetAllChildren")]
    public async Task<List<BaseMarkerDTO>> GetAllChildren(string parent_id)
    {
      var markers = await _mapService.GetAllChildren(parent_id);

      var retVal = new List<BaseMarkerDTO>();

      foreach (var marker in markers)
      {
        retVal.Add(marker);
      }

      return retVal;
    }

    private async Task<FiguresDTO> GetFigures(List<GeoObjectDTO> geo)
    {
      var result = new FiguresDTO();

      if (geo == null)
      {
        return result;
      }

      var ids = geo.Select(g => g.id).ToList();
      var tree = await _mapService.GetAsync(ids);

      // For now we use props only for default color.
      var props = await _mapService.GetPropsAsync(ids);

      foreach (var item in tree)
      {
        var geoPart = geo.Where(i => i.id == item.id).FirstOrDefault();

        if (geoPart != null)
        {
          FigureZoomedDTO retItem = null;

          if (geoPart.location is GeometryCircleDTO circle)
          {
            var figure = new FigureCircleDTO();
            figure.radius = geoPart.radius;
            figure.geometry = circle;
            result.circles.Add(figure);
            retItem = figure;
            retItem.type = figure.geometry.GetFigureType();
          }

          if (geoPart.location is GeometryPolygonDTO polygon)
          {
            var figure = new FigurePolygonDTO();

            figure.geometry = polygon;
            result.polygons.Add(figure);
            retItem = figure;
            retItem.type = figure.geometry.GetFigureType();
          }

          if (geoPart.location is GeometryPolylineDTO line)
          {
            var figure = new FigurePolylineDTO();

            figure.geometry = line;
            result.polylines.Add(figure);
            retItem = figure;
            retItem.type = figure.geometry.GetFigureType();
          }

          if (retItem != null)
          {
            retItem.id = item.id;
            retItem.name = item.name;
            retItem.parent_id = item.parent_id;            
            retItem.zoom_level = geoPart.zoom_level?.ToString();
            var objProp = props.Where(p => p.id == retItem.id).First();
            
            if (objProp != null)
            {
              var color = objProp.extra_props.Where(p => p.prop_name == "color").FirstOrDefault();

              if (color != null)
              {
                if (retItem.extra_props == null)
                {
                  retItem.extra_props = new List<ObjExtraPropertyDTO>();
                  retItem.extra_props.Add(color);
                }
              }
            }
          }

        }
      }
      return result;
    }

    [HttpPost]
    [Route("GetByIds")]
    public async Task<ActionResult<FiguresDTO>> GetByIds(List<string> ids)
    {
      var geo = await _geoService.GetGeoObjectsAsync(ids);
      var figures = await GetFigures(geo);

      return CreatedAtAction(nameof(GetByIds), figures);
    }

    [HttpPost]
    [Route("GetByParams")]
    public async Task<ActionResult<FiguresDTO>> GetByParams(ObjPropsSearchDTO propFilter)
    {
      var props = await _mapService.GetPropByValuesAsync(
        propFilter,
        null,
        true,
        1000
      );
      var geo = await _geoService.GetGeoObjectsAsync(props.Select(i => i.id).ToList());
      var figures = await GetFigures(geo);

      return CreatedAtAction(nameof(GetByParams), figures);
    }

    [HttpPost]
    [Route("GetByName")]
    public async Task<ActionResult<List<BaseMarkerDTO>>> GetByName([FromBody] string name)
    {
      var figures = await _mapService.GetByNameAsync(name);
      return CreatedAtAction(nameof(GetByName), figures);
    }    

    [HttpPost]
    [Route("GetByBox")]
    public async Task<FiguresDTO> GetByBox(BoxDTO box)
    {
      var geo = await _geoService.GetGeoAsync(box);
      return await GetFigures(geo);
    }

    [HttpGet()]
    [Route("GetObjProps")]
    public async Task<ActionResult<ObjPropsDTO>> GetObjProps(string id)
    {
      var marker = await _mapService.GetAsync(id);

      if (marker is null)
      {
        return NotFound();
      }

      var markerDto = DTOConverter.GetObjPropsDTO(marker);

      var propDTO = await _mapService.GetPropAsync(id);


      if (propDTO == null)
      {
        propDTO = new ObjPropsDTO();
      }

      var geoPart = await _geoService.GetGeoObjectAsync(id);

      if (geoPart != null)
      {
        markerDto.type = geoPart.location.GetFigureType();

        var geometry = geoPart.location.GetJson();

        propDTO.extra_props.Add(
          new ObjExtraPropertyDTO() { str_val = $"{geometry}", prop_name = "geometry" }
        );

        if (markerDto.type == "Point")
        {          
          propDTO.extra_props.Add(
            new ObjExtraPropertyDTO() { str_val = $"{geoPart.radius}", prop_name = "radius" }
          );
        }

        propDTO.extra_props.Add(
            new ObjExtraPropertyDTO() { str_val = $"{geoPart.zoom_level}", prop_name = "zoom_level" }
          );
      }

      if (propDTO != null && propDTO.extra_props.Count > 0)
      {
        markerDto.extra_props = propDTO.extra_props;
      }

      return markerDto;
    }

    [HttpPost]
    [Route("UpdateOnlyProperties")]
    public async Task<IActionResult> UpdateOnlyProperties(ObjPropsDTO updatedMarker)
    {
      await _mapService.UpdatePropAsync(updatedMarker);
      return CreatedAtAction(nameof(UpdateOnlyProperties), updatedMarker);
    }
    
    [HttpPost]
    [Route("UpdateProperties")]
    public async Task<IActionResult> UpdateProperties(ObjPropsDTO updatedMarker)
    {
      if (string.IsNullOrEmpty(updatedMarker.id))
      {
        await _mapService.CreateOrUpdateHierarchyObject(updatedMarker);
        await _geoService.CreateGeoPoint(updatedMarker);
      }

      var marker = await _mapService.GetAsync(updatedMarker.id);

      if (marker is null)
      {
        return NotFound();
      }

      marker.name = updatedMarker.name;
      marker.parent_id = updatedMarker.parent_id;

      await _mapService.UpdateHierarchyAsync(marker);



      await _mapService.UpdatePropAsync(updatedMarker);

      ObjExtraPropertyDTO radius = null;
      ObjExtraPropertyDTO zoom_level = null;
      ObjExtraPropertyDTO geometry = null;

      if (updatedMarker.extra_props != null)
      {
        radius = updatedMarker.extra_props.Where(p => p.prop_name == "radius").FirstOrDefault();
        zoom_level = updatedMarker.extra_props.Where(p => p.prop_name == "zoom_level").FirstOrDefault();
        geometry = updatedMarker.extra_props.Where(p => p.prop_name == "geometry").FirstOrDefault();
      }

      await _geoService.CreateOrUpdateGeoFromStringAsync(
        updatedMarker.id,
        geometry?.str_val,
        updatedMarker.type,
        radius?.str_val,
        zoom_level?.str_val
      );


      return CreatedAtAction(nameof(UpdateProperties), updatedMarker);
    }

    

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      var marker = await _mapService.GetAsync(id);

      if (marker is null)
      {
        return NotFound();
      }

      var markers = await _mapService.GetAllChildren(id);
      var ids = markers.Select(m => m.id).ToList();
      ids.Add(marker.id);
      await _geoService.RemoveAsync(ids);
      await _mapService.RemoveAsync(ids);
      var ret = CreatedAtAction(nameof(Delete), new { id = marker.id }, ids);

      return ret;
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(List<string> ids)
    {
      HashSet<string> idsToDelete = new HashSet<string>();

      foreach(var id in ids)
      {
        var marker = await _mapService.GetAsync(id);

        if (marker is null)
        {
          continue; ;
        }

        var markers = await _mapService.GetAllChildren(id);
        var bunchIds = markers.Select(m => m.id).ToHashSet();
        idsToDelete.Add(marker.id);
        idsToDelete.UnionWith(bunchIds);
        await _geoService.RemoveAsync(idsToDelete.ToList());
        await _mapService.RemoveAsync(idsToDelete.ToList());
      }
      
      var ret = CreatedAtAction(nameof(Delete), null, idsToDelete);
      return ret;
    }
  }
}