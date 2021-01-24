#include <fbxsdk.h>
#include "FbxWrangler.h"
#include "M2TWrangler.h"
#include "M2Model.h"
#include "Source/MTObject/MT_Object.h"
#include "Source/MTObject/MT_ObjectHandler.h"
#include "Source/Fbx_Wrangler.h"
#include "Source/MT_Wrangler.h"
#include <conio.h>

extern "C" int  __declspec(dllexport) _stdcall RunConvertFBX(const char* source, const char* dest);
extern "C" int  __declspec(dllexport) _stdcall RunConvertM2T(const char* source, const char* dest, unsigned char isBin);
extern "C" int  __declspec(dllexport) _stdcall RunConvertType(const char* source, const char* dest);

extern int _stdcall RunConvertFBX(const char* source, const char* dest)
{
	WriteLine("Called RunConvertFBX");
	return ConvertFBX(source, dest);
}
extern int _stdcall RunConvertM2T(const char* source, const char* dest, unsigned char isBin)
{
	WriteLine("Called RunConvertM2T");
	return ConvertM2T(source, dest, isBin);
}
int _stdcall RunConvertType(const char* source, const char* dest)
{
	WriteLine("Called RunConvertType");
	return ConvertType(source, dest);
}

int main(int argc, char** argv)
{
	int result = 0;

	if ((strcmp(argv[1], "-ConvertM2T") == 0) && (argc >=4))
	{
		result = ConvertM2T(argv[2], argv[3], 0);
	}
	else if ((strcmp(argv[1], "-ConvertFBX") == 0) && (argc >= 4))
	{
		MT_Wrangler* Wrangler = new MT_Wrangler(argv[2], argv[3]);
		Wrangler->ConstructMTBFromFbx();
		Wrangler->SaveBundleToFile();
	}
	else if ((strcmp(argv[1], "-ConvertType") == 0) && (argc >= 4))
	{
		result = ConvertType(argv[2], argv[3]);
	}
	else if ((strcmp(argv[1], "-ConvertMTB") == 0) && (argc >= 4))
	{
		MT_ObjectBundle* ObjectBundle = MT_ObjectHandler::ReadBundleFromFile(argv[2]);
		if (ObjectBundle)
		{
			Fbx_Wrangler* Wrangler = new Fbx_Wrangler(argv[2], argv[3]);
			Wrangler->ConvertBundleToFbx();

			ObjectBundle->Cleanup();
			ObjectBundle = nullptr;
		}

		return 0;
	}
	else if ((strcmp(argv[1], "-ConvertMTO") == 0) && (argc >= 4))
	{
		Fbx_Wrangler* Wrangler = new Fbx_Wrangler(argv[2], argv[3]);
		Wrangler->ConvertObjectToFbx();
	}
	else
	{
		printf("M2FBX Initiated succesfully.");
	}
	return result;
}