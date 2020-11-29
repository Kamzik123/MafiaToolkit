#include "MT_Object.h"

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
