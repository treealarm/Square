using System.Collections.Generic;

namespace Domain.ObjectInterfaces
{
    public interface IObjectProps
    {
        public List<ObjExtraPropertyDTO>? extra_props { get; set; }
    }
}
