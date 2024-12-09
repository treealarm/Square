using Domain.Diagram;

namespace Domain.DiagramType
{
    public record DiagramTypeRegionDTO
    {
        public string? id { get; set; }
        public DiagramCoordDTO? geometry { get; set; }
    }
}
