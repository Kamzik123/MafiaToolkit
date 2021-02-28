#include "MT_ObjectUtils.h"

#include "fbxsdk.h"

namespace MT_ObjectUtils_Consts
{
	const char* ConstMesh = "MESH";
	const char* ConstRigged = "RIGD";
	const char* ConstItemDesc = "ITEM";
	const char* ConstActor = "ACTR";
	const char* ConstDummy = "DUMY";
	const char* ConstJoint = "JNT";
	const char* ConstNull = "NULL";
}

void MT_ObjectUtils::RemoveMetaTagFromName(std::string& ObjectName)
{
	size_t Offset = ObjectName.find('[');
	if (Offset != std::string::npos)
	{
		size_t EndOffset = Offset + 4;
		if (ObjectName[EndOffset] == ']')
		{
			ObjectName.erase(ObjectName.begin() + Offset);
		}
	}

	// TODO: Assert or Log here
}

MT_ObjectType MT_ObjectUtils::GetTypeFromString(const FbxString ObjectName)
{
	if (ObjectName.Find(MT_ObjectUtils_Consts::ConstMesh))
	{
		return MT_ObjectType::StaticMesh;
	}
	else if (ObjectName.Find(MT_ObjectUtils_Consts::ConstRigged))
	{
		return MT_ObjectType::RiggedMesh;
	}
	else if (ObjectName.Find(MT_ObjectUtils_Consts::ConstItemDesc))
	{
		return MT_ObjectType::ItemDesc;
	}
	else if (ObjectName.Find(MT_ObjectUtils_Consts::ConstDummy))
	{
		return MT_ObjectType::Dummy;
	}
	else if (ObjectName.Find(MT_ObjectUtils_Consts::ConstActor))
	{
		return MT_ObjectType::Actor;
	}
	else if (ObjectName.Find(MT_ObjectUtils_Consts::ConstJoint))
	{
		return MT_ObjectType::Joint;
	}
	else if (ObjectName.Find(MT_ObjectUtils_Consts::ConstNull))
	{
		return MT_ObjectType::Null;
	}
	else
	{
		return MT_ObjectType::Null;
	}
}

void MT_ObjectUtils::GetTypeAsStringClosed(const MT_ObjectType ObjectType, std::string& TypeEnclosed)
{
	TypeEnclosed += "[";
	TypeEnclosed += GetTypeAsString(ObjectType);
	TypeEnclosed += "]";
}

const char* MT_ObjectUtils::GetTypeAsString(const MT_ObjectType ObjectType)
{
	switch (ObjectType)
	{
	case StaticMesh:
	{
		return MT_ObjectUtils_Consts::ConstMesh;
	}
	case RiggedMesh:
	{
		return MT_ObjectUtils_Consts::ConstRigged;
	}
	case ItemDesc:
	{
		return MT_ObjectUtils_Consts::ConstItemDesc;
	}
	case Actor:
	{
		return MT_ObjectUtils_Consts::ConstActor;
	}
	case Dummy:
	{
		return MT_ObjectUtils_Consts::ConstDummy;
	}
	case Joint:
	{
		return MT_ObjectUtils_Consts::ConstJoint;
	}
	}

	return MT_ObjectUtils_Consts::ConstNull;
}