#include "MT_ObjectHandler.h"

#include "MT_Object.h"
#include "Common.h"

#include <cstdio>

template <class TType>
size_t Read(FILE* InStream, TType* Buffer)
{
	return fread(Buffer, sizeof(TType), 1, InStream);
}

template <class TType>
size_t Write(FILE* OutStream, TType Buffer)
{
	return fwrite(&Buffer, sizeof(TType), 1, OutStream);
}

MT_Object* MT_ObjectHandler::ReadObjectFromFile(const std::string& FileName)
{
	FILE* InStream = nullptr;
	fopen_s(&InStream, FileName.data(), "rb");

	if (!InStream)
	{
		// Failed
		return nullptr;
	}

	// Try Validate Header
	uint Magic = 0;
	fread(&Magic, sizeof(uint), 1, InStream);
	if (Magic != 55530573)
	{
		// TODO: Probably send to MT_Object to validate?
		// Invalid
		return nullptr;
	}

	// Begin reading the file
	MT_Object* NewObject = new MT_Object();
	NewObject->ObjectName = ReadString(InStream, NewObject->ObjectName);
	fread(&NewObject->ObjectFlags, sizeof(uint), 1, InStream);

	// Read LODs
	if (NewObject->HasObjectFlag(MT_ObjectFlags::HasLODs))
	{
		ReadLODFromFile(InStream, NewObject);
	}

	fclose(InStream);

	return NewObject;
}

void MT_ObjectHandler::WriteObjectToFile(const std::string& FileName, const MT_Object& Object)
{
	FILE* OutStream = nullptr;
	fopen_s(&OutStream, FileName.data(), "wb");

	if (!OutStream)
	{
		// Failed
		return;
	}

	// Write magic
	Write(OutStream, (uint)55530573);

	// Begin to Write
	WriteString(OutStream, Object.ObjectName);
	Write(OutStream, Object.ObjectFlags);

	// Write LODs
	if (Object.HasObjectFlag(MT_ObjectFlags::HasLODs))
	{
		WriteLODToFile(OutStream, Object);
	}
}

void MT_ObjectHandler::ReadLODFromFile(FILE* InStream, MT_Object* NewObject)
{
	// Read LODS Size
	uint NumLODs = 0;
	Read(InStream, &NumLODs);
	NewObject->LodObjects.resize(NumLODs);

	// Begin to read LOD
	MT_Lod* LodObject = new MT_Lod();
	Read(InStream, &LodObject->VertexDeclaration);

	// Read the Vertices
	uint NumVertices = 0;
	Read(InStream, &NumVertices);
	LodObject->Vertices.resize(NumVertices);

	for (uint i = 0; i < NumVertices; i++)
	{
		Vertex NewVertex = Vertex();
		if (LodObject->HasVertexFlag(VertexFlags::Position))
		{
			Read(InStream, &NewVertex.position);
		}
		if (LodObject->HasVertexFlag(VertexFlags::Normals))
		{
			Read(InStream, &NewVertex.normals);
		}
		if (LodObject->HasVertexFlag(VertexFlags::Tangent))
		{
			Read(InStream, &NewVertex.tangent);
		}
		if (LodObject->HasVertexFlag(VertexFlags::Skin))
		{
			Read(InStream, &NewVertex.boneIDs);
			Read(InStream, &NewVertex.boneWeights);
		}
		if (LodObject->HasVertexFlag(VertexFlags::Color))
		{
			Read(InStream, &NewVertex.color0);
		}
		if (LodObject->HasVertexFlag(VertexFlags::Color1))
		{
			Read(InStream, &NewVertex.color1);
		}
		if (LodObject->HasVertexFlag(VertexFlags::TexCoords0))
		{
			Read(InStream, &NewVertex.uv0);
		}
		if (LodObject->HasVertexFlag(VertexFlags::TexCoords1))
		{
			Read(InStream, &NewVertex.uv1);
		}
		if (LodObject->HasVertexFlag(VertexFlags::TexCoords2))
		{
			Read(InStream, &NewVertex.uv2);
		}
		if (LodObject->HasVertexFlag(VertexFlags::ShadowTexture))
		{
			Read(InStream, &NewVertex.uv3);
		}

		LodObject->Vertices[i] = NewVertex;
	}

	// Read the FaceGroups
	uint NumFaceGroups = 0;
	Read(InStream, &NumFaceGroups);
	LodObject->FaceGroups.resize(NumFaceGroups);
	for (uint i = 0; i < NumFaceGroups; i++)
	{
		// Read FaceGroup
		MT_FaceGroup FaceGroup = MT_FaceGroup();
		Read(InStream, &FaceGroup.StartIndex);
		Read(InStream, &FaceGroup.NumFaces);

		// Read MaterialInstance
		MT_MaterialInstance* MaterialInstance = new MT_MaterialInstance();
		Read(InStream, &MaterialInstance->MaterialFlags);
		MaterialInstance->Name = ReadString(InStream, MaterialInstance->Name);
		MaterialInstance->DiffuseTexture = ReadString(InStream, MaterialInstance->DiffuseTexture);
		FaceGroup.MaterialInstance = MaterialInstance;

		LodObject->FaceGroups[i] = FaceGroup;
	}

	// Read the Indices
	uint NumIndices = 0;
	Read(InStream, &NumIndices);
	for (uint i = 0; i < NumIndices/3; i++)
	{
		Int3 Triangle = {};
		Read(InStream, &Triangle);
		LodObject->Indices.push_back(Triangle);
	}

	NewObject->LodObjects[0] = *LodObject;
	fclose(InStream);
}

