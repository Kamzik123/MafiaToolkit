#pragma once

#include "MT_Lod.h"

#include <string>
#include <vector>

class MT_Collision;
class MT_ObjectHandler;
class MT_Skeleton;
struct TransformStruct;

enum MT_ObjectFlags : uint
{
	HasLODs = 1,
	HasSkinning = 2,
	HasCollisions = 4,
};

struct TransformStruct
{
	Point3 Position;
	Point3 Rotation;
	Point3 Scale;
};

class MT_Object
{

	friend MT_ObjectHandler;

public:

	bool HasObjectFlag(const MT_ObjectFlags FlagToCheck) const;
	void Cleanup();

	// Accessors
	const std::string& GetName() const { return ObjectName; }
	const MT_ObjectFlags& GetFlags() const { return ObjectFlags; }
	const std::vector<MT_Lod> GetLods() const { return LodObjects; }
	const TransformStruct& GetTransform() const { return Transform; }
	const MT_Collision* GetCollision() const { return CollisionObject; }
	const MT_Skeleton* GetSkeleton() const { return SkeletonObject; }

	// Setters
	void SetName(std::string& InName) { ObjectName = InName; }
	void SetLods(std::vector<MT_Lod> InLods) { LodObjects = InLods; }
	void SetObjectFlags(MT_ObjectFlags InFlags) { ObjectFlags = InFlags; }
	void SetTransform(TransformStruct& InTransform) { Transform = InTransform; }
	void SetCollisions(MT_Collision* InCollision) 
	{ 
		CollisionObject = InCollision; 
		int Temp = ObjectFlags;
		Temp |= (InCollision ? MT_ObjectFlags::HasCollisions : 0);
		ObjectFlags = (MT_ObjectFlags)Temp;
	}
	void SetSkeleton(MT_Skeleton* InSkeleton)
	{
		SkeletonObject = InSkeleton;
		int Temp = ObjectFlags;
		Temp |= (InSkeleton ? MT_ObjectFlags::HasSkinning : 0);
		ObjectFlags = (MT_ObjectFlags)Temp;
	}

	// IO
	bool ReadFromFile(FILE* InStream);
	void WriteToFile(FILE* OutStream) const;

private:

	bool ValidateHeader(const int Magic) const;

	std::string ObjectName = "";
	MT_ObjectFlags ObjectFlags;
	TransformStruct Transform;

	std::vector<MT_Lod> LodObjects;
	MT_Collision* CollisionObject = nullptr;
	MT_Skeleton* SkeletonObject = nullptr;
};

class MT_ObjectBundle
{

	friend MT_ObjectHandler;

public:

	void Cleanup();

	// Accessor
	const std::vector<MT_Object>& GetObjects() const { return Objects; }

	// Setter
	void SetObjects(std::vector<MT_Object> InObjects) { Objects = InObjects; }

private:

	std::vector<MT_Object> Objects;
};
