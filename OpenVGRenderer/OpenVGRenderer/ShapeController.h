#pragma once
#include "Shape.h"
#include <vector>
#include <map>
#include "Structures.h"

#ifdef rpi

#ifndef OpenVGIncludes
#define OpenVGIncludes

extern "C" 
{
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include "VG/openvg.h"
#include "VG/vgu.h"
#include "fontinfo.h"
#include "shapes.h"
}

#endif

#endif

class ShapeController
{
public:
	ShapeController(OVERSCAN overscan);
	~ShapeController(void);
	void Add(Shape* s);
	void Replace(int id, Shape* s);
	void Clear();
	void Remove(int id);
	void Draw();

	void OpenContext();
	void CloseContext();

private:
	std::map<int, Shape*>* shapesById;

	bool dirty;
	int screenWidth, screenHeight;
	OVERSCAN overscan;

};

