
using Common;
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
    private readonly IIntegroTypeUpdateService _integroTypeUpdateService;
    public IntegroGrpcImp(
      IIntegroUpdateService integroUpdateService,
      IIntegroService integroService,
      IIntegroTypeUpdateService integroTypeUpdateService
    )
    {
      _integroUpdateService = integroUpdateService;
      _integroService = integroService;
      _integroTypeUpdateService = integroTypeUpdateService;
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

    private IntegroListProto ConvertDTO2Proto(Dictionary<string, IntegroDTO> objects)
    {
      var response = new IntegroListProto();
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
    public override async Task<IntegroListProto> GetListByType(GetListByTypeRequest request, ServerCallContext context)
    {
      var objects = await _integroService.GetListByType(request.IName, request.IType);
      return ConvertDTO2Proto(objects);
    }

    public override async Task<BoolValue> UpdateIntegroTypes(IntegroTypesProto request, ServerCallContext context)
    {
      var ret = new BoolValue();
      ret.Value = true;

      var types_dto = new List<IntegroTypeDTO>();
      foreach(var type in request.Types_)
      {
        var children = new List<IntegroTypeChildDTO>();
        foreach (var c in type.Children)
        {
          children.Add(new IntegroTypeChildDTO()
          { child_i_type = c.ChildIType });
        }
        types_dto.Add(new IntegroTypeDTO()
        {
          i_type = type.IType,
          i_name = type.IName,
          children = children
        });
      }
      await _integroTypeUpdateService.UpdateTypesAsync(types_dto);
      return ret;
    }
    public override async Task<IntegroListProto> GetListByIds(ProtoObjectIds request, ServerCallContext context)
    {
      var objects =  await _integroService.GetListByIdsAsync(request.Ids.ToList());
      return ConvertDTO2Proto(objects);
    }
  }
}
