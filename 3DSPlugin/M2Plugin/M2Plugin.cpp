//**************************************************************************/
// Copyright (c) 1998-2007 Autodesk, Inc.
// All rights reserved.
// 
// These coded instructions, statements, and computer programs contain
// unpublished proprietary information written by Autodesk, Inc., and are
// protected by Federal copyright law. They may not be disclosed to third
// parties or copied or duplicated in any form, in whole or in part, without
// the prior written consent of Autodesk, Inc.
//**************************************************************************/
// DESCRIPTION: Appwizard generated plugin
// AUTHOR: 
//***************************************************************************/

#include "M2Plugin.h"
#include "M2Helpers.h"
#include "M2EDM.h"
#include "triobj.h"
#include <impapi.h>

#define M2Plugin_CLASS_ID	Class_ID(0xac9aa34b, 0xbb4578d1)

class M2PluginClassDesc : public ClassDesc2 
{
public:
	virtual int IsPublic() 							{ return TRUE; }
	virtual void* Create(BOOL /*loading = FALSE*/) 		{ return new EDMImport(); }
	virtual const TCHAR *	ClassName() 			{ return GetString(IDS_CLASS_NAME); }
	virtual SClass_ID SuperClassID() 				{ return SCENE_IMPORT_CLASS_ID; }
	virtual Class_ID ClassID() 						{ return M2Plugin_CLASS_ID; }
	virtual const TCHAR* Category() 				{ return GetString(IDS_CATEGORY); }

	virtual const TCHAR* InternalName() 			{ return _T("M2Plugin"); }	// returns fixed parsable name (scripter-visible name)
	virtual HINSTANCE HInstance() 					{ return hInstance; }					// returns owning module handle
	

};


ClassDesc2* GetM2PluginDesc() { 
	static M2PluginClassDesc M2PluginDesc;
	return &M2PluginDesc; 
}

INT_PTR CALLBACK M2PluginOptionsDlgProc(HWND hWnd,UINT message,WPARAM ,LPARAM lParam) {
	static EDMImport* imp = nullptr;

	switch(message) {
		case WM_INITDIALOG:
			imp = (EDMImport *)lParam;
			CenterWindow(hWnd,GetParent(hWnd));
			return TRUE;

		case WM_CLOSE:
			EndDialog(hWnd, 0);
			return 1;
	}
	return 0;
}

static FILE *stream = NULL;

//EDM IMPORT SECTION
//=========================

EDMImport::EDMImport() {
}
EDMImport::~EDMImport() {
}
int EDMImport::ExtCount() {
	return 1;
}
const TCHAR* EDMImport::Ext(int n) {
	switch (n) {
	case 0:
		return _T("EDM");
	}
	return _T("");
}
const TCHAR* EDMImport::LongDesc() {
	return GetString(IDS_EDM_L_DESC);
}
const TCHAR* EDMImport::ShortDesc() {
	return GetString(IDS_EDM_S_DESC);
}
const TCHAR* EDMImport::AuthorName() {
	return GetString(IDS_EDM_AUTHOR);
}
const TCHAR* EDMImport::CopyrightMessage() {
	return _T("");
}
const TCHAR* EDMImport::OtherMessage1() {
	return _T("");
}
const TCHAR* EDMImport::OtherMessage2() {
	return _T("");
}
unsigned int EDMImport::Version() {
	return 1;
}
void EDMImport::ShowAbout(HWND hWnd) {}
int EDMImport::DoImport(const TCHAR* filename, ImpInterface* importerInt, Interface* ip, BOOL suppressPrompts)
{
	EDMWorkClass edm(filename, _T("rb"));

	stream = edm.Stream();

	EDMStructure file = EDMStructure();

	file.ReadFromStream(stream);

	std::vector<EDMPart> parts = file.GetParts();

	for (int i = 0; i != parts.size(); i++)
	{
		TriObject* triObject = CreateNewTriObject();
		Mesh &mesh = triObject->GetMesh();

		EDMPart part = parts[i];

		std::vector<Point3> verts = part.GetVertices();
		std::vector<UVVert> uvs = part.GetUVs();
		std::vector<Int3> indices = part.GetIndices();

		mesh.setNumVerts(part.GetVertSize());
		for (int i = 0; i != mesh.numVerts; i++) {
			mesh.setVert(i, verts[i]);
		}

		mesh.setNumMaps(2);
		mesh.setMapSupport(1, true);
		MeshMap &map = mesh.Map(1);
		map.setNumVerts(part.GetUVSize());

		for (int i = 0; i != part.GetUVSize(); i++) {
			map.tv[i].x = uvs[i].x;
			map.tv[i].y = uvs[i].y;
			map.tv[i].z = 0.0f;
		}

		mesh.setNumFaces(part.GetIndicesSize());
		map.setNumFaces(part.GetIndicesSize());

		for (int i = 0; i != mesh.numFaces; i++) {
			mesh.faces[i].setVerts(indices[i].i1, indices[i].i2, indices[i].i3);
			mesh.faces[i].setMatID(1);
			mesh.faces[i].setEdgeVisFlags(1, 1, 1);
			map.tf[i].setTVerts(indices[i].i1, indices[i].i2, indices[i].i3);
		}

		mesh.InvalidateGeomCache();
		mesh.InvalidateTopologyCache();
		ImpNode* node = importerInt->CreateNode();

		if (!node) {
			delete triObject;
			return FALSE;
		}
		node->Reference(triObject);
		node->SetName(_T("NewObject"+i));
		importerInt->AddNodeToScene(node);
	}
	importerInt->RedrawViews();


	//if(!suppressPrompts)
	//	DialogBoxParam(hInstance, 
	//			MAKEINTRESOURCE(IDD_PANEL), 
	//			GetActiveWindow(), 
	//			M2PluginOptionsDlgProc, (LPARAM)this);

	return TRUE;
}
	
