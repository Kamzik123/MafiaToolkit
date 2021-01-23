#include "MT_Wrangler.h"

#include "FbxUtilities.h"
#include "Source/MTObject/MT_Object.h"
#include "MTObject/MT_ObjectHandler.h"

MT_Wrangler::MT_Wrangler(const char* InName, const char* InDest)
{
	FbxName = InName;
	MTOName = InDest;
}

bool MT_Wrangler::SetupFbxManager()
{
	Fbx_Utilities::InitializeSdkObjects(SdkManager);
	if (!SdkManager)
	{
		// failed to load SDKManager
		return false;
	}
	return true;
}

bool MT_Wrangler::SetupImporter()
{
	Importer = FbxImporter::Create(SdkManager, "");
	FBX_ASSERT(Importer);

	if (!Importer->Initialize(FbxName, -1, SdkManager->GetIOSettings()))
	{
		// failed to setup importer and file
		return false;
	}

	// move imported data into scene
	Scene = FbxScene::Create(SdkManager, "Scene");
	Importer->Import(Scene);
	Importer->Destroy();

	return true;
}

bool MT_Wrangler::ConstructMTBFromFbx()
{
	SetupFbxManager();
	SetupImporter();

	LoadedBundle = new MT_ObjectBundle();

	FbxNode* RootNode = Scene->GetRootNode();
	std::vector<MT_Object> ObjectsForBundle = {};
	for (int i = 0; i < RootNode->GetChildCount(); i++)
	{
		FbxNode* ChildNode = RootNode->GetChild(i);
		const FbxString& NodeName = ChildNode->GetName();
		FbxNodeAttribute::EType NodeAttribute = GetNodeType(ChildNode);

		if (Fbx_Utilities::FindInString(NodeName, "[MESH]"))
		{
			MT_Object* NewObject = ConstructMesh(ChildNode);
			if (NewObject)
			{
				ObjectsForBundle.push_back(*NewObject);
			}
		}
		else if (Fbx_Utilities::FindInString(NodeName, "[MODEL]"))
		{
			// Convert to RIGGED Model.
		}	
	}

	LoadedBundle->SetObjects(ObjectsForBundle);

	return true;
}

bool MT_Wrangler::SaveBundleToFile()
{
	if (LoadedBundle)
	{
		MT_ObjectHandler::WriteBundleToFile(MTOName, *LoadedBundle);
		return true;
	}

	return false;
}

const FbxNodeAttribute::EType MT_Wrangler::GetNodeType(FbxNode* Node) const
{
	const FbxNodeAttribute* NodeAttribute = Node->GetNodeAttribute();
	if (NodeAttribute)
	{
		return NodeAttribute->GetAttributeType();
	}

	return FbxNodeAttribute::EType::eUnknown;
}

MT_Object* MT_Wrangler::ConstructMesh(FbxNode* Node)
{
	// Format name to respectfully
	FbxString NodeName = Node->GetName();
	NodeName.FindAndReplace("[MESH]", "");
	std::string RawName = NodeName.Buffer();

	// Construct object and set name
	MT_Object* ModelObject = new MT_Object();
	ModelObject->SetName(RawName);
	ModelObject->SetObjectFlags(MT_ObjectFlags::HasLODs);

	std::vector<MT_Lod> Lods = {};

	FbxInt32 NumLods = Node->GetChildCount();
	Lods.resize(NumLods);
	for (int i = 0; i < NumLods; i++)
	{
		FbxNode* LodNode = Node->GetChild(i);
		Lods[i] = *ConstructFromLod(LodNode);
	}

	ModelObject->SetLods(Lods);
	
	return ModelObject;
}

