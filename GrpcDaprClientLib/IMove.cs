using LeafletAlarmsGrpc;

namespace GrpcDaprClientLib
{
  internal interface IMove
  {    
    public Task<ProtoFigures?> Move(ProtoFigures figs);
  }
}
