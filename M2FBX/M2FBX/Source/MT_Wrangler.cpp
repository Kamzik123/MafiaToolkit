#include "MT_Wrangler.h"

#include "FbxUtilities.h"
#include "Source/MTObject/MT_Collision.h"
#include "Source/MTObject/MT_FaceGroup.h"
#include "Source/MTObject/MT_Object.h"
#include "Source/MTObject/MT_ObjectUtils.h"
#include "Source/MTObject/MT_Skeleton.h"
#include "MTObject/MT_ObjectHandler.h"

#include <map>

namespace WranglerUtils
{
	std::map<uint, std::string> UVLookupMap
	{
		{0, "DiffuseUV"},
		{1, "UV0"},
		{2, "UV1"},
		{3, "OMUV"},
	};
}

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
		else 
		{
			// Attempt conversion to other Frame Type
			MT_Object* NewObject = ConstructBaseObject(ChildNode);
			if (NewObject)
			{
				ObjectsForBundle.push_back(*NewObject);
			}
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

MT_Object* MT_Wrangler::ConstructBaseObject(FbxNode* Node)
{
	// Format name to respectful name
	FbxString NodeName = Node->GetName();
	std::string RawName = NodeName.Buffer();
	MT_ObjectUtils::RemoveMetaTagFromName(RawName);

	// Construct object and set name
	MT_Object* ModelObject = new MT_Object();
	ModelObject->SetName(RawName);
	ModelObject->SetObjectFlags(MT_ObjectFlags::HasChildren);
	ModelObject->SetType(MT_ObjectUtils::GetTypeFromString(Node->GetName()));

	// Get Objects transform
	TransformStruct TransformObject = {};
	const FbxDouble3& Position = Node->LclTranslation;
	const FbxDouble3& Rotation = Node->LclRotation;
	const FbxDouble3& Scale = Node->LclScaling;
	TransformObject.Position = { (float)Position[0], (float)Position[1], (float)Position[2] };
	TransformObject.Rotation = { (float)Rotation[0], (float)Rotation[1], (float)Rotation[2] };
	TransformObject.Scale = { (float)Scale[0], (float)Scale[1], (float)Scale[2] };
	ModelObject->SetTransform(TransformObject);

	// Setup Children
	std::vector<MT_Object> Children = {};
	FbxInt32 NumChildren = Node->GetChildCount();
	Children.resize(NumChildren);
	for (int i = 0; i < NumChildren; i++)
	{
		MT_Object* NewChildObject;

		FbxNode* ChildNode = Node->GetChild(i);
		const FbxString& ChildName = ChildNode->GetName();
		if (ChildName.Find("MESH"))
		{
			NewChildObject = ConstructMesh(ChildNode);
		}
		else
		{
			NewChildObject = ConstructBaseObject(ChildNode);
		}

		Children[i] = *NewChildObject;
	}

	// Update ModelObject's array
	ModelObject->SetChildren(Children);

	return ModelObject;
}

MT_Object* MT_Wrangler::ConstructMesh(FbxNode* Node)
{
	MT_Object* ModelObject = ConstructBaseObject(Node);

	// Model Conversion
	std::vector<MT_Lod> Lods = {};
	FbxInt32 NumLods = Node->GetChildCount();
	for (int i = 0; i < NumLods; i++)
	{
		FbxNode* LodNode = Node->GetChild(i);
		FbxString NodeName = LodNode->GetName();
		if (NodeName.Find("LOD") != -1)
		{
			Lods.push_back(*ConstructFromLod(LodNode));
		}
	}

	ModelObject->SetLods(Lods);


	// Collision Conversion
	FbxNode* CollisionNode = Node->FindChild("COL");
	if (CollisionNode)
	{
		// checks are done in SetCollisions, flag is added too.
		ModelObject->SetCollisions(ConstructCollision(CollisionNode));
	}

	// Skeleton Conversion
	FbxNode* SkeletonNode = Node->FindChild("Root");
	if (SkeletonNode)
	{
		MT_Skeleton* SkeletonObject = ConstructSkeleton(SkeletonNode);
		ModelObject->SetSkeleton(SkeletonObject);
	}

	return ModelObject;
}

MT_Collision* MT_Wrangler::ConstructCollision(FbxNode* Node)
{
	FbxMesh* Mesh = Node->GetMesh();
	MT_Collision* Collision = new MT_Collision();

	std::vector<Point3> Vertices = {};
	Vertices.resize(Mesh->GetControlPointsCount());

	// Transfer vertices into Collision
	for (size_t i = 0; i < Vertices.size(); i++)
	{
		const FbxVector4& ControlPoint = Mesh->GetControlPointAt(i);
		Point3 NewVertex = { (float)(ControlPoint[0]), (float)(ControlPoint[1]), (float)(ControlPoint[2]) };
		Vertices[i] = NewVertex;
	}

	Collision->SetVertices(Vertices);

	// Transfer indices and facegroups into Collision
	std::vector<Int3> Indices = {};
	std::vector<MT_FaceGroup> FaceGroups = {};
	ConstructIndicesAndFaceGroupsFromNode(Node, &Indices, &FaceGroups);

	// Get Material name and texture from FbxSurfaceMaterials
	for (int i = 0; i < Node->GetMaterialCount(); i++)
	{
		MT_FaceGroup& FaceGroup = FaceGroups[i];
		FbxSurfaceMaterial* Material = Node->GetMaterial(i);
		FBX_ASSERT(Material);

		MT_MaterialInstance* NewInstance = new MT_MaterialInstance();
		NewInstance->SetName(Material->GetName());
		NewInstance->SetMaterialFlags(MT_MaterialInstanceFlags::IsCollision);
		FaceGroup.SetMatInstance(NewInstance);
	}

	Collision->SetIndices(Indices);
	Collision->SetFaceGroups(FaceGroups);

	return Collision;
}

MT_Lod* MT_Wrangler::ConstructFromLod(FbxNode* Lod)
{
	MT_Lod* LodObject = new MT_Lod();
	LodObject->ResetVertexFlags();

	FbxMesh* Mesh = Lod->GetMesh();

	FbxGeometryElementUV* DiffuseUVElement = GetUVElementByIndex(Mesh, 0);
	FbxGeometryElementUV* UV0Element = GetUVElementByIndex(Mesh, 1);
	FbxGeometryElementUV* UV1Element = GetUVElementByIndex(Mesh, 2);
	FbxGeometryElementUV* OMUVElement = GetUVElementByIndex(Mesh, 3);
	FbxGeometryElementNormal* NormalElement = Mesh->GetElementNormal(0);
	FbxGeometryElementTangent* TangentElement = Mesh->GetElementTangent(0);

	// Construct Vertex Array
	FbxInt32 NumVertices = Mesh->GetControlPointsCount();
	std::vector<Vertex> Vertices = {};
	Vertices.resize(NumVertices);
	for (int i = 0; i < NumVertices; i++)
	{
		Vertex NewVertex = {};

		FbxVector4 ControlPoint = Mesh->GetControlPointAt(i);

		NewVertex.position = { (float)ControlPoint[0], (float)ControlPoint[1], (float)ControlPoint[2] };

		// Add Normal element to Vertex
		if (NormalElement)
		{
			const FbxVector4& Value = NormalElement->GetDirectArray().GetAt(i);
			NewVertex.normals = { (float)Value[0], (float)Value[1], (float)Value[2] };
			LodObject->AddVertexFlag(Normals);
		}

		// Add Tangent element to Vertex
		if (TangentElement)
		{
			const FbxVector4& Value = TangentElement->GetDirectArray().GetAt(i);
			NewVertex.tangent = { (float)Value[0], (float)Value[1], (float)Value[2] };
			LodObject->AddVertexFlag(Tangent);
		}

		// Add Diffuse element to Vertex
		if (DiffuseUVElement)
		{
			const FbxVector2& Value = DiffuseUVElement->GetDirectArray().GetAt(i);
			NewVertex.uv0 = { (float)Value[0], (float)Value[1] };
			LodObject->AddVertexFlag(TexCoords0);
		}

		// Add UV0 element to Vertex
		if (UV0Element)
		{
			const FbxVector2& Value = UV0Element->GetDirectArray().GetAt(i);
			NewVertex.uv1 = { (float)Value[0], (float)Value[1] };
			LodObject->AddVertexFlag(TexCoords1);
		}

		// Add UV1 element to Vertex
		if (UV1Element)
		{
			const FbxVector2& Value = UV1Element->GetDirectArray().GetAt(i);
			NewVertex.uv2 = { (float)Value[0], (float)Value[1] };
			LodObject->AddVertexFlag(TexCoords2);
		}

		// Add OMUV element to Vertex
		if (OMUVElement)
		{
			const FbxVector2& Value = OMUVElement->GetDirectArray().GetAt(i);
			NewVertex.uv3 = { (float)Value[0], (float)Value[1] };
			LodObject->AddVertexFlag(ShadowTexture);
		}

		Vertices[i] = NewVertex;
	}

	LodObject->AddVertexFlag(Position);
	LodObject->SetVertices(Vertices);

	std::vector<Int3> Indices = {};
	std::vector<MT_FaceGroup> FaceGroups = {};
	ConstructIndicesAndFaceGroupsFromNode(Lod, &Indices, &FaceGroups);

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

MT_Skeleton* MT_Wrangler::ConstructSkeleton(FbxNode* Node)
{
	MT_Skeleton* SkeletonObject = new MT_Skeleton();

	SkeletonState.JointLookupTable = {};
	SkeletonState.Joints = {};

	ConstructJoint(Node);

	for (size_t i = 0; i < SkeletonState.Joints.size(); i++)
	{
		MT_Joint& JointObject = SkeletonState.Joints[i];

		// Sort out Parent Index
		const std::string& Name = JointObject.GetParentName();
		const bool bExists = SkeletonState.JointLookupTable.find(Name) != SkeletonState.JointLookupTable.end();
		if (bExists)
		{
			int NodeIdx = SkeletonState.JointLookupTable[Name];
			JointObject.SetParentJointIndex(NodeIdx);
		}
		else
		{
			JointObject.SetParentJointIndex(0xFF);
		}
	}

	SkeletonObject->SetJoints(SkeletonState.Joints);

	return SkeletonObject;
}

MT_Joint* MT_Wrangler::ConstructJoint(FbxNode* Node)
{
	// Check FbxNodeAttribute
	if (FbxNodeAttribute* const NodeAttribute = Node->GetNodeAttribute())
	{
		if (NodeAttribute->GetAttributeType() != FbxNodeAttribute::eSkeleton)
		{
			return nullptr;
		}
	}

	// Set Name and Usage.
	MT_Joint* JointObject = new MT_Joint();
	JointObject->SetName(Node->GetName());
	JointObject->SetParentName(Node->GetParent()->GetName());
	JointObject->SetUsage(MT_JointUsage::LOD0);

	// Get Node Matrix -> Set Joint Transform
	JointMatrix Matrix = {};
	FbxAMatrix& lLocalTransform = Node->EvaluateLocalTransform();
	const FbxDouble3 Position = Node->LclTranslation.Get();
	Matrix.Position = { (float)Position[0], (float)Position[1], (float)Position[2] };
	const FbxDouble3 Scale = Node->LclScaling.Get();
	Matrix.Scale = { (float)Scale[0], (float)Scale[1], (float)Scale[2] };
	const FbxDouble4 Rotation = lLocalTransform.GetR();
	Matrix.Rotation = { (float)Rotation[0], (float)Rotation[1], (float)Rotation[2], (float)Rotation[3] };
	JointObject->SetTransform(Matrix);

	// Add to the state
	SkeletonState.JointLookupTable.insert(std::pair<std::string, int>(Node->GetName(), (int)SkeletonState.JointLookupTable.size()));
	SkeletonState.Joints.push_back(*JointObject);

	for (int i = 0; i < Node->GetChildCount(); i++)
	{
		FbxNode* const ChildNode = Node->GetChild(i);
		MT_Joint* const ChildJoint = ConstructJoint(ChildNode);
	}

	// return
	return JointObject;
}

void MT_Wrangler::ConstructIndicesAndFaceGroupsFromNode(FbxNode* TargetNode, std::vector<Int3>* Indices, std::vector<MT_FaceGroup>* FaceGroups)
{
	FbxMesh* Mesh = TargetNode->GetMesh();

	// Get DiffuseMaterial, we'll need to use it to create Indices & FaceGroup
	FbxGeometryElementMaterial* DiffuseMaterial = Mesh->GetElementMaterial(0);
	FBX_ASSERT(DiffuseMaterial);
	int MaterialCount = DiffuseMaterial ? TargetNode->GetMaterialCount() : 1;

	// Construct Indices & FaceGroups Array
	std::vector<std::vector<Int3>> FaceGroupLookup = {};
	FaceGroupLookup.resize(MaterialCount);
	for (int i = 0; i < Mesh->GetPolygonCount(); i++)
	{
		Int3 Triangle = {};
		Triangle.i1 = Mesh->GetPolygonVertex(i, 0);
		Triangle.i2 = Mesh->GetPolygonVertex(i, 1);
		Triangle.i3 = Mesh->GetPolygonVertex(i, 2);

		int MaterialAssignment = DiffuseMaterial ? DiffuseMaterial->GetIndexArray().GetAt(i) : 0;
		FaceGroupLookup[MaterialAssignment].push_back(Triangle);
	}

	// Reorder Indices by FaceGroups then push into LodObject
	FbxUInt32 CurrentTotal = 0;
	for (int i = 0; i < FaceGroupLookup.size(); i++)
	{
		Indices->insert(Indices->end(), FaceGroupLookup[i].begin(), FaceGroupLookup[i].end());

		size_t NumFaces = FaceGroupLookup[i].size();
		MT_FaceGroup NewFaceGroup = {};
		NewFaceGroup.SetNumFaces(NumFaces);
		NewFaceGroup.SetStartIndex(CurrentTotal);

		// Update CurrentTotal, then insert into array
		CurrentTotal += NumFaces * 3;
		FaceGroups->push_back(NewFaceGroup);
	}
}

FbxGeometryElementUV* MT_Wrangler::GetUVElementByIndex(FbxMesh* Mesh, uint ElementType) const
{
	std::map<uint, std::string>& LookupMap = WranglerUtils::UVLookupMap;
	std::string& ElementName = LookupMap[ElementType];
	FbxGeometryElementUV* Element = nullptr;

	if (!ElementName.empty())
	{
		Element = Mesh->GetElementUV(ElementName.data());
		if (Element)
		{
			return Element;
		}
	}

	return Mesh->GetElementUV(ElementType);
}
