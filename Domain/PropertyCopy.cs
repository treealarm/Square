using System.Linq;
using System.Text.Json;

namespace Domain
{
  public static class PropertyCopy
  {
    public static void CopyAllToAsJson<T, T1>(this T source, out T1 target)
    {
      var json = JsonSerializer.Serialize(source);

      target = JsonSerializer.Deserialize<T1>(json);
    }
    public static T1 CopyAll<T, T1>(this T source) where T1 : new()
    {
      var target = new T1();

      source.CopyAllTo(target);

      return target;
    }

    public static void CopyAllTo<T, T1>(this T source, T1 target)
    {
      var type = typeof(T);
      var type1 = typeof(T1);

      foreach (var sourceProperty in type.GetProperties())
      {
        foreach (var targetProperty in type1.GetProperties()
          .Where(targetProperty => sourceProperty.Name == targetProperty.Name)
          .Where(targetProperty => targetProperty.SetMethod != null))
        {
          targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
        }
      }

      foreach (var sourceField in type.GetFields())
      {
        foreach (var targetField in type1.GetFields()
          .Where(targetField => sourceField.Name == targetField.Name))
        {
          targetField.SetValue(target, sourceField.GetValue(source));
        }
      }
    }
  }
}
