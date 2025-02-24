using System;
using System.Collections.Generic;
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

    public static void CopyAllToJson<T, T1>(this T source, T1 target)
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

    public static T_DTO? ConvertDB2DTO<T_DB, T_DTO>(T_DB dbObj)
       where T_DTO : new()
    {
      if (dbObj == null)
      {
        return default;
      }

      T_DTO result = new T_DTO();
      dbObj.CopyAllTo(result);
      return result;
    }

    public static Dictionary<string, T_DTO> ConvertListDB2DTO<T_DB, T_DTO>(List<T_DB> dbObjs)
      where T_DTO : new()
    {
      var result = new Dictionary<string, T_DTO>();

      foreach (var dbItem in dbObjs)
      {
        var idProperty = typeof(T_DB).GetProperty("id");
        if (idProperty == null)
        {
          throw new InvalidOperationException($"Тип {typeof(T_DB).Name} не содержит свойства 'id'");
        }

        var idValue = idProperty.GetValue(dbItem)?.ToString();
        if (idValue == null)
        {
          throw new InvalidOperationException($"Свойство 'id' в объекте {dbItem} имеет значение null");
        }

        result.Add(idValue, ConvertDB2DTO<T_DB, T_DTO>(dbItem)!);
      }

      return result;
    }

    public static T_DB? ConvertDTO2DB<T_DB,T_DTO>(T_DTO dto)
       where T_DB : new()
    {
      if (dto == null)
      {
        return default;
      }

      var dbo = new T_DB();
     
      dto.CopyAllToJson(dbo);
      return dbo;
    }

    public static T? CloneObject<T>(T source) where T : new()
    {
      if (source == null)
      {
        return default;
      }

      // Создание нового экземпляра типа T
      var clone = new T();

      // Копирование всех свойств из исходного объекта в новый объект
      var properties = typeof(T).GetProperties();
      foreach (var property in properties)
      {
        if (property.CanWrite)
        {
          var value = property.GetValue(source);
          property.SetValue(clone, value);
        }
      }

      return clone;
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

          if (sourceProperty.PropertyType == targetProperty.PropertyType)
          {
            targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
          }              
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
