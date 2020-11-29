#include "MT_Lod.h"

void MT_MaterialInstance::Cleanup()
{
	Name = "";
	DiffuseTexture = "";
}

void MT_FaceGroup::Cleanup()
{
	if (MaterialInstance)
	{
		MaterialInstance->Cleanup();
		MaterialInstance = nullptr;
	}
}

bool MT_Lod::HasVertexFlag(const VertexFlags Flag) const
{
	return (VertexDeclaration & Flag);
}

void MT_Lod::Cleanup()
{
	// Empty FaceGroups
	for (auto& FaceGroup : FaceGroups)
	{
		FaceGroup.Cleanup();
	}

	// Cleanup other vectors
	FaceGroups.clear();
	Vertices.clear();
	Indices.clear();
}

void MT_Collision::Cleanup()
{
	// Cleanup other vectors
	MaterialAssignments.clear();
	Vertices.clear();
	Indices.clear();	
}
