#include "ShapeController.h"
#include <stdlib.h>
#include <iostream>
#include <cstring>

using namespace std;



ShapeController::ShapeController(OVERSCAN overscan)
{
	shapesById = new map<int, Shape*>();
	this->overscan = overscan;

	this->OpenContext();
}

void ShapeController::OpenContext() {
		int sWidth, sHeight;
#ifdef rpi
	init(&sWidth, &sHeight);
#endif
	this->screenWidth = sWidth;
	this->screenHeight = sHeight;
	this->dirty=true;
}


void ShapeController::CloseContext() {
	#ifdef rpi
		finish();
	#endif
}

void ShapeController::Add(Shape* shape)
{

	int id = shape->getId();

	// remove old if it already exists
	map<int, Shape*>::iterator it = shapesById->find(id);
	if(it != shapesById->end()) {
		it->second->dispose();
		delete it->second;
	}

	shapesById->operator[](id) = shape;

	this->dirty = true;
}

void ShapeController::Clear()
{
	for(map<int, Shape*>::iterator iter = this->shapesById->begin(); iter != this->shapesById->end(); ++iter )
	{
		Shape* shape = (*iter).second;
		shape->dispose();
		delete shape;
	}

	shapesById->clear();
	this->dirty=true;
}

void ShapeController::Remove(int id)
{
	map<int, Shape*>::iterator it = shapesById->find(id);

	if (it != shapesById->end()) {
		it->second->dispose();
		delete it->second;
		shapesById->erase(id);

		this->dirty= true;
	} 

}

void ShapeController::Replace(int id, Shape* s)
{
	Add(s); // todo obsolete?
	this->dirty=true;
}

int frameCounter = 0;
void ShapeController::Draw() 
{
	if(!this->dirty)
		return;

	
#ifdef rpi
	Start(this->screenWidth, this->screenHeight);                              // Start the picture
#endif
	
	for(map<int, Shape*>::iterator iter = this->shapesById->begin(); iter != this->shapesById->end(); ++iter)
	{
		Shape* shape = (*iter).second;
		if(shape->isVisible())
			shape->draw(this->overscan, this->screenWidth,this->screenHeight);
	}

	
	//char* filename  = "frame.raw";
	
#ifdef rpi
	End();
	//SaveEnd(filename);
#endif
	this->dirty=false;
}

ShapeController::~ShapeController(void)
{
	CloseContext();
	delete shapesById;
}
