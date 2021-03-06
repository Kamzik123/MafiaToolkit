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

        protected override bool InternalValidate(MT_ValidationTracker TrackerObject)
        {
            bool bValidity = true;

            if(string.IsNullOrEmpty(Name))
            {
               AddMessage(MT_MessageType.Error, "This Material has no name.");
                bValidity = false;
            }

            if (MaterialFlags.HasFlag(MT_MaterialInstanceFlags.IsCollision))
            {
                Collisions.CollisionMaterials MaterialChoice = Collisions.CollisionMaterials.Undefined;
                if(Enum.TryParse(Name, out MaterialChoice))
                {
                    AddMessage(MT_MessageType.Error, "This Material is set to Collision, yet it cannot be converted to CollisionEnum - {0}", Name);
                    bValidity = false;
                }
            }

            return bValidity;
        }
    }
}
