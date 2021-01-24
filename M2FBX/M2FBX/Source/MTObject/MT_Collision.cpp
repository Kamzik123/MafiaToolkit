#include "MT_Collision.h"

void MT_Collision::Cleanup()
{
	// Cleanup other vectors
	MaterialAssignments.clear();
	Vertices.clear();
	Indices.clear();
}
