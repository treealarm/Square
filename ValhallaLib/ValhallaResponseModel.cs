using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValhallaLib
{
  // RootResponse myDeserializedClass = JsonConvert.DeserializeObject<RootResponse>(myJsonResponse);
  public class Leg
  {
    public List<Maneuver>? maneuvers { get; set; }
    public Summary? summary { get; set; }
    public string? shape { get; set; }
  }

  public class LocationResponse
  {
    public string? type { get; set; }
    public double? lat { get; set; }
    public double? lon { get; set; }
    public string? side_of_street { get; set; }
    public int? original_index { get; set; }
  }

  public class Maneuver
  {
    public int? type { get; set; }
    public string? instruction { get; set; }
    public string? verbal_succinct_transition_instruction { get; set; }
    public string? verbal_pre_transition_instruction { get; set; }
    public string? verbal_post_transition_instruction { get; set; }
    public double? time { get; set; }
    public double? length { get; set; }
    public double? cost { get; set; }
    public int? begin_shape_index { get; set; }
    public int? end_shape_index { get; set; }
    public bool? verbal_multi_cue { get; set; }
    public string? travel_mode { get; set; }
    public string? travel_type { get; set; }
    public string? verbal_transition_alert_instruction { get; set; }
    public List<string>? street_names { get; set; }
  }

  public class RootResponse
  {
    public Trip? trip { get; set; }
    public string? id { get; set; }
  }

  public class Summary
  {
    public bool? has_time_restrictions { get; set; }
    public bool? has_toll { get; set; }
    public bool? has_highway { get; set; }
    public bool? has_ferry { get; set; }
    public double? min_lat { get; set; }
    public double? min_lon { get; set; }
    public double? max_lat { get; set; }
    public double? max_lon { get; set; }
    public double? time { get; set; }
    public double? length { get; set; }
    public double? cost { get; set; }
  }

  public class Trip
  {
    public List<LocationResponse>? locations { get; set; }
    public List<Leg>? legs { get; set; }
    public Summary? summary { get; set; }
    public string? status_message { get; set; }
    public int? status { get; set; }
    public string? units { get; set; }
    public string? language { get; set; }
  }


}