MT_Lod* MT_Wrangler::ConstructFromLod(FbxNode* Lod)
{
	MT_Lod* LodObject = new MT_Lod();
	LodObject->ResetVertexFlags();

	FbxMesh* Mesh = Lod->GetMesh();

	FbxGeometryElementUV* DiffuseUV = Mesh->GetElementUV();

	// Construct Vertex Array
	FbxInt32 NumVertices = Mesh->GetControlPointsCount();
	std::vector<Vertex> Vertices = {};
	Vertices.resize(NumVertices);
	for (int i = 0; i < NumVertices; i++)
	{
		Vertex NewVertex = {};

		FbxVector4 ControlPoint = Mesh->GetControlPointAt(i);

		NewVertex.position = { (float)ControlPoint[0], (float)ControlPoint[1], (float)ControlPoint[2] };

		if (DiffuseUV && DiffuseUV->GetMappingMode() == FbxLayerElement::eByControlPoint)
		{
			const FbxVector2& Value = DiffuseUV->GetDirectArray().GetAt(i);
			NewVertex.uv0 = { (float)Value[0], (float)Value[1] };
			LodObject->AddVertexFlag(TexCoords0);
		}

		Vertices[i] = NewVertex;
	}

	LodObject->AddVertexFlag(Position);
	LodObject->SetVertices(Vertices);

	// Get DiffuseMaterial, we'll need to use it to create Indices & FaceGroup
	FbxGeometryElementMaterial* DiffuseMaterial = Mesh->GetElementMaterial(0);
	FBX_ASSERT(DiffuseMaterial);

	// Construct Indices & FaceGroups Array
	std::vector<std::vector<Int3>> FaceGroupLookup = {};
	FaceGroupLookup.resize(Lod->GetMaterialCount());
	for (int i = 0; i < Mesh->GetPolygonCount(); i++)
	{
		Int3 Triangle = {};
		Triangle.i1 = Mesh->GetPolygonVertex(i, 0);
		Triangle.i2 = Mesh->GetPolygonVertex(i, 1);
		Triangle.i3 = Mesh->GetPolygonVertex(i, 2);

		if (DiffuseMaterial)
		{
			int MaterialAssignment = DiffuseMaterial->GetIndexArray().GetAt(i);
			FaceGroupLookup[MaterialAssignment].push_back(Triangle);
		}
	}

	// Reorder Indices by FaceGroups then push into LodObject
	std::vector<Int3> Indices = {};
	std::vector<MT_FaceGroup> FaceGroups = {};
	FaceGroups.resize(FaceGroupLookup.size());
	FbxUInt32 CurrentTotal = 0;
	for (int i = 0; i < FaceGroupLookup.size(); i++)
	{
		Indices.insert(Indices.end(), FaceGroupLookup[i].begin(), FaceGroupLookup[i].end());

		size_t NumFaces = FaceGroupLookup[i].size();
		MT_FaceGroup NewFaceGroup = {};
		NewFaceGroup.SetNumFaces(NumFaces);
		NewFaceGroup.SetStartIndex(CurrentTotal);

		// Update CurrentTotal, then insert into array
		CurrentTotal += NumFaces * 3;
		FaceGroups[i] = NewFaceGroup;
	}

	// Finish by setting indices
	LodObject->SetIndices(Indices);

	// Get Material name and texture from FbxSurfaceMaterials
	for (int i = 0; i < Lod->GetMaterialCount(); i++)
	{
		MT_FaceGroup& FaceGroup = FaceGroups[i];
		FbxSurfaceMaterial* Material = Lod->GetMaterial(i);
		FBX_ASSERT(Material);

		MT_MaterialInstance* NewInstance = new MT_MaterialInstance();
		NewInstance->SetName(Material->GetName());

		const char* DiffuseTexture = Material->sDiffuse;
		NewInstance->SetTextureName(DiffuseTexture);
		NewInstance->SetMaterialFlags(MT_MaterialInstanceFlags::HasDiffuse);
		FaceGroup.SetMatInstance(NewInstance);
	}

	// Finally, set face groups.
	LodObject->SetFaceGroups(FaceGroups);

	return LodObject;
}
