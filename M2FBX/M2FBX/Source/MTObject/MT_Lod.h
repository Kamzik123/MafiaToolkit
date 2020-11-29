#pragma once

#include "DataTypes.h"

#include <string>

typedef unsigned int uint;
typedef unsigned short ushort;

class MT_ObjectHandler;

enum MT_MaterialInstanceFlags
{
	IsCollision = 1,
	HasDiffuse = 2
};

class MT_MaterialInstance
{
	friend MT_ObjectHandler;

public:

	void Cleanup();

private:

	MT_MaterialInstanceFlags MaterialFlags;
	std::string Name = "";
	std::string DiffuseTexture = "";

};

class MT_FaceGroup
{

	friend MT_ObjectHandler;

public:

	void Cleanup();

private:

	uint StartIndex = 0;
	uint NumFaces = 0;
	MT_MaterialInstance* MaterialInstance = nullptr;
	
};

class MT_Lod
{
	friend MT_ObjectHandler;

public:

	bool HasVertexFlag(const VertexFlags Flag) const;
	void Cleanup();

private:

	VertexFlags VertexDeclaration;
	std::vector<Vertex> Vertices;
	std::vector<Int3> Indices;
	std::vector<MT_FaceGroup> FaceGroups;
};

class MT_Collision
{
public:

	void Cleanup();

private:

	std::vector<Point3> Vertices;
	std::vector<uint> Indices;
	std::vector<ushort> MaterialAssignments;
};
