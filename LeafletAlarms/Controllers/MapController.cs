using Domain;
using LeafletAlarms.Authentication;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace LeafletAlarms.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public class MapController : ControllerBase
  {
    private readonly IMapService _mapService;
    private readonly IGeoService _geoService;
    private readonly ITrackService _tracksService;
    private readonly ILevelService _levelService;
    private readonly RightsCheckerService _rightChecker;
    private readonly IOptions<RoutingSettings> _routingSettings;
    private readonly IMapUpdateService _mapUpdateService;
    private readonly IIntegroUpdateService _integroUpdateService;
    public MapController(
      IMapService mapsService,
      IGeoService geoService,
      ITrackService tracksService,
      ILevelService levelService,
      RightsCheckerService rightChecker,
      IOptions<RoutingSettings> routingSettings,
      IMapUpdateService mapUpdateService,
      IIntegroUpdateService integroUpdateService
    )
    {
      _mapService = mapsService;
      _geoService = geoService;
      _tracksService = tracksService;
      _levelService = levelService;
      _rightChecker = rightChecker;
      _routingSettings = routingSettings;
      _mapUpdateService = mapUpdateService;
      _integroUpdateService = integroUpdateService;
    }        

    [HttpGet()]
    [Route("GetById")]
    public async Task<ActionResult<BaseMarkerDTO>> GetById(string id)
    {
      var marker = await _mapService.GetAsync(id);

      if (marker is null)
      {
        return NotFound();
      }

      return marker;
    }

    [AllowAnonymous]
    [HttpGet("GetTiles/{layer}/{z}/{x}/{y}.png")]
    public async Task<FileResult> GetTiles(string layer, string z, string x, string y)
    {
      byte[] data = null;
      var tile_server = new Uri($"https://tile.openstreetmap.org/{z}/{x}/{y}.png");

      var dataDirectory = new DirectoryInfo(_routingSettings.Value.RootFolder);

      string path = AppDomain.CurrentDomain.BaseDirectory.ToString();

      if (dataDirectory.Exists)
      {
        path = dataDirectory.FullName;        
      }

      path = Path.Combine(path, "map_cash");

      try
      {
        Directory.CreateDirectory(path);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      string localName = Path.Combine(path, tile_server.LocalPath.TrimStart('/'));

      if (System.IO.File.Exists(localName))
      {
        try
        {
          data = await System.IO.File.ReadAllBytesAsync(localName);
          return File(data, "image/png");
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }      

      try
      {
        using (HttpClient httpClient = new HttpClient())
        {
          var productValue = new ProductInfoHeaderValue("Mozilla", "1.0");
          var commentValue = new ProductInfoHeaderValue("(+http://www.leftfront.ru)");

          httpClient.DefaultRequestHeaders.Add(
              HeaderNames.UserAgent, productValue.ToString());
          httpClient.DefaultRequestHeaders.Add(
              HeaderNames.UserAgent, commentValue.ToString());

          data = await httpClient.GetByteArrayAsync(tile_server);
        }

        try
        {
          Directory.CreateDirectory(Path.GetDirectoryName(localName));
          System.IO.File.WriteAllBytes(localName, data);
        }
        catch (Exception) // Couldn't save the file
        {
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      if (data == null)
      {
        return null;
      }
      return File(data, "image/png");
    }

    [HttpGet()]
    [Route("GetByParent")]
    public async Task<GetByParentDTO> GetByParent(
      [FromQuery] string parent_id = null,
      [FromQuery] string start_id = null,
      [FromQuery] string end_id = null,
      [FromQuery] int count = 0
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

      string minId = null;
      string maxId = null;

      foreach (var marker in markers.Values)
      {
        var markerDto = DTOConverter.GetMarkerDTO(marker);
        retVal.children.Add(markerDto);
        parentIds.Add(marker.id);

        if (minId == null || string.Compare(marker.id, minId) < 0)
        {
          minId = marker.id;
        }
        if (maxId == null || string.Compare(marker.id, maxId) > 0)
        {
          maxId = marker.id;
        }
      }

      retVal.start_id = minId;
      retVal.end_id = maxId;

      //retVal.start_id = retVal.children.FirstOrDefault()?.id;
      //retVal.end_id = retVal.children.LastOrDefault()?.id;

      if (markers.Count < count)
      {
        //retVal.end_id = null;
      }

      // Set flag has_children.
      var withChildren = await _mapService.GetTopChildren(parentIds);

      foreach (var item in withChildren)
      {
        var markerDto = retVal.children.Where(x => x.id == item.Key).FirstOrDefault();

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

      var props = await _mapService.GetPropByValuesAsync(
        filter.property_filter,
        filter.start_id,
        filter.forward,
        filter.count
      );
      retVal.AddRange(props);

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

        // History, so limit by time.
        var tracks = await _tracksService.GetFirstTracksByTime(
          filter.time_start,
          filter.time_end,
          ids
        );

        ids = tracks.Select(t => t.figure.id).ToList();


        var tree = await _mapService.GetAsync(ids);

        retVal.list.AddRange(tree.Values);

        if (!string.IsNullOrEmpty(filter.start_id))
        {
          if (filter.forward > 0)
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

    [HttpPost]
    [Route("GetByIds")]
    public async Task<ActionResult<FiguresDTO>> GetByIds(List<string> ids)
    {
      var geo = await _geoService.GetGeoObjectsAsync(ids.Where(e=> !string.IsNullOrEmpty(e)).ToList());
      var figures = await _mapService.GetFigures(geo);

      return CreatedAtAction(nameof(GetByIds), figures);
    }

    [HttpGet]
    [Route("GetGeoObject")]
    public async Task<ActionResult<GeoObjectDTO>> GetGeoObject(string id)
    {
      var geo = await _geoService.GetGeoObjectAsync(id);
      return CreatedAtAction(nameof(GetGeoObject), geo);
    }

    [HttpPost]
    [Route("GetByParams")]
    public async Task<ActionResult<FiguresDTO>> GetByParams(SearchFilterDTO propFilter)
    {
      var props = await _mapService.GetPropByValuesAsync(
        propFilter.property_filter,
        propFilter.start_id,
        propFilter.forward,
        propFilter.count
      );
      var geo = await _geoService.GetGeoObjectsAsync(props.Select(i => i.id).ToList());
      var figures = await _mapService.GetFigures(geo);

      return CreatedAtAction(nameof(GetByParams), figures);
    }

    [HttpPost]
    [Route("GetByName")]
    public async Task<ActionResult<List<BaseMarkerDTO>>> GetByName([FromBody] string name)
    {
      var figures = await _mapService.GetByNameAsync(name);
      return CreatedAtAction(nameof(GetByName), figures.Values);
    }

    private async Task AddIdsByProperties(BoxDTO box)
    {
      // This methods means filter by properties if exists.
      List<string> ids = null;

      if (box.property_filter != null && box.property_filter.props.Count > 0)
      {
        var props = await _mapService.GetPropByValuesAsync(
          box.property_filter,
          null,
          1,
          1000
        );
        ids = props.Select(i => i.id).ToList();

        if (box.ids == null)
        {
          box.ids = new List<string>();
        }
        box.ids.AddRange(ids);
      }
    }

    [HttpPost]
    [Route("GetByBox")]
    public async Task<FiguresDTO> GetByBox(BoxDTO box)
    {
      await AddIdsByProperties(box);

      var geo = await _geoService.GetGeoAsync(box);

      if (!User.IsInRole(RoleConstants.admin))
      {        
        var toView = await _rightChecker.CheckForView(geo.Keys.ToList());
        geo = geo.Where(kvp => toView.Contains(kvp.Key))
          .ToDictionary(ent => ent.Key, ent => ent.Value);
      }            

      var figures =  await _mapService.GetFigures(geo);

      if (figures.figs.Count < 1000)
      {
        return figures;
      }
      var zooms = await _levelService.GetAllZooms();
      figures.figs.Sort(new ZoomComparer(zooms));

      var figsRaw = figures.figs.Take(500);
      var figsRest = figures.figs.Skip(500).ToList();
      var nCentroids = Math.Max(100, 1 + figsRest.Count / 1000);
      var retval =  KMeans.GetCentroids(figsRest, nCentroids);
      retval.figs.AddRange(figsRaw);
      return retval;
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

      if (geoPart != null && geoPart.location != null)
      {
        var type = geoPart.location.GetFigureType();

        var zoomLevel = await _levelService.GetByZoomLevel(geoPart.zoom_level);

        if (zoomLevel != null)
        {
          markerDto.zoom_min = zoomLevel.zoom_min;
          markerDto.zoom_max = zoomLevel.zoom_max;
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
    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConstants.admin + "," + RoleConstants.power_user)]
    public async Task<ActionResult<ObjPropsDTO>> UpdateProperties(ObjPropsDTO updatedMarker)
    {
      await _mapUpdateService.UpdateProperties(updatedMarker);
      await _integroUpdateService.OnUpdatedNormalObjects(
        new List<string>() { updatedMarker.id }, 
        Topics.OnUpdateIntegros);

      return CreatedAtAction(nameof(UpdateProperties), updatedMarker);
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConstants.admin + "," + RoleConstants.power_user)]
    [HttpDelete]
    public async Task<IActionResult> Delete(List<string> ids)
    {
      HashSet<string> idsToDelete = await _mapUpdateService.Delete(ids);
      var ret = CreatedAtAction(nameof(Delete), null, idsToDelete);
      return ret;
    }

    [HttpPost]
    [Route("UpdateFigures")]
    public async Task<ActionResult<FiguresDTO>> UpdateFigures(FiguresDTO statMarkers)
    {
      await _mapUpdateService.UpdateFigures(statMarkers);

      return CreatedAtAction(nameof(UpdateFigures), statMarkers);
    }

    [AllowAnonymous]
    [HttpGet()]
    [Route("GetZoomLevels")]
    public async Task<ActionResult<List<LevelDTO>>> GetZoomLevels()
    {
      var zooms = await _levelService.GetAllZooms();
      var zoomLevels = zooms.Values.OrderBy(v => v.zoom_min).ThenBy(v=>v.zoom_max);
      return CreatedAtAction(nameof(GetZoomLevels), zoomLevels.ToList());
    }
   }
}