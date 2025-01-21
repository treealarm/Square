using Domain.Integro;
using Domain.ServiceInterfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.IntegroService;

namespace LeafletAlarms.Grpc.Implementation
{
  internal class IntegroGrpcImp : IntegroServiceBase
  {
    private readonly IIntegroUpdateService _integroUpdateService;
    public IntegroGrpcImp(
      IIntegroUpdateService integroUpdateService
    )
    {
      _integroUpdateService = integroUpdateService;
    }

    public override async Task<GenerateObjectIdResponse> GenerateObjectId(GenerateObjectIdRequest request, ServerCallContext context)
    {
      var response = new GenerateObjectIdResponse();

      foreach(var r in request.Input)
      {
        response.Output.Add(new GenerateObjectIdData()
        {
          Input = r.Input,
          Version = r.Version,
          ObjectId = Utils.GenerateObjectId(r.Input, r.Version)
        });
      }
     
      return await Task.FromResult(response);
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public override async Task<BoolValue> UpdateIntegro(UpdateIntegroRequest request, ServerCallContext context)
    {
      List<IntegroDTO> dto = new List<IntegroDTO>();

      foreach (var i in request.Objects)
      {
        dto.Add(new IntegroDTO()
        {
          id = i.ObjectId,
          i_name = i.IName
        });
      }
      var ret = new BoolValue();
      ret.Value = true;
      await _integroUpdateService.UpdateIntegros(dto);
      return ret;
    }
  }
}
