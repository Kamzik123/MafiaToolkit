#pragma once

#include "MT_Lod.h"

#include <string>
#include <vector>

class MT_ObjectHandler;

enum MT_ObjectFlags : uint
{
	HasLODs = 1,
	HasSkinning = 2,
	HasCollisions = 4
};

class MT_Object
{

	friend MT_ObjectHandler;

public:

	bool HasObjectFlag(const MT_ObjectFlags FlagToCheck) const;

	void Cleanup();

private:

	std::string ObjectName = "";
	MT_ObjectFlags ObjectFlags;

	std::vector<MT_Lod> LodObjects;
	MT_Collision* CollisionObject = nullptr;


};

