namespace Domain
{
  public interface IRequestContextProvider
  {
    string? GetRealm();
    string? GetUserName();
  }
}
