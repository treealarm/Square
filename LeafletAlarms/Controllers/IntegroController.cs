using Dapr.Client;
using Domain.Integro;
using Domain.ServiceInterfaces;
using LeafletAlarms.Grpc;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Mvc;
using ObjectActions;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class IntegroController : ControllerBase
  {
    private readonly IIntegroUpdateService _integroUpdateService;
    private readonly IIntegroService _integroService;
    private readonly IDaprClientService _daprClientService;
    public IntegroController(
      IIntegroUpdateService integroUpdateService,
      IIntegroService integroService,
      IDaprClientService daprClientService
    )
    {
      _integroUpdateService = integroUpdateService;
      _integroService = integroService;
      _daprClientService = daprClientService;
    }

    async Task<Dictionary<string,IntegroDTO>> GetAppIdByObjectId(List<string> object_ids)
    {
      return await _integroService.GetListByIdsAsync(object_ids);
    }

    [HttpGet()]
    [Route("GetAvailableActions")]
    public async Task<List<ActionDescrDTO>> GetAvailableActions(string id)
    {
      var app_ids = await GetAppIdByObjectId(new List<string> { id });

      var daprClient = _daprClientService.GetDaprClient(app_ids.Keys.FirstOrDefault());
      ActionsService.ActionsServiceClient _client = new ActionsService.ActionsServiceClient(daprClient);
      var request = new ProtoGetAvailableActionsRequest();
      request.ObjectId = id;
      var response = await _client.GetAvailableActionsAsync(request);


      var retActions = new List<ActionDescrDTO>();

      foreach (var action in response.ActionsDescr)
      {
        var actionDescr = new ActionDescrDTO()
        {
          name = action.Name,
        };

        foreach (var param in action.Parameters)
        {
          var dto = ProtoToDTOConverter.ConvertToActionParameterDTO(param);

          actionDescr.parameters.Add(dto);
        }
        retActions.Add(actionDescr);
      }
      return retActions;
    }

    [HttpPost()]
    [Route("ExecuteActions")]
    public async Task<bool> ExecuteActions(List<ActionExeDTO> actions)
    {
      var app_ids = await GetAppIdByObjectId(actions.Select(i=> i.object_id).ToList());


      var daprClient = _daprClientService.GetDaprClient("grpctracksclient");
      ActionsService.ActionsServiceClient _client = new ActionsService.ActionsServiceClient(daprClient);
      var request = new ProtoExecuteActionRequest();

      foreach (var action in actions)
      {
        var act_exe = new ProtoActionExe() 
        { 
          Name = action.name, 
          ObjectId = action.object_id
        };

        foreach(var p in action.parameters)
        {
          act_exe.Parameters.Add(ProtoToDTOConverter.ConvertToProtoActionParameter(p));
        }
        request.Actions.Add(act_exe);
      }
      
      var response = await _client.ExecuteActionsAsync(request);

      return response.Success;
    }

    [HttpPost()]
    [Route("GetByIds")]
    public async Task<List<IntegroDTO>> GetByIds(List<string> ids)
    {
      var dic = await _integroService.GetListByIdsAsync(ids);
      return dic.Values.ToList();
    }

    [HttpPost]
    [Route("UpdateIntegros")]
    public async Task<List<IntegroDTO>> UpdateValues([FromBody] List<IntegroDTO> integros)
    {
      await _integroUpdateService.UpdateIntegros(integros);
      return integros;
    }

    [HttpDelete()]
    [Route("DeleteIntegros")]
    public async Task DeleteValues(List<string> ids)
    {
      await _integroUpdateService.RemoveIntegros(ids);
    }
  }
}
