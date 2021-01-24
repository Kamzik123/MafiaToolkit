#include "MT_Object.h"

#include "Source/MTObject/MT_Collision.h"
#include "Source/Utilities/FileUtils.h"

bool MT_Object::HasObjectFlag(const MT_ObjectFlags FlagToCheck) const
{
	return (ObjectFlags & FlagToCheck);
}

void MT_Object::Cleanup()
{
	ObjectName = "";

	// Empty LodObjects
	for (auto& LodObject : LodObjects)
	{
		LodObject.Cleanup();
	}

	LodObjects.clear();

	// Cleanup Collision
	if (CollisionObject)
	{
		CollisionObject->Cleanup();
		CollisionObject = nullptr;
	}
}

bool MT_Object::ReadFromFile(FILE* InStream)
{
	// Try Validate Header
	uint Magic = 0;
	FileUtils::Read(InStream, &Magic);
	if (!ValidateHeader(Magic))
	{
		return false;
	}

	// Read Name and Flags
	FileUtils::ReadString(InStream, &ObjectName);
	FileUtils::Read(InStream, &ObjectFlags);

	// Read LODs
	if (HasObjectFlag(MT_ObjectFlags::HasLODs))
	{
		// Read LODS Size
		uint NumLODs = 0;
		FileUtils::Read(InStream, &NumLODs);
		LodObjects.resize(NumLODs);

		for (uint i = 0; i < NumLODs; i++)
		{
			// Begin to read LOD
			MT_Lod LodObject = {};
			LodObject.ReadFromFile(InStream);
			LodObjects[i] = LodObject;
		}
	}

	return true;
}

void MT_Object::WriteToFile(FILE* OutStream) const
{
	// Write magic
	FileUtils::Write(OutStream, (uint)55530573);

	// Begin to Write
	FileUtils::WriteString(OutStream, ObjectName);
	FileUtils::Write(OutStream, ObjectFlags);

	// Write LODs
	if (HasObjectFlag(MT_ObjectFlags::HasLODs))
	{
		FileUtils::Write(OutStream, (uint)LodObjects.size());

		for (int i = 0; i < LodObjects.size(); i++)
		{
			const MT_Lod& LodInfo = LodObjects[i];
			LodInfo.WriteToFile(OutStream);
		}
	}
}

bool MT_Object::ValidateHeader(const int Magic) const
{
	// MTO, version 3
	if (Magic == 55530573)
	{
		return true;
	}

	return false;
}

void MT_ObjectBundle::Cleanup()
{
	for (auto& Object : Objects)
	{
		Object.Cleanup();
	}
}
