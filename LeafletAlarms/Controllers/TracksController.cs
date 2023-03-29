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

    public TracksController(
      TracksUpdateService trackUpdateService
    )
    {
      _trackUpdateService = trackUpdateService;
    }


    [HttpGet()]
    [Route("GetHello")]
    public  List<string> GetHello()
    {
      return new List<string>() { "Hello world" };
    }

    [HttpPost]
    [Route("AddTracks")]
    public async Task<ActionResult<FiguresDTO>> AddTracks(FiguresDTO movedMarkers)
    {
      var retVal = await _trackUpdateService.AddTracks(movedMarkers);
      return CreatedAtAction(nameof(AddTracks), retVal);
    }

    [HttpPost]
    [Route("UpdateTracks")]
    public async Task<ActionResult<Dictionary<string, TimeSpan>>> UpdateTracks(FiguresDTO movedMarkers)
    {   
      var dic = await _trackUpdateService.UpdateTracks(movedMarkers);
      return CreatedAtAction(nameof(UpdateTracks), dic);
    }


    [HttpPost]
    [Route("GetTracksByBox")]
    public async Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box)
    {
      return await _trackUpdateService.GetTracksByBox(box);
    }
  }
}
