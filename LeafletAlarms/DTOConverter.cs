using Domain;

namespace LeafletAlarms
{
  public class DTOConverter
  {
    public static MarkerDTO GetMarkerDTO(BaseMarkerDTO marker)
    {
      return new MarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static ObjPropsDTO GetObjPropsDTO(BaseMarkerDTO marker)
    {
      return new ObjPropsDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }
  }
}
