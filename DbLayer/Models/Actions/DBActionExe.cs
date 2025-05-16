using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DbLayer.Models.Actions
{
  internal record DBActionParameter : BasePgEntity
  {
    public Guid action_execution_id { get; set; }
    public string name { get; set; } // Имя параметра
    public string type { get; set; } // Тип параметра
    public JsonElement cur_val { get; set; } // Текущее значение
  }

  internal record DBActionExe : BasePgEntity
  {
    public Guid object_id { get; set; }
    public string name { get; set; }
    public List<DBActionParameter> parameters { get; set; }
    public DateTime timestamp { get; set; }
  }

  internal record DBActionExeResult : BasePgEntity
  {
    public int progress { get; set; }
    public JsonElement result { get; set; }
  }
}
