using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class GetTracksBySearchDTO
  {
    public List<TrackPointDTO> list { get; set; } = default!;
    public string search_id { get; set; } = default!;
  }
}
