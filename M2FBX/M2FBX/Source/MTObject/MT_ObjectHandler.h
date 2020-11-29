#pragma once

#include <string>

class MT_Object;

class MT_ObjectHandler
{
public:

	static MT_Object* ReadObjectFromFile(const std::string& FileName);

	static void WriteObjectToFile(const std::string& FileName, const MT_Object& Object);

private:

	static void ReadLODFromFile(FILE* InStream, MT_Object* NewObject);

	static void WriteLODToFile(FILE* OutStream, const MT_Object& Object);
};

