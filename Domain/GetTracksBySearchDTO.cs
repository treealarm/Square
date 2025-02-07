using System.Collections.Generic;

namespace Domain
{
  public class GetTracksBySearchDTO
  {
    public List<TrackPointDTO>? list { get; set; }
    public string? search_id { get; set; }
  }
}
