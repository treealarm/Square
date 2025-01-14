
using System.Collections.Generic;

namespace Domain.Integro
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
  }
}
