using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ObjectInterfaces
{
    public interface IObjectProps
    {
        public List<ObjExtraPropertyDTO> extra_props { get; set; }
    }
}
