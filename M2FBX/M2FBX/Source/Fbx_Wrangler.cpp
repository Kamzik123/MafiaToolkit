#include "Fbx_Wrangler.h"

#include "FbxUtilities.h"
#include "Source/MTObject/MT_Collision.h"
#include "Source/MTObject/MT_Object.h"
#include "Source/MTObject/MT_ObjectHandler.h"
#include "Source/MTObject/MT_Lod.h"
#include "Source/MTObject/MT_Skeleton.h"

namespace WranglerUtils
{
	const std::string DiffuseUVName = "DiffuseUV";
	const std::string UV0Name = "UV0";
	const std::string UV1Name = "UV1";
	const std::string OMUVName = "OMUV";
	const std::string Empty = "";
}

Fbx_Wrangler::Fbx_Wrangler(const char* InName, const char* InDest)
{
	MTOName = InName;
	FbxName = InDest;
}

Fbx_Wrangler::~Fbx_Wrangler()
{
	LoadedObject->Cleanup();

	Scene->Destroy(true);
	Fbx_Utilities::DestroySdkObjects(SdkManager, true);
}

bool Fbx_Wrangler::SetupFbxManager()
{
	Fbx_Utilities::InitializeSdkObjects(SdkManager);
	if (!SdkManager)
	{
		// failed to load SDKManager
		return false;
	}
	return true;
}

bool Fbx_Wrangler::ConstructScene()
{
	// Create document info
	FbxDocumentInfo* DocInfo = FbxDocumentInfo::Create(SdkManager, "DocInfo");
	DocInfo->mTitle = "FBX Model";
	DocInfo->mSubject = "FBX Created by M2FBX - Used by MafiaToolkit.";
	DocInfo->mAuthor = "Greavesy";
	DocInfo->mRevision = "rev. 0.30";
	DocInfo->mKeywords = "";
	DocInfo->mComment = "";

	Scene = FbxScene::Create(SdkManager, "ToolkitScene");
	Scene->SetDocumentInfo(DocInfo);
	
	return true;
}

bool Fbx_Wrangler::ConvertObjectToFbx()
{
	SetupFbxManager();
	ConstructScene();

	MT_Object* Object = MT_ObjectHandler::ReadObjectFromFile(MTOName);
	if (!Object)
	{
		// Failed
		return false;
	}

	bool bResult = ConvertObjectToNode(*Object);
	SaveDocument();
	return bResult;
}

bool Fbx_Wrangler::ConvertBundleToFbx()
{
	SetupFbxManager();
	ConstructScene();

	MT_ObjectBundle* Bundle = MT_ObjectHandler::ReadBundleFromFile(MTOName);
	if (!Bundle)
	{
		// Failed
		return false;
	}

	const std::vector<MT_Object>& Objects = Bundle->GetObjects();
	for (auto& Object : Objects)
	{
		ConvertObjectToNode(Object);
	}

	return true;
}

bool Fbx_Wrangler::ConvertObjectToNode(const MT_Object& Object)
{
	std::string ObjectName = Object.GetName();
	ObjectName += " [MESH]";
	FbxNode* RootNode = FbxNode::Create(SdkManager, ObjectName.data());
	
	// Setup transform of object
	const TransformStruct& Transform = Object.GetTransform();
	RootNode->LclTranslation = { Transform.Position.x, Transform.Position.y , Transform.Position.z };
	RootNode->LclRotation = { Transform.Rotation.x, Transform.Rotation.y , Transform.Rotation.z };
	RootNode->LclScaling = { Transform.Scale.x, Transform.Scale.y , Transform.Scale.z };

	FbxSkin* Skin = nullptr;
	if (Object.HasObjectFlag(HasSkinning))
	{
		const MT_Skeleton* Skeleton = Object.GetSkeleton();
		Skin = FbxSkin::Create(SdkManager, "Skin");
		FbxNode* Joint = nullptr;
		ConvertSkeletonToNode(*Skeleton, Skin, Joint, 0);
		RootNode->AddChild(Joint);
	}

	if (Object.HasObjectFlag(HasLODs))
	{
		const std::vector<MT_Lod>& ModelLods = Object.GetLods();
		for (size_t i = 0; i < ModelLods.size(); i++)
		{
			// Setup name and get lod
			std::string NodeName = "LOD";
			NodeName += std::to_string(i);
			const MT_Lod& Lod = ModelLods[i];

			// Create lod and convert to object
			FbxNode* NewLodNode = FbxNode::Create(SdkManager, NodeName.data());
			bool bResult = ConvertLodToNode(Lod, NewLodNode);
			RootNode->AddChild(NewLodNode);

			if (Object.HasObjectFlag(HasSkinning))
			{
				bResult = ApplySkinToMesh(Lod, Skin, NewLodNode);
			}
		}
	}

	if (Object.HasObjectFlag(HasCollisions))
	{
		const MT_Collision* Collision = Object.GetCollision();

		FbxNode* CollisionNode = FbxNode::Create(SdkManager, "COL");
		bool bResult = ConvertCollisionToNode(*Collision, CollisionNode);
		RootNode->AddChild(CollisionNode);
	}

	FbxNode* SceneRootNode = Scene->GetRootNode();
	SceneRootNode->AddChild(RootNode);
	SaveDocument();
	return true;
}

