using System;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    [Flags]
    public enum MT_MaterialInstanceFlags
    {
        IsCollision = 1,
        HasDiffuse = 2
    }

    public class MT_MaterialInstance
    {
        public MT_MaterialInstanceFlags MaterialFlags { get; set; }
        public string Name { get; set; }
        public string DiffuseTexture { get; set; }

        public MT_MaterialInstance()
        {
            Name = "";
            DiffuseTexture = "";
        }
    }
}