void MT_ObjectHandler::WriteLODToFile(FILE* OutStream, const MT_Object& Object)
{
	Write(OutStream, (uint)Object.LodObjects.size());

	for (int i = 0; i < Object.LodObjects.size(); i++)
	{
		const MT_Lod& LodInfo = Object.LodObjects[i];

		Write(OutStream, LodInfo.VertexDeclaration);
		Write(OutStream, (uint)LodInfo.Vertices.size());

		for (int x = 0; x < LodInfo.Vertices.size(); x++)
		{
			const Vertex& VertexInfo = LodInfo.Vertices[i];
			if (LodInfo.HasVertexFlag(VertexFlags::Position))
			{
				Write(OutStream, VertexInfo.position);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::Normals))
			{
				Write(OutStream, VertexInfo.normals);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::Tangent))
			{
				Write(OutStream, VertexInfo.tangent);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::Skin))
			{
				Write(OutStream, VertexInfo.boneIDs);
				Write(OutStream, VertexInfo.boneWeights);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::Color))
			{
				Write(OutStream, VertexInfo.color0);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::Color1))
			{
				Write(OutStream, VertexInfo.color1);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::TexCoords0))
			{
				Write(OutStream, VertexInfo.uv0);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::TexCoords1))
			{
				Write(OutStream, VertexInfo.uv1);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::TexCoords2))
			{
				Write(OutStream, VertexInfo.uv2);
			}
			if (LodInfo.HasVertexFlag(VertexFlags::ShadowTexture))
			{
				Write(OutStream, VertexInfo.uv3);
			}
		}

		// Write FaceGroups
		Write(OutStream, (uint)LodInfo.FaceGroups.size());
		for (uint i = 0; i < LodInfo.FaceGroups.size(); i++)
		{
			// Write the FaceGroup
			const MT_FaceGroup& FaceGroupInfo = LodInfo.FaceGroups[i];
			Write(OutStream, FaceGroupInfo.StartIndex);
			Write(OutStream, FaceGroupInfo.NumFaces);

			// Write the MaterialInstance
			const MT_MaterialInstance& MaterialInfo = *FaceGroupInfo.MaterialInstance;
			Write(OutStream, MaterialInfo.MaterialFlags);
			WriteString(OutStream, MaterialInfo.Name);
			WriteString(OutStream, MaterialInfo.DiffuseTexture);
		}

		// Multiply by 3 because indices are stored as Int3's
		Write(OutStream, (uint)LodInfo.Indices.size() * 3); 
		for (uint i = 0; i < (uint)LodInfo.Indices.size(); i++)
		{
			Write(OutStream, LodInfo.Indices[i]);
		}
	}
}
