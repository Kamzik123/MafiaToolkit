#include "Common.h"

typedef unsigned short ushort;

class MT_Collision
{
public:

	void Cleanup();

	// Getters
	const std::vector<Point3>& GetVertices() const { return Vertices; }
	const std::vector<Int3>& GetIndices() const { return Indices; }
	const std::vector<ushort>& GetMatAssignments() const { return MaterialAssignments; }

	// Setters 
	void SetVertices(const std::vector<Point3>& InVertices) { Vertices = InVertices; }
	void SetIndices(const std::vector<Int3>& InTriangles) { Indices = InTriangles; }
	void SetMaterialAssignments(const std::vector<ushort>& InAssignments) { MaterialAssignments = InAssignments; }

	// IO
	void ReadFromFile(FILE* InStream) {}
	void WriteToFile(FILE* OutStream) const {}

private:

	std::vector<Point3> Vertices;
	std::vector<Int3> Indices;
	std::vector<ushort> MaterialAssignments;
};
