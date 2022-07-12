using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public class StateBaseDTO
  {  
    public string action { get; set; }
    public object data { get; set; }
  }
}
