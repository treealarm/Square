using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public static class PropertyCopy
  {
    public static void CopyAllTo<T, T1>(this T source, T1 target)
    {
      var type = typeof(T);
      var type1 = typeof(T1);

      foreach (var sourceProperty in type.GetProperties())
      {
        foreach (var targetProperty in type1.GetProperties()
          .Where(targetProperty => sourceProperty.Name == targetProperty.Name)
          .Where(targetProperty => targetProperty.SetMethod != null))
        {
          targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
        }
      }

      foreach (var sourceField in type.GetFields())
      {
        foreach (var targetField in type1.GetFields()
          .Where(targetField => sourceField.Name == targetField.Name))
        {
          targetField.SetValue(target, sourceField.GetValue(source));
        }
      }
    }
  }
}
