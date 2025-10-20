using System;

namespace Domain
{
  public class Utils
  {
    public static Guid NewGuid()
    {
      return Guid.NewGuid();
    }
    public static Guid? ConvertObjectIdToGuid(string? objectIdString)
    {
      if (string.IsNullOrEmpty(objectIdString)) return null;

      if (Guid.TryParse(objectIdString, out Guid id)) 
      { 
        return id; 
      }

      return null;
    }
    public static string ConvertGuidToObjectId(Guid? guid)
    {
      if (guid == null)
      {
        return string.Empty;
      }

      return guid.ToString()!;
    }
  }
}

