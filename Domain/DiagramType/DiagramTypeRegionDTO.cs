using Domain.Diagram;
using System.Collections.Generic;

namespace Domain.DiagramType
{
    public record DiagramTypeRegionDTO
    {
        public string? id { get; set; }
        public DiagramCoordDTO? geometry { get; set; }
        public Dictionary<string, string>? styles { get; set; } // Дополнительные стили
  }
}
