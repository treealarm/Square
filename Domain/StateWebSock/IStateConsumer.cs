using Domain.States;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public interface IStateConsumer
  {
    Task OnStateChanged(List<ObjectStateDTO> state);
  }
}
