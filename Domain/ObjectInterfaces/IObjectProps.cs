using System.Collections.Generic;

namespace Domain
{
    public interface IObjectProps: IIdentifiable
    {
        public List<ObjExtraPropertyDTO>? extra_props { get; set; }
    }
}
