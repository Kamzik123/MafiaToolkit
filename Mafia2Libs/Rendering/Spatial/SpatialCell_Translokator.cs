﻿namespace Rendering.Core
{
    public class SpatialCell_TranslokatorParams : SpatialCell_InitParams
    {
    }

    public class SpatialCell_Translokator : SpatialCell
    {
        public SpatialCell_Translokator(SpatialCell_InitParams InitParams) : base(InitParams)
        {
            if (InitParams is SpatialCell_TranslokatorParams)
            {
                // TODO:
            }
        }
    }
}
