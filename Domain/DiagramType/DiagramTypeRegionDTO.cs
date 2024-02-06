using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Diagram;

namespace Domain.DiagramType
{
    public record DiagramTypeRegionDTO
    {
        public string id { get; set; }
        public DiagramCoordDTO geometry { get; set; }
    }
}
