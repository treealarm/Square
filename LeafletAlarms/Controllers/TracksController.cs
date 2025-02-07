using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public class TracksController : ControllerBase
  {
    private ITracksUpdateService _trackUpdateService;
    private readonly ITrackService _tracksService;

    public TracksController(
      ITracksUpdateService trackUpdateService,
      ITrackService tracksService
    )
    {
      _trackUpdateService = trackUpdateService;
      _tracksService = tracksService;
    }

    [HttpPost]
    [Route("GetTracksByBox")]
    public async Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box)
    {
      return await _tracksService.GetTracksByBox(box);
    }

    [HttpGet]
    [Route("GetTrackById")]
    public async Task<TrackPointDTO> GetTrackById(string id)
    {
      return await _tracksService.GetByIdAsync(id);
    }

    [HttpPost]
    [Route("GetByFilter")]
    public async Task<ActionResult<List<GetTracksBySearchDTO>>> GetByFilter(
      SearchFilterDTO filter
    )
    {
      GetTracksBySearchDTO retVal = new GetTracksBySearchDTO();
      retVal.search_id = filter.search_id;
      retVal.list = await _tracksService.GetTracksByFilter(filter);        

      return CreatedAtAction(nameof(GetByFilter), retVal);
    }

    [HttpPost]
    [Route("AddTracks")]
    public async Task<ActionResult<List<string>>> AddTracks(List<TrackPointDTO> movedMarkers)
    {
      var retVal = await _trackUpdateService.AddTracks(movedMarkers);
      return CreatedAtAction(nameof(AddTracks), retVal);
    }
  }
}
