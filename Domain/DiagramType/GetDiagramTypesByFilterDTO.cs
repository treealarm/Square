namespace Domain.DiagramType
{
  public record GetDiagramTypesByFilterDTO
  (string? filter, string? start_id, bool forward, int count);
}
