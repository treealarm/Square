using Domain;
using Domain.Integro;
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
    
    private readonly IIntegroTypeUpdateService _integroTypeUpdateService;
    private readonly IIntegroTypesService _integroTypesService;
    private readonly IMapUpdateService _mapUpdateService;
    private readonly IActionsUpdateService _actionsUpdateService;
    private readonly IActionsService _actionsService;

    private readonly IDaprClientService _daprClientService;
    public IntegroController(
      IIntegroUpdateService integroUpdateService,
      IIntegroService integroService,
      IIntegroTypeUpdateService integroTypeUpdateService,
      IIntegroTypesService integroTypesService,
      IMapUpdateService mapUpdateService,
      IDaprClientService daprClientService,
      IActionsUpdateService actionsUpdateService,
      IActionsService actionsService
    )
    {
      _integroUpdateService = integroUpdateService;
      _integroTypeUpdateService = integroTypeUpdateService;
      _integroService = integroService;
      _integroTypesService = integroTypesService;
      _mapUpdateService = mapUpdateService;
      _actionsUpdateService = actionsUpdateService;
      _actionsService = actionsService;
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

    [HttpPost()]
    [Route("CancelAction")]
    public async Task CancelAction(string uid)
    {
      var actions = await _actionsService.GetActionsByActionIds(new List<string> { uid });
      var app_ids = await GetAppIdByObjectId(actions.Select(a=>a.object_id).ToList());
      var key = app_ids.Values.Select(i => i.i_name).FirstOrDefault();

      var daprClient = _daprClientService.GetDaprClient(key);

      if (daprClient == null)
      {
        throw new Exception($"daprClient:{key} not found");
      }
      ActionsService.ActionsServiceClient _client = new ActionsService.ActionsServiceClient(daprClient);
      var ids_to_cancel = new ProtoEnumList();
      ids_to_cancel.Values.Add(uid);
      await _client.CancelActionsAsync(ids_to_cancel);

      var action = new ActionExeResultDTO();
      action.action_execution_id = uid;
      action.progress = 100;
      action.result = "Cancelled";
      await _actionsUpdateService.UpdateResultsAsync(new List<ActionExeResultDTO>() { action });
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
      try
      {
        var app_ids = await GetAppIdByObjectId(new List<string> { id });

        if (!app_ids.Any())
        {
          return retActions;
        }
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
            is_long_action = action.IsLongAction,
          };

          foreach (var param in action.Parameters)
          {
            var dto = ProtoToDTOConverter.ConvertToActionParameterDTO(param);

            actionDescr.parameters.Add(dto);
          }
          retActions.Add(actionDescr);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
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
    public async Task<List<ActionExeInfoDTO>> ExecuteActions(List<ActionExeDTO> actions)
    {
      var res = new List<ActionExeInfoDTO>();

      await _actionsUpdateService.UpdateListAsync(actions);

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
            ObjectId = action.object_id,
            Uid = action.action_execution_id,
          };

          res.Add(new ActionExeInfoDTO()
          {
            name = action.name,
            object_id = action.object_id
          });

          foreach (var p in action.parameters)
          {
            act_exe.Parameters.Add(ProtoToDTOConverter.ConvertToProtoActionParameter(p));
          }
          request.Actions.Add(act_exe);
        }

        var response = await _client.ExecuteActionsAsync(request);
        succ|= response.Success;
      }      

      return res;
    }

    [HttpPost()]
    [Route("GetActionsByObjectId")]
    public async Task<List<ActionExeInfoDTO>> GetActionsByObjectId(ActionExeInfoRequestDTO request)
    {
      var ret = await _actionsService.GetActionsByObjectId(request);
     
      return ret;
    }
    [HttpPost()]
    [Route("UpdateResults")]
    public async Task<List<ActionExeResultDTO>> UpdateResults(List<ActionExeResultDTO> results)
    {
      await _actionsUpdateService.UpdateResultsAsync(results);
      return results;
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
    public async Task<List<IntegroDTO>> UpdateIntegros([FromBody] List<IntegroDTO> integros)
    {
      await _integroUpdateService.UpdateIntegros(integros);
      return integros;
    }

    [HttpPost]
    [Route("UpdateIntegroObject")]
    public async Task<BaseMarkerDTO> UpdateIntegroObject(UpdateIntegroObjectDTO new_obj)
    {
      var props = await _mapUpdateService.UpdateProperties(new_obj.obj);

      new_obj.integro.id = props.id;

      await _integroUpdateService.UpdateIntegros(new List<IntegroDTO>() { new_obj.integro });
      return props;
    }
    

    [HttpDelete()]
    [Route("DeleteIntegros")]
    public async Task DeleteValues(List<string> ids)
    {
      await _integroUpdateService.RemoveIntegros(ids);
    }
    [HttpPost]
    [Route("UpdateIntegroTypes")]
    public async Task UpdateIntegroTypes(List<IntegroTypeDTO> types)
    {
      await _integroTypeUpdateService.UpdateTypesAsync(types);
    }

    [HttpDelete()]
    [Route("DeleteIntegroTypes")]
    public async Task DeleteIntegroTypes(List<IntegroTypeKeyDTO> types)
    {
      await _integroTypeUpdateService.RemoveTypesAsync(types);
    }

    [HttpPost()]
    [Route("GetIntegroTypes")]
    public async Task<List<IntegroTypeDTO>> GetIntegroTypes(List<IntegroTypeKeyDTO> types)
    {
      var dic = await _integroTypesService.GetTypesAsync(types);
      return dic.Values.ToList();
    }

    [HttpGet()]
    [Route("GetObjectIntegroType")]
    public async Task<ActionResult<IntegroTypeDTO>> GetObjectIntegroType(string id)
    {
      var types = await GetByIds(new List<string> { id });
      var type = types.FirstOrDefault();

      if (type == null)
        return NotFound(); // отдаёт 404

      var dic = await _integroTypesService.GetTypesAsync(new List<IntegroTypeKeyDTO>()
    {
        new IntegroTypeKeyDTO()
        {
            i_name = type.i_name,
            i_type = type.i_type,
        }
    });

      var result = dic.Values.ToList().FirstOrDefault();
      if (result == null)
        return NotFound(); // тоже 404, если не нашли

      return Ok(result); // 200 с JSON
    }

  }
}