bool Fbx_Wrangler::ApplySkinToMesh(const MT_Lod& LodObject, FbxSkin* Skin, FbxNode* MeshNode)
{
	if (LodObject.HasVertexFlag(VertexFlags::Skin))
	{
		const std::vector<Vertex>& Vertices = LodObject.GetVertices();
		for (size_t x = 0; x < Vertices.size(); x++)
		{
			Vertex Vert = Vertices[x];
			for (int z = 0; z < 4; z++)
			{
				if (Vert.boneWeights[z] != 0.0f)
				{
					FbxCluster* Cluster = Skin->GetCluster(Vert.boneIDs[z]);
					Cluster->AddControlPointIndex(x, Vert.boneWeights[z]);
				}
			}
		}
	}

	return true;
}

bool Fbx_Wrangler::ConvertLodToNode(const MT_Lod& Lod, FbxNode* LodNode)
{
	// Create new FbxNodes and then set generic settings
	FbxMesh* Mesh = FbxMesh::Create(SdkManager, "Mesh");
	LodNode->SetNodeAttribute(Mesh);
	LodNode->SetShadingMode(FbxNode::eTextureShading);

	// Create main layer
	FbxLayer* Layer0 = Mesh->GetLayer(0);
	if (!Layer0)
	{
		Mesh->CreateLayer();
		Layer0 = Mesh->GetLayer(0);
	}

	// Setup initial control point array
	const std::vector<Vertex>& Vertices = Lod.GetVertices();
	Mesh->InitControlPoints(Vertices.size());
	
	// Get ControlPoints and begin filling in our Vertices data.
	if (FbxVector4* ControlPoints = Mesh->GetControlPoints())
	{
		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const Point3& VertexEntry = Vertices[i].position;
			ControlPoints[i] = FbxVector4{ VertexEntry.x, VertexEntry.y, VertexEntry.z };
		}
	}

	// Construct normal information
	if (Lod.HasVertexFlag(Normals))
	{
		FbxLayerElementNormal* LayerElementNormal = FbxLayerElementNormal::Create(Mesh, "");
		LayerElementNormal->SetMappingMode(FbxLayerElement::eByControlPoint);
		LayerElementNormal->SetReferenceMode(FbxLayerElement::eDirect);

		FbxLayerElementArrayTemplate<FbxVector4>& DirectArray = LayerElementNormal->GetDirectArray();
		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const Point3& VertexEntry = Vertices[i].normals;
			DirectArray.Add({ VertexEntry.x, VertexEntry.y, VertexEntry.z });
		}

		Layer0->SetNormals(LayerElementNormal);
	}

	// Construct tangent information
	if (Lod.HasVertexFlag(Tangent))
	{
		FbxLayerElementTangent* LayerElementTangent = FbxLayerElementTangent::Create(Mesh, "");
		LayerElementTangent->SetMappingMode(FbxLayerElement::eByControlPoint);
		LayerElementTangent->SetReferenceMode(FbxLayerElement::eDirect);

		FbxLayerElementArrayTemplate<FbxVector4>& DirectArray = LayerElementTangent->GetDirectArray();
		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const Point3& VertexEntry = Vertices[i].tangent;
			DirectArray.Add({ VertexEntry.x, VertexEntry.y, VertexEntry.z });
		}

		Layer0->SetTangents(LayerElementTangent);
	}

	// Construct TexCoord0
	FbxGeometryElementUV* DiffuseUV = nullptr;
	FbxGeometryElementMaterial* DiffuseMat = nullptr;
	if (Lod.HasVertexFlag(TexCoords0))
	{
		DiffuseUV = CreateUVElement(Mesh, UVElementType::UV_Diffuse);
		DiffuseMat = CreateMaterialElement(Mesh, "Diffuse Mapping");

		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const UVVert& VertexEntry = Vertices[i].uv0;
			DiffuseUV->GetDirectArray().Add({ VertexEntry.x, VertexEntry.y });
		}
	}

	// Construct TexCoord0
	FbxGeometryElementUV* TexCoord1UV = nullptr;
	FbxGeometryElementMaterial* TexCoord1Mat = nullptr;
	if (Lod.HasVertexFlag(TexCoords1))
	{
		TexCoord1UV = CreateUVElement(Mesh, UVElementType::UV_1);
		TexCoord1Mat = CreateMaterialElement(Mesh, "UV1 Mapping");

		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const UVVert& VertexEntry = Vertices[i].uv1;
			TexCoord1UV->GetDirectArray().Add({ VertexEntry.x, VertexEntry.y });
		}
	}

	// Construct TexCoord2
	FbxGeometryElementUV* TexCoord2UV = nullptr;
	FbxGeometryElementMaterial* TexCoord2Mat = nullptr;
	if (Lod.HasVertexFlag(TexCoords2))
	{
		TexCoord2UV = CreateUVElement(Mesh, UVElementType::UV_2);
		TexCoord2Mat = CreateMaterialElement(Mesh, "UV2 Mapping");

		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const UVVert& VertexEntry = Vertices[i].uv2;
			TexCoord2UV->GetDirectArray().Add({ VertexEntry.x, VertexEntry.y });
		}
	}

	// Construct OMUV
	FbxGeometryElementUV* OmissiveUV = nullptr;
	FbxGeometryElementMaterial* OmmisiveMat = nullptr;
	if (Lod.HasVertexFlag(ShadowTexture))
	{
		OmissiveUV = CreateUVElement(Mesh, UVElementType::UV_Omissive);
		OmmisiveMat = CreateMaterialElement(Mesh, "Omissive Mapping");

		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const UVVert& VertexEntry = Vertices[i].uv3;
			OmissiveUV->GetDirectArray().Add({ VertexEntry.x, VertexEntry.y });
		}
	}

	// Setup Indices of object
	const std::vector<MT_FaceGroup>& FaceGroups = Lod.GetFaceGroups();
	const std::vector<Int3>& Indices = Lod.GetIndices();
	for (size_t i = 0; i < FaceGroups.size(); i++)
	{
		const MT_FaceGroup& FaceGroup = FaceGroups[i];
		FbxSurfacePhong* FaceGroupMaterial = CreateMaterial(FaceGroup.GetMaterialInstance());
		LodNode->AddMaterial(FaceGroupMaterial);

		const size_t StartIndex = FaceGroup.GetStartIndex() / 3;
		const size_t NumFaces = FaceGroup.GetNumFaces();

		for (size_t x = StartIndex; x < StartIndex + NumFaces; x++)
		{
			const Int3& Tri = Indices[x];
			Mesh->BeginPolygon(i);
			Mesh->AddPolygon(Tri.i1);
			Mesh->AddPolygon(Tri.i2);
			Mesh->AddPolygon(Tri.i3);
			Mesh->EndPolygon();

			// Set DiffuseUV Mapping
			if (Lod.HasVertexFlag(TexCoords0))
			{
				FBX_ASSERT(DiffuseUV);//
				DiffuseUV->GetIndexArray().Add(i);
			}

			// Set UV1 Mapping
			if (Lod.HasVertexFlag(TexCoords1))
			{
				FBX_ASSERT(TexCoord1UV);
				TexCoord1UV->GetIndexArray().Add(0);
			}

			// Set UV2 Mapping
			if (Lod.HasVertexFlag(TexCoords2))
			{
				FBX_ASSERT(TexCoord2UV);
				TexCoord2UV->GetIndexArray().Add(0);
			}

			// Set Omissive Mapping
			if (Lod.HasVertexFlag(ShadowTexture))
			{
				FBX_ASSERT(OmissiveUV);
				OmissiveUV->GetIndexArray().Add(0);
			}
		}
	}

	return true;
}

