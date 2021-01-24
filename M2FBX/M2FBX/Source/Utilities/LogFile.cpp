#include "LogFile.h"

#include <cstdarg>

namespace LogFileUtils
{
	const std::string InfoString = "[INFO]";
	const std::string WarningString = "[WARNING]";
	const std::string ErrorString = "[ERROR]";
}

std::ofstream mFile;

void LogFile::Construct(const std::string& name)
{
	if (IsOpen())
	{
		mFile.open(name.data(), ios::out | ios::trunc);
	}
}

void LogFile::Destroy()
{
	if (IsOpen())
	{
		mFile.close();
	}
}

void LogFile::Printf(const ELogError Error, const char* Text, ...)
{
	if (IsOpen()) 
	{
		char buffer[4096]{ NULL };

		va_list va;
		va_start(va, Text);
		vsprintf_s(buffer, Text, va);
		va_end(va);
		WriteLine(Error, buffer);
	}
}

void LogFile::WriteLine(const ELogError Error, const char* Text)
{
	if (IsOpen())
	{
		Write(Error, Text);
		Append();
	}
}

bool LogFile::IsOpen()
{
	return mFile.is_open();
}

void LogFile::Append()
{
	if (IsOpen())
	{
		mFile << std::endl;
	}
}

void LogFile::Write(const ELogError Error, const char* Text)
{
	// TODO: We can probably separate this to another function
	std::string ErrorType = "";
	switch (Error)
	{
	case ELogError::eError:
	{
		ErrorType = LogFileUtils::ErrorString;
	}
	case ELogError::eInfo:
	{
		ErrorType = LogFileUtils::InfoString;
	}
	case ELogError::eWarning:
	{
		ErrorType = LogFileUtils::WarningString;
	}
	default:
	{
		break;
	}
	}

	if (IsOpen())
	{
		mFile << ErrorType << " ";
		mFile << Text;
	}
}
