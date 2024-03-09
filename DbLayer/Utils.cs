namespace DbLayer
{
  public class Utils
  {
    static public string GenerateBsonId()
    {
      return MongoDB.Bson.ObjectId.GenerateNewId().ToString();
    }
  }
}
