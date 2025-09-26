
using System.Collections.Generic;

namespace Domain
{
    public record DiagramTypeRegionDTO
    {
        public string? region_key { get; set; }
        public DiagramCoordDTO? geometry { get; set; }
        public Dictionary<string, string>? styles { get; set; } // Дополнительные стили
  }
}
