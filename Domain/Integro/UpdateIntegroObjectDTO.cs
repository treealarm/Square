namespace Domain.Integro
{
  public class UpdateIntegroObjectDTO
  {
    public ObjPropsDTO obj {  get; set; } = new ObjPropsDTO();
    public IntegroDTO integro { get; set; } = new IntegroDTO();
  }
}
