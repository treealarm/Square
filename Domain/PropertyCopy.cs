using System;
using System.Linq;
using System.Text.Json;

namespace Domain
{
  public static class PropertyCopy
  {
    public static void CopyAllToAsJson<T, T1>(this T source, out T1? target)
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
      // Проверка, является ли тип Nullable и если да, получить базовый тип
      var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

      return
          underlyingType.IsPrimitive ||
          underlyingType.IsEnum ||
          underlyingType == typeof(string) ||
          underlyingType == typeof(decimal) ||
          underlyingType == typeof(DateTime) ||
          underlyingType == typeof(bool) ||
          underlyingType == typeof(TimeSpan) ||
          underlyingType == typeof(Guid);
    }

    public static void CopyAllTo<T, T1>(this T source, T1 target)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (target == null)
      {
        throw new ArgumentNullException(nameof(target));
      }

      // Сериализация источника в JSON и десериализация в промежуточный объект
      var json = JsonSerializer.Serialize(source);
      var tempTarget = JsonSerializer.Deserialize<T1>(json);

      if (tempTarget != null)
      {
        foreach (var property in typeof(T1).GetProperties())
        {
          if (property.CanWrite)
          {
            property.SetValue(target, property.GetValue(tempTarget));
          }
        }
      }
    }

    public static void CopyAllTo1<T, T1>(this T source, T1 target)
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
