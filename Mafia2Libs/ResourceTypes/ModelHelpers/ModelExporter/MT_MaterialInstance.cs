using System;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    [Flags]
    public enum MT_MaterialInstanceFlags
    {
        IsCollision = 1,
        HasDiffuse = 2
    }

    public class MT_MaterialInstance : IValidator
    {
        public MT_MaterialInstanceFlags MaterialFlags { get; set; }
        public string Name { get; set; }
        public string DiffuseTexture { get; set; }

        public MT_MaterialInstance()
        {
            Name = "";
            DiffuseTexture = "";
        }

        public bool Validate()
        {
            bool bValidity = true;

            bValidity = !string.IsNullOrEmpty(Name);

            if(MaterialFlags.HasFlag(MT_MaterialInstanceFlags.IsCollision))
            {
                Collisions.CollisionMaterials MaterialChoice = Collisions.CollisionMaterials.Undefined;
                bValidity = Enum.TryParse(Name, out MaterialChoice);
            }

            return bValidity;
        }
    }
}
