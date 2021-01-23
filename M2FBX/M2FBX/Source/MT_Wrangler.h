#pragma once

#include "Common.h"

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
	MT_Lod* ConstructFromLod(FbxNode* Lod);

	const char* MTOName;
	const char* FbxName;

	MT_ObjectBundle* LoadedBundle = nullptr;

	// Fbx related
	FbxManager* SdkManager = nullptr;
	FbxImporter* Importer = nullptr;
	FbxScene* Scene = nullptr;
};

