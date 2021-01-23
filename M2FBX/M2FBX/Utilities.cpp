#include "Utilities.h"

#include <algorithm>
#include <fbxsdk.h>

void WriteLine(const char *format, ...)
{
	char buf[255];
	va_list va;
	va_start(va, format);
	_vsnprintf_s(buf, sizeof(buf), format, va);
	va_end(va);

	FBXSDK_printf(buf);
	FBXSDK_printf("\n");
}