#pragma once
#include "Structures.h"

class Shape
{
public:
	Shape(int id);
	virtual ~Shape(void);
	int getId();

	bool isVisible();
	void setVisible(bool visible);

	virtual void draw(OVERSCAN overscan, int screenWidth, int screenHeight) = 0;
	virtual void dispose() = 0;

protected:
	int id;
	bool visible;

};

