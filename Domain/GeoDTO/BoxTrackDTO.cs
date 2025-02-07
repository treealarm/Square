using System;


namespace Domain
{
  public class BoxTrackDTO: BoxDTO
  {
    public DateTime? time_start { get; set; }
    public DateTime? time_end { get; set; }
    public int sort { get; set; } = 0;       
  }
}
