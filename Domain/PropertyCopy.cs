using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Domain
{
  public static class PropertyCopy
  {
    private enum CopyKind
    {
      Direct,
      GuidToString,
      StringToGuid
    }

    private readonly struct CopyStep
    {
      public readonly Func<object, object> Get;
      public readonly Action<object, object> Set;
      public readonly CopyKind Kind;

      public CopyStep(Func<object, object> get, Action<object, object> set, CopyKind kind)
      {
        Get = get;
        Set = set;
        Kind = kind;
      }
    }

    // Кэш плана копирования по паре типов (source, target) — строится один раз через
    // GetProperties()/GetFields() и reflection-сопоставление, дальше копирование идёт
    // через скомпилированные делегаты вместо PropertyInfo.GetValue/SetValue на каждый вызов.
    private static readonly ConcurrentDictionary<(Type, Type), CopyStep[]> _planCache = new();

    private static Func<object, object> CompileGetter(MemberInfo member, Type declaringType, Type memberType)
    {
      var objParam = Expression.Parameter(typeof(object), "obj");
      var typedObj = Expression.Convert(objParam, declaringType);
      Expression access = member is PropertyInfo prop
        ? Expression.Property(typedObj, prop)
        : Expression.Field(typedObj, (FieldInfo)member);

      var boxed = Expression.Convert(access, typeof(object));
      return Expression.Lambda<Func<object, object>>(boxed, objParam).Compile();
    }

    private static Action<object, object> CompileSetter(MemberInfo member, Type declaringType, Type memberType)
    {
      var objParam = Expression.Parameter(typeof(object), "obj");
      var valueParam = Expression.Parameter(typeof(object), "value");
      var typedObj = Expression.Convert(objParam, declaringType);
      var typedValue = Expression.Convert(valueParam, memberType);

      Expression access = member is PropertyInfo prop
        ? Expression.Property(typedObj, prop)
        : Expression.Field(typedObj, (FieldInfo)member);

      var assign = Expression.Assign(access, typedValue);
      return Expression.Lambda<Action<object, object>>(assign, objParam, valueParam).Compile();
    }

    private static CopyStep[] BuildPlan(Type sourceType, Type targetType)
    {
      var steps = new List<CopyStep>();

      Type GetUnderlyingType(Type t) => Nullable.GetUnderlyingType(t) ?? t;

      foreach (var sourceProperty in sourceType.GetProperties())
      {
        if (!IsSimpleType(sourceProperty.PropertyType))
          continue;

        foreach (var targetProperty in targetType.GetProperties())
        {
          if (sourceProperty.Name != targetProperty.Name || targetProperty.SetMethod == null)
            continue;

          var sourceUnderlying = GetUnderlyingType(sourceProperty.PropertyType);
          var targetUnderlying = GetUnderlyingType(targetProperty.PropertyType);

          CopyKind? kind = null;
          if (sourceUnderlying == targetUnderlying)
            kind = CopyKind.Direct;
          else if (sourceUnderlying == typeof(Guid) && targetUnderlying == typeof(string))
            kind = CopyKind.GuidToString;
          else if (sourceUnderlying == typeof(string) && targetUnderlying == typeof(Guid))
            kind = CopyKind.StringToGuid;

          if (kind == null)
            continue;

          steps.Add(new CopyStep(
            CompileGetter(sourceProperty, sourceType, sourceProperty.PropertyType),
            CompileSetter(targetProperty, targetType, targetProperty.PropertyType),
            kind.Value));
        }
      }

      foreach (var sourceField in sourceType.GetFields())
      {
        if (!IsSimpleType(sourceField.FieldType))
          continue;

        foreach (var targetField in targetType.GetFields())
        {
          if (sourceField.Name != targetField.Name || sourceField.FieldType != targetField.FieldType)
            continue;

          steps.Add(new CopyStep(
            CompileGetter(sourceField, sourceType, sourceField.FieldType),
            CompileSetter(targetField, targetType, targetField.FieldType),
            CopyKind.Direct));
        }
      }

      return steps.ToArray();
    }

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

    public static List<T_DB> ConvertListDTO2DB<T_DB, T_DTO>(List<T_DTO> dtoObjs)
     where T_DB : new()
    {
      if (dtoObjs == null)
      {
        return new List<T_DB>(); 
      }

      var result = new List<T_DB>();

      foreach (var dtoItem in dtoObjs)
      {
        var dbItem = ConvertDTO2DB<T_DB, T_DTO>(dtoItem);
        if (dbItem != null)
        {
          result.Add(dbItem);
        }
      }

      return result;
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
      if (source == null || target == null)
        return;

      var plan = _planCache.GetOrAdd((typeof(T), typeof(T1)), key => BuildPlan(key.Item1, key.Item2));

      foreach (var step in plan)
      {
        var value = step.Get(source);

        switch (step.Kind)
        {
          case CopyKind.Direct:
            step.Set(target, value);
            break;

          case CopyKind.GuidToString:
            if (value is Guid guid)
            {
              step.Set(target, Utils.ConvertGuidToObjectId(guid));
            }
            break;

          case CopyKind.StringToGuid:
            if (value is string str && !string.IsNullOrEmpty(str))
            {
              var converted = Utils.ConvertObjectIdToGuid(str);
              if (converted != null)
                step.Set(target, converted.Value);
            }
            break;
        }
      }
    }
  }
}
