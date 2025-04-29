using Domain;
using LeafletAlarms.Grpc.Implementation;
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
        ProtoActionValue.ValueOneofCase.DoubleValue => (VisualTypes.Double, (object)value.DoubleValue),
        ProtoActionValue.ValueOneofCase.IntValue => (VisualTypes.Int, (object)value.IntValue),
        ProtoActionValue.ValueOneofCase.StringValue => (VisualTypes.String, (object)value.StringValue),
        ProtoActionValue.ValueOneofCase.Coordinates => (VisualTypes.Coordinates, GRPCServiceProxy.CoordsFromProto2DTO(value.Coordinates)),
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


    public static ProtoActionParameter ConvertToProtoActionParameter(ActionParameterDTO dto)
    {
      // Создание ProtoActionParameter
      var protoActionParameter = new ProtoActionParameter
      {
        Name = dto.name,
        CurVal = new ProtoActionValue()
        {
          
        }
      };

      if (dto.cur_val == null)
      {
        return protoActionParameter;
      }

      object cur_val;

      if (dto.cur_val is JsonElement jsonElement)
      {
        // Преобразуем JsonElement в строку для дальнейшей обработки
        cur_val = jsonElement.ValueKind switch
        {
          JsonValueKind.String => jsonElement.GetString(),
          JsonValueKind.Number when jsonElement.TryGetDouble(out var d) => d,
          JsonValueKind.Number when jsonElement.TryGetInt32(out var i) => i,
          JsonValueKind.Object => jsonElement, // Оставляем для обработки как object
          _ => jsonElement.ToString() // Если это что-то другое, конвертируем в строку
        };
      }
      else
      {
        cur_val = dto.cur_val;
      }

      protoActionParameter.CurVal = dto.type switch
      {
        VisualTypes.Double when double.TryParse(cur_val?.ToString(), out var d) =>
            new ProtoActionValue { DoubleValue = d },

        VisualTypes.Int when int.TryParse(cur_val?.ToString(), out var i) =>
            new ProtoActionValue { IntValue = i },

        VisualTypes.String =>
            new ProtoActionValue { StringValue = cur_val?.ToString() },

        VisualTypes.Coordinates when cur_val is JsonElement element && element.ValueKind == JsonValueKind.Object =>
            new ProtoActionValue { Coordinates = GRPCServiceProxy.ConvertGeoDTO2Proto(element.Deserialize<GeometryDTO>()) },

        VisualTypes.Coordinates when cur_val is GeometryDTO coords =>
            new ProtoActionValue { Coordinates = GRPCServiceProxy.ConvertGeoDTO2Proto(coords) },

        _ => new ProtoActionValue() // Пустое значение для неизвестного типа
      };


      return protoActionParameter;
    }

  }

}