bool Fbx_Wrangler::ConvertCollisionToNode(const MT_Collision& Collision, FbxNode* CollisionNode)
{
	// Create new FbxNodes then set attributes
	FbxMesh* Mesh = FbxMesh::Create(SdkManager, "CollisionMesh");
	CollisionNode->SetNodeAttribute(Mesh);
	CollisionNode->SetShadingMode(FbxNode::eHardShading);

	// Create MainLayer
	FbxLayer* Layer0 = Mesh->GetLayer(0);
	if (!Layer0)
	{
		Mesh->CreateLayer();
		Layer0 = Mesh->GetLayer(0);
	}

	// Setup Initial Control Point Array
	const std::vector<Point3>& Vertices = Collision.GetVertices();
	Mesh->InitControlPoints(Vertices.size());
	if (FbxVector4* ControlPoints = Mesh->GetControlPoints())
	{
		for (size_t i = 0; i < Vertices.size(); i++)
		{
			const Point3& VertexEntry = Vertices[i];
			ControlPoints[i] = FbxVector4{ VertexEntry.x, VertexEntry.y, VertexEntry.z };
		}
	}

	// Construct Main UV (although empty)
	FbxGeometryElementUV* CollisionUV = CreateUVElement(Mesh, "CollisionUV");
	CollisionUV->GetDirectArray().Resize(Vertices.size());

	// Setup Triangle data
	const std::vector<MT_FaceGroup>& FaceGroups = Collision.GetFaceGroups();
	const std::vector<Int3>& Indices = Collision.GetIndices();

	for (size_t i = 0; i < FaceGroups.size(); i++)
	{
		const MT_FaceGroup& FaceGroup = FaceGroups[i];
		FbxSurfacePhong* FaceGroupMaterial = CreateMaterial(FaceGroup.GetMaterialInstance());
		CollisionNode->AddMaterial(FaceGroupMaterial);

		const size_t StartIndex = FaceGroup.GetStartIndex() / 3;
		const size_t NumFaces = FaceGroup.GetNumFaces();

		for (size_t x = StartIndex; x < StartIndex + NumFaces; x++)
		{
			const Int3& Tri = Indices[x];
			Mesh->BeginPolygon(i);
			Mesh->AddPolygon(Tri.i1);
			Mesh->AddPolygon(Tri.i2);
			Mesh->AddPolygon(Tri.i3);
			Mesh->EndPolygon();

			// Set CollisionUV Mapping
			FBX_ASSERT(CollisionUV);//
			CollisionUV->GetIndexArray().Add(i);
		}
	}

	return true;
}

