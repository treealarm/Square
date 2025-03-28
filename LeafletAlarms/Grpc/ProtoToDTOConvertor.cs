﻿using Domain;
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
        ProtoActionValue.ValueOneofCase.DoubleValue => ("double", (object)value.DoubleValue),
        ProtoActionValue.ValueOneofCase.IntValue => ("int", (object)value.IntValue),
        ProtoActionValue.ValueOneofCase.StringValue => ("string", (object)value.StringValue),
        ProtoActionValue.ValueOneofCase.Coordinates => ("coordinates", GRPCServiceProxy.CoordsFromProto2DTO(value.Coordinates)),
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
        "double" when double.TryParse(cur_val?.ToString(), out var d) =>
            new ProtoActionValue { DoubleValue = d },

        "int" when int.TryParse(cur_val?.ToString(), out var i) =>
            new ProtoActionValue { IntValue = i },

        "string" =>
            new ProtoActionValue { StringValue = cur_val?.ToString() },

        "coordinates" when cur_val is JsonElement element && element.ValueKind == JsonValueKind.Object =>
            new ProtoActionValue { Coordinates = GRPCServiceProxy.ConvertGeoDTO2Proto(element.Deserialize<GeometryDTO>()) },

        "coordinates" when cur_val is GeometryDTO coords =>
            new ProtoActionValue { Coordinates = GRPCServiceProxy.ConvertGeoDTO2Proto(coords) },

        _ => new ProtoActionValue() // Пустое значение для неизвестного типа
      };


      return protoActionParameter;
    }

  }

}
