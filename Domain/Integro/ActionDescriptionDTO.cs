
using System.Collections.Generic;

namespace Domain
{
  public class ActionParameterDTO
  {
    public string? name { get; set; } // Имя параметра
    public string? type { get; set; } // Тип параметра
    public object? cur_val { get; set; } // Текущее значение
  }

  public record ActionDescrDTO : BaseObjectDTO
  {
    public string? name { get; set; }
    public List<ActionParameterDTO> parameters { get; set; } = new List<ActionParameterDTO>();
    public bool is_long_action { get; set; }  
  }

  public record ActionExeDTO
  {
    public string? object_id { get; set; } // id объекта
    public string? name { get; set; } // Имя Действия
    public List<ActionParameterDTO>? parameters { get; set; }
    public string? uid { get; set; }
  }

  public record ActionExeResultDTO
  {
    public string? uid { get; set; }
    public int? progress { get; set; }
    public object? result { get; set; }
  }

  public record ActionExeResponseDTO
  {
    public string? uid { get; set; }
    public string? object_id { get; set; } // id объекта
    public string? name { get; set; } // Имя Действия
  }
}
