#pragma once

#include <fstream>
#include <iostream>

using namespace std;

enum class ELogError
{
	eInfo,
	eWarning,
	eError
};

class LogFile
{
public:

	static void Construct(const std::string& name);
	static void Destroy();

	static void Printf(const ELogError Error, const char* Text, ...);
	static void WriteLine(const ELogError Error, const char* Text);

private:

	static bool IsOpen();

	static void Append();
	static void Write(const ELogError Error, const char* Text);
};
