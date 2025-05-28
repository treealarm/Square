
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
  }

  public record ActionExeDTO
  {
    public string? object_id { get; set; } // id объекта
    public string? name { get; set; } // Имя Действия
    public List<ActionParameterDTO>? parameters { get; set; }
    public string? action_execution_id { get; set; }
  }

  public record ActionExeResultDTO
  {
    public string? action_execution_id { get; set; }
    public int? progress { get; set; }
    public object? result { get; set; }
  }

  public record ActionExeInfoDTO
  {
    public string? object_id { get; set; } // id объекта
    public string? name { get; set; } // Имя Действия
    public ActionExeResultDTO? result { get; set; }
  }

  public record ActionExeInfoRequestDTO
  {
    public string? object_id { get; set; } // id объекта
    public int max_progress { get; set; } // Имя Действия
  }
}
