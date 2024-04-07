using System;
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

    private static bool IsSimpleType(Type type)
    {
      return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
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
          if (!IsSimpleType(sourceProperty.PropertyType))
          {
            continue;
          }  
          targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
        }
      }

      foreach (var sourceField in type.GetFields())
      {
        foreach (var targetField in type1.GetFields()
          .Where(targetField => sourceField.Name == targetField.Name))
        {
          if (!IsSimpleType(sourceField.FieldType))
          {
            continue;
          }
          targetField.SetValue(target, sourceField.GetValue(source));
        }
      }
    }
  }
}
