#include "M2EDM.h"
#include "M2Helpers.h"


//===================================================
//		EDMPART
//===================================================
void EDMPart::SetName(const wchar_t * name) {
	EDMPart::name = name;
}

void EDMPart::SetVertSize(int count) {
	EDMPart::vertSize = count;
}

void EDMPart::SetVertices(std::vector<Point3> vertices) {
	EDMPart::vertices = vertices;
}

void EDMPart::SetUVSize(int count) {
	EDMPart::uvSize = count;
}

void EDMPart::SetUVs(std::vector<UVVert> uvs) {
	EDMPart::uvs = uvs;
}

void EDMPart::SetIndicesSize(int count) {
	EDMPart::indicesSize = count;
}

void EDMPart::SetIndices(std::vector<Int3> indices) {
	EDMPart::indices = indices;
}

const wchar_t* EDMPart::GetName() {
	return EDMPart::name;
}

int EDMPart::GetVertSize() {
	return EDMPart::vertSize;
}

std::vector<Point3> EDMPart::GetVertices() {
	return EDMPart::vertices;
}

int EDMPart::GetUVSize() {
	return EDMPart::uvSize;
}

std::vector<UVVert> EDMPart::GetUVs() {
	return EDMPart::uvs;
}

int EDMPart::GetIndicesSize() {
	return EDMPart::indicesSize;
}

std::vector<Int3> EDMPart::GetIndices() {
	return EDMPart::indices;
}

void EDMPart::ReadFromStream(FILE * stream) {
	byte numBytes;
	std::wstring wname;

	fread(&numBytes, sizeof(byte), 1, stream);

	for (int i = 0; i != numBytes; i++) {
		char nChar;
		fread(&nChar, sizeof(char), 1, stream);
		wname += nChar;
	}

	name = wname.c_str();

	fread(&vertSize, sizeof(int), 1, stream);
	vertices = std::vector<Point3>(vertSize);
	for (int i = 0; i != vertSize; i++) {
		fread(&vertices[i].x, sizeof(float), 1, stream);
		fread(&vertices[i].y, sizeof(float), 1, stream);
		fread(&vertices[i].z, sizeof(float), 1, stream);
	}

	fread(&uvSize, sizeof(int), 1, stream);
	uvs = std::vector<UVVert>(uvSize);
	for (int i = 0; i != uvSize; i++) {
		fread(&uvs[i].x, sizeof(float), 1, stream);
		fread(&uvs[i].y, sizeof(float), 1, stream);
	}

	fread(&indicesSize, sizeof(int), 1, stream);
	indices = std::vector<Int3>(indicesSize);
	for (int i = 0; i != indicesSize; i++) {
		fread(&indices[i].i1, sizeof(int), 1, stream);
		fread(&indices[i].i2, sizeof(int), 1, stream);
		fread(&indices[i].i3, sizeof(int), 1, stream);

		indices[i].i1 -= 1;
		indices[i].i2 -= 1;
		indices[i].i3 -= 1;
	}
}

EDMPart::EDMPart() {}
EDMPart::~EDMPart() {}

//===================================================
//		EDMSTRUCTURE
//===================================================
void EDMStructure::SetName(const wchar_t * name) {
	EDMStructure::name = name;
}

void EDMStructure::SetPartSize(int count) {
	EDMStructure::partSize = count;
}

void EDMStructure::SetParts(std::vector<EDMPart> parts) {
	EDMStructure::parts = parts;
}

const wchar_t * EDMStructure::GetName() {
	return name;
}

int EDMStructure::GetPartSize() {
	return partSize;
}

std::vector<EDMPart> EDMStructure::GetParts() {
	return parts;
}

void EDMStructure::ReadFromStream(FILE * stream) {

	byte numBytes;
	std::wstring wname;

	fread(&numBytes, sizeof(byte), 1, stream);

	for (int i = 0; i != numBytes; i++) {
		char nChar;
		fread(&nChar, sizeof(char), 1, stream);
		wname += nChar;
	}

	name = wname.c_str();


	fread(&partSize, sizeof(int), 1, stream);
	parts = std::vector<EDMPart>(partSize);
	
	for (int i = 0; i != parts.size(); i++) {
		parts[i].ReadFromStream(stream);
	}
}

EDMStructure::EDMStructure() {}
EDMStructure::~EDMStructure() {}
