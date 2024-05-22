using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IDataChangeService
  {
    public Task<ObjPropsDTO?> UpdateProperties(ObjPropsDTO updatedMarker);
  }
}
