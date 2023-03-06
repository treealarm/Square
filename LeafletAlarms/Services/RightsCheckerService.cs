using Domain.ServiceInterfaces;
using LeafletAlarms.Authentication;
using Microsoft.AspNetCore.Http;
using OsmSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Domain.Rights.ObjectRightValueDTO;

namespace LeafletAlarms.Services
{  
  public class RightsCheckerService
  {
    private IRightService _rightService;
    private IHttpContextAccessor _httpContextAccessor;
    public RightsCheckerService(
      IRightService rightService,
      IHttpContextAccessor httpContextAccessor
    )
    {
      _rightService = rightService;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<HashSet<string>> CheckForControl(List<string> ids)
    {
      return await CheckForRight(ids, ERightValue.Control);
    }

    public async Task<HashSet<string>> CheckForView(List<string> ids)
    {
      return await CheckForRight(ids, ERightValue.View);
    }
    private async Task<HashSet<string>> CheckForRight(List<string> ids, ERightValue rightToCheck)
    {
      if (_httpContextAccessor.HttpContext.User.IsInRole(RoleConstants.admin))
      {
        return ids.ToHashSet();
      }

      HashSet<string> roles = new HashSet<string>();

      foreach (var item in _httpContextAccessor.HttpContext.User.Identities)
      { 
        var temp = item.Claims
          .Where(c => c.Type == ClaimTypes.Role || c.Type == ClaimTypes.Name)
          .Select(c => c.Value)
          .ToHashSet();

        roles.UnionWith(temp);
      }

      Console.WriteLine($"CheckForRight: {string.Join(",", roles)}");

      var ret = new HashSet<string>();

      var rights = await _rightService.GetListByIdsAsync(ids);

      foreach (var id in ids)
      {
        if (rights.TryGetValue(id, out var right))
        {
          var myRights = right.rights.Where(r => roles.Contains(r.role));

          foreach (var r in myRights)
          {
            if (rightToCheck == ERightValue.View)
            {
              if (r.value != ERightValue.None)
              {
                ret.Add(id);
                break;
              }
            }
            
            if (r.value.HasFlag(rightToCheck))
            {
              ret.Add(id);
              break;
            }
          }
          
          continue;
        }
        ret.Add(id);
      }
      return ret;
    }
  }
}
