#include "Shape.h"


Shape::Shape(int id)
{
	this->id = id;
}

int Shape::getId() 
{
	return this->id;
}

bool Shape::isVisible() 
{
	return this->visible;
}

void Shape::setVisible(bool visible) 
{
	this->visible = visible;
}


Shape::~Shape(void)
{
}
