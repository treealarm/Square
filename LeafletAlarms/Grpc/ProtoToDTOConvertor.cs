using Domain.GeoDBDTO;
using Domain.Integro;
using ObjectActions;
using System.Text.Json;

namespace LeafletAlarms.Grpc
{
  public static class ProtoToDTOConverter
  {
    public static ActionParameterDTO ConvertToActionParameterDTO(ProtoActionParameter param)
    {
      var value = param.CurVal;

      // Используем один switch для определения и типа, и значения
      var (parameterType, convertedValue) = value.ValueCase switch
      {
        ProtoActionValue.ValueOneofCase.DoubleValue => ("double", (object)value.DoubleValue),
        ProtoActionValue.ValueOneofCase.IntValue => ("int", (object)value.IntValue),
        ProtoActionValue.ValueOneofCase.StringValue => ("string", (object)value.StringValue),
        ProtoActionValue.ValueOneofCase.Coordinates => ("coordinates", ConvertCoordinates(value.Coordinates)),
        _ => ("unknown", null) // Если значение не задано
      };

      // Создание и возврат DTO
      return new ActionParameterDTO
      {
        name = param.Name,
        type = parameterType,
        cur_val = convertedValue
      };
    }

    private static List<Geo2DCoordDTO> ConvertCoordinates(ProtoCoordinates coordinates)
    {
      return coordinates.Coordinates
          .Select(coord => new Geo2DCoordDTO
          {
            Lat = coord.Lat,
            Lon = coord.Lon
          })
          .ToList();
    }

    public static ProtoActionParameter ConvertToProtoActionParameter(ActionParameterDTO dto)
    {
      // Создание ProtoActionParameter
      var protoActionParameter = new ProtoActionParameter
      {
        Name = dto.name
      };

      // Конвертация типа и значения
      if (dto.type != null && dto.cur_val != null)
      {
        protoActionParameter.CurVal = dto.type switch
        {
          "double" => new ProtoActionValue { DoubleValue = Convert.ToDouble(dto.cur_val) },
          "int" => new ProtoActionValue { IntValue = Convert.ToInt32(dto.cur_val) },
          "string" => new ProtoActionValue { StringValue = dto.cur_val.ToString() },
          "coordinates" => new ProtoActionValue { Coordinates = ConvertToProtoCoordinates(dto.cur_val as List<Geo2DCoordDTO>) },
          _ => new ProtoActionValue() // Пустое значение для неизвестного типа
        };
      }
      else
        if (dto.cur_val != null)
      {
        if (dto.cur_val is JsonElement jsonElement)
        {
          // Проверяем тип с использованием JsonElement
          switch (jsonElement.ValueKind)
          {
            case JsonValueKind.Number:
              if (jsonElement.TryGetInt32(out var intValue))
              {
                // Если значение можно представить как int, устанавливаем его в IntValue
                protoActionParameter.CurVal = new ProtoActionValue { IntValue = intValue };
              }
              else if (jsonElement.TryGetInt64(out var longValue))
              {
                // Если значение можно представить как long (например, большие целые числа), устанавливаем его в IntValue
                protoActionParameter.CurVal = new ProtoActionValue { IntValue = (int)longValue }; // Можно также оставить long, если нужно
              }
              else if (jsonElement.TryGetDouble(out var doubleValue))
              {
                // Если это число с плавающей запятой, устанавливаем в DoubleValue
                protoActionParameter.CurVal = new ProtoActionValue { DoubleValue = doubleValue };
              }
              break;


            case JsonValueKind.String:
              protoActionParameter.CurVal = new ProtoActionValue { StringValue = jsonElement.GetString() };
              break;

            case JsonValueKind.Array:
              var geoCoords = JsonSerializer.Deserialize<List<Geo2DCoordDTO>>(jsonElement.GetRawText());
              protoActionParameter.CurVal = new ProtoActionValue { Coordinates = ConvertToProtoCoordinates(geoCoords) };
              break;

            default:
              protoActionParameter.CurVal = new ProtoActionValue(); // Пустое значение для неизвестного типа
              break;
          }
        }
        else
        {
          // На случай, если dto.cur_val не является JsonElement, можем попробовать другие типы
          if (dto.cur_val is double doubleVal)
          {
            protoActionParameter.CurVal = new ProtoActionValue { DoubleValue = doubleVal };
          }
          else if (dto.cur_val is int intVal)
          {
            protoActionParameter.CurVal = new ProtoActionValue { IntValue = intVal };
          }
          else if (dto.cur_val is string strVal)
          {
            protoActionParameter.CurVal = new ProtoActionValue { StringValue = strVal };
          }
          else if (dto.cur_val is List<Geo2DCoordDTO> coordinates)
          {
            protoActionParameter.CurVal = new ProtoActionValue { Coordinates = ConvertToProtoCoordinates(coordinates) };
          }
          else
          {
            protoActionParameter.CurVal = new ProtoActionValue(); // Пустое значение для неизвестного типа
          }
        }
      }

      return protoActionParameter;
    }

    private static ProtoCoordinates ConvertToProtoCoordinates(List<Geo2DCoordDTO> coords)
    {
      var protoCoordinates = new ProtoCoordinates();
      if (coords != null)
      {
        foreach (var coord in coords)
        {
          protoCoordinates.Coordinates.Add(new ProtoCoordinate
          {
            Lat = coord.Lat,
            Lon = coord.Lon
          });
        }
      }
      return protoCoordinates;
    }

  }

}
