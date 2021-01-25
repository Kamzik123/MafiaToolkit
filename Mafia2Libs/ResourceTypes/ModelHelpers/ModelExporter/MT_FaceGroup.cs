using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    public class MT_FaceGroup
    {
        public BoundingBox Bounds { get; set; }
        public uint StartIndex { get; set; }
        public uint NumFaces { get; set; }
        public MT_MaterialInstance Material { get; set; }
    }
}
