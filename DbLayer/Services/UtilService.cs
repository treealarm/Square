using Domain.ServiceInterfaces;
using MongoDB.Bson;

namespace DbLayer.Services
{
  internal class UtilService: IUtilService
  {
    public int Compare(string id1, string id2)
    {
      return new ObjectId(id1).CompareTo(new ObjectId(id2));
    }
  }
}
