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
        ProtoActionValue.ValueOneofCase.IpRange => (VisualTypes.IpRange, new IpRangeDTO() 
        { 
          start_ip = value.IpRange.StartIp,
          end_ip = value.IpRange.EndIp
        }),
        ProtoActionValue.ValueOneofCase.CredentialList => (VisualTypes.CredentialList, new CredentialListDTO()
        {
          values = value.CredentialList.Values
            .Select(c => new CredentialDTO
            {
              username = c.Username,
              password = c.Password
            })
            .ToList()
        }),
        ProtoActionValue.ValueOneofCase.EnumList => (VisualTypes.EnumList, value.EnumList.Values.ToList()),
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
      var protoActionParameter = new ProtoActionParameter
      {
        Name = dto.name,
        CurVal = new ProtoActionValue()
      };

      if (dto.cur_val == null)
      {
        return protoActionParameter;
      }

      object cur_val;

      if (dto.cur_val is JsonElement jsonElement)
      {
        cur_val = jsonElement.ValueKind switch
        {
          JsonValueKind.String => jsonElement.GetString(),
          JsonValueKind.Number when jsonElement.TryGetDouble(out var d) => d,
          JsonValueKind.Number when jsonElement.TryGetInt32(out var i) => i,
          JsonValueKind.Object => jsonElement,
          JsonValueKind.Array => jsonElement, // Вдруг список пар
          _ => jsonElement.ToString()
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

        VisualTypes.Coordinates when cur_val is JsonElement elem && elem.ValueKind == JsonValueKind.Object =>
            new ProtoActionValue { Coordinates = GRPCServiceProxy.ConvertGeoDTO2Proto(elem.Deserialize<GeometryDTO>()) },

        VisualTypes.Coordinates when cur_val is GeometryDTO coords =>
            new ProtoActionValue { Coordinates = GRPCServiceProxy.ConvertGeoDTO2Proto(coords) },

        VisualTypes.IpRange when cur_val is JsonElement elem && elem.ValueKind == JsonValueKind.Object =>
            new ProtoActionValue
            {
              IpRange = JsonSerializer.Deserialize<IpRangeDTO>(elem.GetRawText()) is { } ipRangeDto
                    ? new ProtoIpRange
                    {
                      StartIp = ipRangeDto.start_ip,
                      EndIp = ipRangeDto.end_ip
                    }
                    : null
            },

        VisualTypes.IpRange when cur_val is IpRangeDTO ipRange =>
            new ProtoActionValue
            {
              IpRange = new ProtoIpRange
              {
                StartIp = ipRange.start_ip,
                EndIp = ipRange.end_ip
              }
            },

        VisualTypes.CredentialList when cur_val is JsonElement elem && elem.ValueKind == JsonValueKind.Object =>
            new ProtoActionValue
            {
              CredentialList = new ProtoCredentialList
              {
                Values = {
                elem.TryGetProperty("values", out var credsElement) && credsElement.ValueKind == JsonValueKind.Array
                    ? credsElement.Deserialize<List<CredentialDTO>>()?.Select(c => new ProtoCredential
                    {
                        Username = c.username,
                        Password = c.password
                    }) ?? Enumerable.Empty<ProtoCredential>()
                    : Enumerable.Empty<ProtoCredential>()
                    }
              }
            },


        VisualTypes.CredentialList when cur_val is CredentialListDTO credList =>
            new ProtoActionValue
            {
              CredentialList = new ProtoCredentialList
              {
                Values = 
                {
                  credList.values.Select(c => new ProtoCredential
                  {
                      Username = c.username,
                      Password = c.password
                  })
                }
              }
            },

        VisualTypes.EnumList when cur_val is JsonElement elem && elem.ValueKind == JsonValueKind.Array =>
        new ProtoActionValue
        {
          EnumList = new ProtoEnumList()
          { 
            Values = { elem.EnumerateArray()
              .Select(e => e.GetString())
              .Where(s => s != null).ToList() } 
          }          
        },

        VisualTypes.EnumList when cur_val is IEnumerable<string> list =>
            new ProtoActionValue
            {
              EnumList = new ProtoEnumList()
              {
                Values = { list }
              }
            },

        _ => new ProtoActionValue()
      };

      return protoActionParameter;
    }
  }
}