bool Fbx_Wrangler::ConvertSkeletonToNode(const MT_Skeleton& Skeleton, FbxSkin* Skin, FbxNode* BoneRoot, const int LODIndex)
{
	std::vector<FbxNode*> BoneNodes = {};
	std::vector<FbxCluster*> ClusterNodes = {};

	const std::vector<MT_Joint>& Joints = Skeleton.GetJoints();
	for (size_t i = 0; i < Joints.size(); i++)
	{
		const MT_Joint& JointObject = Joints[i];
		const std::string& Name = JointObject.GetName();
		FbxString SkeletonName = Name.data();
		FbxString ClusterName = Name.data();

		// Append names
		SkeletonName.Append("_Skeleton", 10);
		ClusterName.Append("_Cluster", 8);

		// Construct Transform 
		const JointMatrix& Transform = JointObject.GetTransform();

		FbxQuaternion quaterion = FbxQuaternion(Transform.Rotation.x, Transform.Rotation.y, Transform.Rotation.z, Transform.Rotation.w);
		FbxVector4 euler;
		euler.SetXYZ(quaterion);

		FbxAMatrix lTransformMatrix;
		lTransformMatrix.SetIdentity();

		lTransformMatrix.SetT(FbxVector4(Transform.Position.x, Transform.Position.y, Transform.Position.z));
		lTransformMatrix.SetQ(quaterion);
		lTransformMatrix.SetS(FbxVector4(Transform.Scale.x, Transform.Scale.y, Transform.Scale.z));

		// Construct Node to contain Skeleton and Link for Cluster
		// (Then Construct transform)
		FbxNode* JointNode = FbxNode::Create(SdkManager, Name.data());
		JointNode->LclTranslation.Set(lTransformMatrix.GetT());
		JointNode->LclRotation.Set(lTransformMatrix.GetR());
		JointNode->LclScaling.Set(lTransformMatrix.GetS());

		// Setup Parenting
		const int ParentIndex = JointObject.GetParentJointIndex();
		if (ParentIndex != 0xFF)
		{
			BoneNodes[ParentIndex]->AddChild(JointNode);
		}
		else
		{
			BoneRoot = JointNode;
		}

		// Create FbxSkeleton Joint
		FbxSkeleton* JointFbx = FbxSkeleton::Create(SdkManager, SkeletonName);
		JointFbx->SetSkeletonType(FbxSkeleton::EType::eLimbNode);
		JointNode->SetNodeAttribute(JointFbx);

		// Create FbxCluster
		FbxCluster* ClusterFbx = FbxCluster::Create(SdkManager, ClusterName);
		ClusterFbx->SetLinkMode(FbxCluster::eTotalOne);
		ClusterFbx->SetLink(JointNode);
		ClusterFbx->SetTransformLinkMatrix(JointNode->EvaluateGlobalTransform());
		Skin->AddCluster(ClusterFbx);

		BoneNodes.push_back(JointNode);
	}

	return true;
}

