using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.NonDto;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TracksController : ControllerBase
  {
    private TracksUpdateService _trackUpdateService;
    private readonly ITrackService _tracksService;

    public TracksController(
      TracksUpdateService trackUpdateService,
      ITrackService tracksService
    )
    {
      _trackUpdateService = trackUpdateService;
      _tracksService = tracksService;
    }


    [HttpGet()]
    [Route("GetHello")]
    public  List<string> GetHello()
    {
      return new List<string>() { "Hello world" };
    }

    [HttpPost]
    [Route("AddTracks")]
    public async Task<ActionResult<List<string>>> AddTracks(List<TrackPointDTO> movedMarkers)
    {
      var retVal = await _trackUpdateService.AddTracks(movedMarkers);
      return CreatedAtAction(nameof(AddTracks), retVal);
    }

    [HttpPost]
    [Route("GetTracksByBox")]
    public async Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box)
    {
      return await _trackUpdateService.GetTracksByBox(box);
    }

    [HttpGet]
    [Route("GetTrackById")]
    public async Task<TrackPointDTO> GetTrackById(string id)
    {
      return await _trackUpdateService.GetTrackById(id);
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
  }
}
