using LeafletAlarmsGrpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDaprClientLib
{
  internal interface IMove
  {    
    public Task<ProtoFigures?> Move(ProtoFigures figs);
  }
}
