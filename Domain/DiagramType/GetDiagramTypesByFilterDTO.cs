namespace Domain
{
  public record GetDiagramTypesByFilterDTO
  (string? filter, string? start_id, bool forward, int count);
}
