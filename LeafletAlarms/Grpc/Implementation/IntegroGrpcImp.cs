
using Domain;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.IntegroService;

namespace LeafletAlarms.Grpc.Implementation
{
  internal class IntegroGrpcImp : IntegroServiceBase
  {
    private readonly IIntegroUpdateService _integroUpdateService;
    private readonly IIntegroService _integroService;
    public IntegroGrpcImp(
      IIntegroUpdateService integroUpdateService,
      IIntegroService integroService
    )
    {
      _integroUpdateService = integroUpdateService;
      _integroService = integroService;
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

    public override async Task<BoolValue> UpdateIntegro(IntegroListProto request, ServerCallContext context)
    {
      List<IntegroDTO> dto = new List<IntegroDTO>();

      foreach (var i in request.Objects)
      {
        dto.Add(new IntegroDTO()
        {
          id = i.ObjectId,
          i_name = i.IName,
          i_type = i.IType
        });
      }
      var ret = new BoolValue();
      ret.Value = true;
      await _integroUpdateService.UpdateIntegros(dto);
      return ret;
    }

    public override async Task<IntegroListProto> GetListByType(GetListByTypeRequest request, ServerCallContext context)
    {
      var response = new IntegroListProto();

      var objects = await _integroService.GetListByType(request.IName, request.IType);

      foreach (var i in objects.Values)
      {
        var obj = new IntegroProto()
        {
          IName = i.i_name,
          IType = i.i_type,
          ObjectId = i.id
        };
        response.Objects.Add(obj);       
      }
      return response;
    }
  }
}
