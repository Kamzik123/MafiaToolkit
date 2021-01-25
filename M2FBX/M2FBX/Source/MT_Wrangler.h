#pragma once

#include "Common.h"

#include "Source/MTObject/MT_FaceGroup.h"

class MT_Collision;
class MT_Object;
class MT_ObjectBundle;
class MT_Lod;

class MT_Wrangler
{
public:

	MT_Wrangler() = default;
	MT_Wrangler(const char* InName, const char* InDest);

	bool SetupFbxManager();
	bool SetupImporter();
	bool ConstructMTBFromFbx();
	bool SaveBundleToFile();

private:

	const FbxNodeAttribute::EType GetNodeType(FbxNode* Node) const;

	MT_Object* ConstructMesh(FbxNode* Node);
	MT_Collision* ConstructCollision(FbxNode* Node);
	MT_Lod* ConstructFromLod(FbxNode* Lod);

	void ConstructIndicesAndFaceGroupsFromNode(FbxNode* TargetNode, std::vector<Int3>* Indices, std::vector<MT_FaceGroup>* FaceGroups);
	FbxGeometryElementUV* GetUVElementByIndex(FbxMesh* Mesh, uint ElementType) const;

	const char* MTOName;
	const char* FbxName;

	MT_ObjectBundle* LoadedBundle = nullptr;

	// Fbx related
	FbxManager* SdkManager = nullptr;
	FbxImporter* Importer = nullptr;
	FbxScene* Scene = nullptr;
};

