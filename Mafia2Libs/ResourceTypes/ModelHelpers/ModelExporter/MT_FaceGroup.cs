using SharpDX;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    public class MT_FaceGroup : IValidator
    {
        public BoundingBox Bounds { get; set; }
        public uint StartIndex { get; set; }
        public uint NumFaces { get; set; }
        public MT_MaterialInstance Material { get; set; }

        public bool Validate()
        {
            bool bValidity = true;

            bValidity = NumFaces > 0;
            bValidity = Material.Validate();

            return bValidity;
        }
    }
}
