
namespace Domain
{
  public interface IIdentifiable
  {
    string? id { get; set; }
  }
  public abstract record BaseObjectDTO : IIdentifiable
  {    public string? id { get; set; }
  }
}
