using Domain;

namespace DataChangeLayer
{
  internal class ActionsUpdateService: IActionsUpdateService
  {
    private readonly IActionsServiceInternal _actionsService;
    public ActionsUpdateService(IActionsServiceInternal actionsService)
    {
      _actionsService = actionsService;
    }

    async public Task UpdateListAsync(List<ActionExeDTO> actions)
    {
      await _actionsService.UpdateListAsync(actions);
    }

    async public Task UpdateResultsAsync(List<ActionExeResultDTO> results)
    {
      await _actionsService.UpdateResultsAsync(results);
    }
  }
}
