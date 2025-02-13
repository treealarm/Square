using Domain;
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
    [Route("GenerateObjectId")]
    public string GenerateObjectId(string input, string version)
    {
      return Utils.GenerateObjectId(input,version);
    }


    [HttpGet()]
    [Route("GetAvailableActions")]
    public async Task<List<ActionDescrDTO>> GetAvailableActions(string id)
    {
      var retActions = new List<ActionDescrDTO>();

      if (string.IsNullOrEmpty(id))
      {
        return retActions;
      }
      var app_ids = await GetAppIdByObjectId(new List<string> { id });
      var key = app_ids.Values.Select(i => i.i_name).FirstOrDefault();

      var daprClient = _daprClientService.GetDaprClient(key);

      if (daprClient == null)
      {
        throw new Exception($"daprClient:{key} not found");
      }

      ActionsService.ActionsServiceClient _client = new ActionsService.ActionsServiceClient(daprClient);
      var request = new ProtoGetAvailableActionsRequest();
      request.ObjectId = id;
      var response = await _client.GetAvailableActionsAsync(request);


      

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

    public static Dictionary<string, List<ActionExeDTO>> GroupActionsByIntegration(
    List<ActionExeDTO> actions,
    Dictionary<string, IntegroDTO> id2integration)
    {
      var groupedActions = new Dictionary<string, List<ActionExeDTO>>();

      foreach (var action in actions)
      {
        // Проверяем, есть ли интеграция для текущего object_id
        if (id2integration.TryGetValue(action.object_id ?? "", out var integration))
        {
          // Если интеграция найдена, добавляем действие в соответствующую группу
          if (!groupedActions.ContainsKey(integration.i_name ?? ""))
          {
            groupedActions[integration.i_name ?? ""] = new List<ActionExeDTO>();
          }

          groupedActions[integration.i_name ?? ""].Add(action);
        }
      }

      return groupedActions;
    }


    [HttpPost()]
    [Route("ExecuteActions")]
    public async Task<bool> ExecuteActions(List<ActionExeDTO> actions)
    {
      var id2integration = await GetAppIdByObjectId(actions.Select(i=> i.object_id).ToList());
      var groupedActions = GroupActionsByIntegration(actions, id2integration);
      bool succ = true;

      foreach (var group in groupedActions)
      {
        var daprClient = _daprClientService.GetDaprClient(group.Key);

        if (daprClient == null)
        {
          throw new Exception($"daprClient:{group.Key} not found");
        }
        ActionsService.ActionsServiceClient _client = new ActionsService.ActionsServiceClient(daprClient);
        var request = new ProtoExecuteActionRequest();

        foreach (var action in group.Value)
        {
          var act_exe = new ProtoActionExe()
          {
            Name = action.name,
            ObjectId = action.object_id
          };

          foreach (var p in action.parameters)
          {
            act_exe.Parameters.Add(ProtoToDTOConverter.ConvertToProtoActionParameter(p));
          }
          request.Actions.Add(act_exe);
        }

        var response = await _client.ExecuteActionsAsync(request);
        succ|= response.Success;
      }      

      return succ;
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
