using DbLayer;
using Domain;
using Domain.GeoDTO;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MapController : ControllerBase
  {
    private readonly MapService _mapService;

    public MapController(MapService mapsService) =>
        _mapService = mapsService;

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

    [HttpPost]
    [Route("GetByBox")]
    public async Task<FiguresDTO> GetByBox(BoxDTO box)
    {
      var result = new FiguresDTO();

      var geo = await _mapService.GetGeoAsync(box);
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

            foreach(var cur in pt.Coordinates.Exterior.Positions)
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
    public async Task<IActionResult> Post(FiguresDTO newMarkers)
    {
      foreach(var figure in newMarkers.circles)
      {
        await _mapService.CreateCompleteObject(figure);
      }

      foreach (var figure in newMarkers.polygons)
      {
        await _mapService.CreateCompleteObject(figure);
      }

      foreach (var figure in newMarkers.polylines)
      {
        await _mapService.CreateCompleteObject(figure);
      }

      var ret = CreatedAtAction(nameof(Post), newMarkers);
      return ret;
    }

    [HttpPut]
    public async Task<IActionResult> Update(Marker updatedMarker)
    {
      var marker = await _mapService.GetAsync(updatedMarker.id);

      if (marker is null)
      {
        return NotFound();
      }

      await _mapService.UpdateAsync(null, updatedMarker);

      return NoContent();
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