FbxGeometryElementUV* Fbx_Wrangler::CreateUVElement(FbxMesh* Mesh, const UVElementType Type)
{
	const std::string& UVName = GetNameByUVType(Type);
	return CreateUVElement(Mesh, UVName.data());
}

FbxGeometryElementUV* Fbx_Wrangler::CreateUVElement(FbxMesh* Mesh, const char* Name)
{
	FbxGeometryElementUV* UVElement = Mesh->CreateElementUV(Name);
	FBX_ASSERT(UVElement);

	UVElement->SetMappingMode(FbxGeometryElement::eByControlPoint);
	UVElement->SetReferenceMode(FbxGeometryElement::eDirect);
	return UVElement;
}

FbxGeometryElementMaterial* Fbx_Wrangler::CreateMaterialElement(FbxMesh* pMesh, const char* pName)
{
	FbxGeometryElementMaterial* MatElement = pMesh->CreateElementMaterial();
	FBX_ASSERT(MatElement);

	MatElement->SetName(pName);
	MatElement->SetMappingMode(FbxLayerElement::eByPolygon);
	MatElement->SetReferenceMode(FbxLayerElement::eIndexToDirect);
	return MatElement;
}

FbxSurfacePhong* Fbx_Wrangler::CreateMaterial(const MT_MaterialInstance& MaterialInstance)
{
	const FbxString& MaterialName = MaterialInstance.GetName().data();
	const FbxString& ShadingName = "Phong";

	FbxSurfacePhong* NewMaterial = FbxSurfacePhong::Create(SdkManager, MaterialName.Buffer());
	NewMaterial->ShadingModel.Set(ShadingName);

	if (MaterialInstance.HasMaterialFlag(HasDiffuse))
	{
		NewMaterial->Diffuse.ConnectSrcObject(CreateTexture(MaterialInstance.GetTextureName()));
	}

	return NewMaterial;

}

FbxTexture* Fbx_Wrangler::CreateTexture(const std::string& Name)
{
	FbxFileTexture* NewTexture = FbxFileTexture::Create(SdkManager, Name.data());
	const FbxString& Path = FbxGetApplicationDirectory();
	const FbxString& TextureString = Name.data();

	// Set texture properties.
	NewTexture->SetFileName(TextureString.Buffer());
	NewTexture->SetName(Name.data());
	NewTexture->SetTextureUse(FbxTexture::eStandard);
	NewTexture->SetMappingType(FbxTexture::eUV);
	NewTexture->SetMaterialUse(FbxFileTexture::eModelMaterial);
	NewTexture->SetSwapUV(false);
	NewTexture->SetAlphaSource(FbxTexture::eNone);
	NewTexture->SetTranslation(0.0, 0.0);
	NewTexture->SetScale(1.0, 1.0);
	NewTexture->SetRotation(0.0, 0.0);

	return NewTexture;
}

const std::string& Fbx_Wrangler::GetNameByUVType(const UVElementType Type)
{
	switch (Type)
	{
	case UVElementType::UV_Diffuse:
	{
		return WranglerUtils::DiffuseUVName;
	}
	case UVElementType::UV_1:
	{
		return WranglerUtils::UV0Name;
	}
	case UVElementType::UV_2:
	{
		return WranglerUtils::UV1Name;
	}
	case UVElementType::UV_Omissive:
	{
		return WranglerUtils::OMUVName;
	}
	default:
	{
		// unhandled type
		return WranglerUtils::Empty;
	}
	}

	return WranglerUtils::Empty;
}

bool Fbx_Wrangler::SaveDocument()
{
	FbxExporter* Exporter = FbxExporter::Create(SdkManager, "");
	
	// Set the export states. By default, the export states are always set to 
	// true except for the option eEXPORT_TEXTURE_AS_EMBEDDED. The code below 
	// shows how to change these states.
	IOS_REF.SetBoolProp(EXP_FBX_MATERIAL, true);
	IOS_REF.SetBoolProp(EXP_FBX_TEXTURE, true);
	IOS_REF.SetBoolProp(EXP_FBX_EMBEDDED, false);
	IOS_REF.SetBoolProp(EXP_FBX_ANIMATION, false);
	IOS_REF.SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);

	// Convert scene to CM
	FbxSystemUnit::cm.ConvertScene(Scene, Scene->GetRootNode());

	Exporter->Initialize(FbxName, 0, SdkManager->GetIOSettings());
	Exporter->Export(Scene);
	Exporter->Destroy();
	return true;
}
