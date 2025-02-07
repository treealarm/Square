using System.Collections.Generic;

namespace Domain
{
    public interface IObjectProps
    {
        public List<ObjExtraPropertyDTO>? extra_props { get; set; }
    }
}
