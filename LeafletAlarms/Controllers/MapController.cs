using DbLayer;
using Domain;
using Domain.GeoDTO;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MapController : ControllerBase
  {
    private readonly MapService _mapService;
    private StateWebSocketHandler _stateService;
    public MapController(MapService mapsService, StateWebSocketHandler stateService)
    {
      _mapService = mapsService;
      _stateService = stateService;
    }        

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Marker>> Get(string id)
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
    public async Task<GetByParentDTO> GetByParent(string parent_id)
    {
      GetByParentDTO retVal = new GetByParentDTO();
      retVal.parent_id = parent_id;

      // Fill out parents.
      var parents = await _mapService.GetByChildIdAsync(parent_id);

      retVal.parents = new List<TreeMarkerDTO>();
      foreach (var parent in parents)
      {
        var markerDto = DTOConverter.GetTreeMarkerDTO(parent);
        retVal.parents.Insert(0, markerDto);
      }

      // Get children.
      var markers = await _mapService.GetByParentIdAsync(parent_id);

      retVal.children = new List<MarkerDTO>();

      List<string> parentIds = new List<string>();

      foreach (var marker in markers)
      {
        var markerDto = DTOConverter.GetMarkerDTO(marker);
        retVal.children.Add(markerDto);
        parentIds.Add(marker.id);
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

    [HttpGet()]
    [Route("GetAllChildren")]
    public async Task<List<TreeMarkerDTO>> GetAllChildren(string parent_id)
    {
      var markers = await _mapService.GetAllChildren(parent_id);

      var retVal = new List<TreeMarkerDTO>();

      foreach (var marker in markers)
      {
        var markerDto = DTOConverter.GetTreeMarkerDTO(marker);
        retVal.Add(markerDto);
      }

      return retVal;
    }

    private async Task<FiguresDTO> GetFigures(List<GeoPoint> geo)
    {
      var result = new FiguresDTO();

      if (geo == null)
      {
        return result;
      }

      var ids = geo.Select(g => g.id).ToList();
      var tree = await _mapService.GetAsync(ids);

      foreach (var item in tree)
      {
        var geoPart = geo.Where(i => i.id == item.id).FirstOrDefault();

        if (geoPart != null)
        {
          FigureBaseDTO retItem = null;

          if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Point)
          {
            var figure = new FigureCircleDTO();
            figure.radius = geoPart.radius;
            var pt = geoPart.location as GeoJsonPoint<GeoJson2DCoordinates>;
            figure.geometry = new double[2] { pt.Coordinates.Y, pt.Coordinates.X };
            result.circles.Add(figure);
            retItem = figure;
          }

          if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Polygon)
          {
            var figure = new FigurePolygonDTO();

            var pt = geoPart.location as GeoJsonPolygon<GeoJson2DCoordinates>;

            List<double[]> list = new List<double[]>();

            foreach (var cur in pt.Coordinates.Exterior.Positions)
            {
              list.Add(new double[2] { cur.Y, cur.X });
            }

            if (list.Count > 3)
            {
              list.RemoveAt(list.Count - 1);
            }

            figure.geometry = list.ToArray();
            result.polygons.Add(figure);
            retItem = figure;
          }

          if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.LineString)
          {
            var figure = new FigurePolylineDTO();

            var pt = geoPart.location as GeoJsonLineString<GeoJson2DCoordinates>;

            List<double[]> list = new List<double[]>();

            foreach (var cur in pt.Coordinates.Positions)
            {
              list.Add(new double[2] { cur.Y, cur.X });
            }
            figure.geometry = list.ToArray();
            result.polylines.Add(figure);
            retItem = figure;
          }

          if (retItem != null)
          {
            retItem.id = item.id;
            retItem.name = item.name;
            retItem.parent_id = item.parent_id;
            retItem.type = geoPart.location.Type.ToString();
          }

        }
      }
      return result;
    }

    [HttpPost]
    [Route("GetByIds")]
    public async Task<FiguresDTO> GetByIds(List<string> ids)
    {
      var geo = await _mapService.GetGeoObjectsAsync(ids);
      return await GetFigures(geo);
    }

    [HttpPost]
    [Route("GetByBox")]
    public async Task<FiguresDTO> GetByBox(BoxDTO box)
    {
      var geo = await _mapService.GetGeoAsync(box);
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

      var props = await _mapService.GetPropAsync(id);

      var propDTO = DTOConverter.Conver2Property2DTO(props);

      var geoPart = await _mapService.GetGeoObjectAsync(id);

      if (geoPart != null)
      {
        markerDto.type = geoPart.location.Type.ToString();
        var figure = DTOConverter.ConvertGeoPoint2DTO(geoPart);
        markerDto.geometry = JsonSerializer.Serialize(figure.geometry);

        if (geoPart.location.Type == GeoJsonObjectType.Point)
        {
          if (propDTO == null)
          {
            propDTO = new ObjPropsDTO();
          }
          propDTO.extra_props.Add(
            new ObjExtraPropertyDTO() { str_val = $"{geoPart.radius}", prop_name = "radius" }
          );
        }
      }

      if (propDTO != null && propDTO.extra_props.Count > 0)
      {
        markerDto.extra_props = propDTO.extra_props;
      }

      return markerDto;
    }

    [HttpPost]
    [Route("UpdateProperties")]
    public async Task<IActionResult> UpdateProperties(ObjPropsDTO updatedMarker)
    {
      if (string.IsNullOrEmpty(updatedMarker.id))
      {
        await _mapService.CreateCompleteObject(updatedMarker);
      }

      var marker = await _mapService.GetAsync(updatedMarker.id);

      if (marker is null)
      {
        return NotFound();
      }

      marker.name = updatedMarker.name;
      marker.parent_id = updatedMarker.parent_id;

      await _mapService.UpdateAsync(marker);

      ObjExtraPropertyDTO radius = null;

      if (updatedMarker.extra_props != null)
      {
        radius = updatedMarker.extra_props.Where(p => p.prop_name == "radius").FirstOrDefault();
      }      

      var props = DTOConverter.ConvertDTO2Property(updatedMarker);

      await _mapService.UpdatePropAsync(props);


      await _mapService.CreateOrUpdateGeoFromStringAsync(
        updatedMarker.id,
        updatedMarker.geometry,
        updatedMarker.type,
        radius?.str_val
      );


      return CreatedAtAction(nameof(UpdateProperties), updatedMarker);
    }

    [HttpPost]
    [Route("AddTracks")]
    public async Task<IActionResult> AddTracks(FiguresDTO movedMarkers)
    {
      List<string> movedIds = new List<string>();

      foreach (var figure in movedMarkers.circles)
      {
        movedIds.Add(figure.id);
        await _mapService.CreateCompleteObject(figure);
      }

      foreach (var figure in movedMarkers.polygons)
      {
        movedIds.Add(figure.id);
        await _mapService.CreateCompleteObject(figure);
      }

      foreach (var figure in movedMarkers.polylines)
      {
        movedIds.Add(figure.id);
        await _mapService.CreateCompleteObject(figure);
      }

      await _stateService.OnUpdatePosition(movedIds);

      var ret = CreatedAtAction(nameof(AddTracks), movedMarkers);
      return ret;
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

        await _mapService.RemoveAsync(idsToDelete.ToList());
      }
      
      var ret = CreatedAtAction(nameof(Delete), null, idsToDelete);
      return ret;
    }
  }
}