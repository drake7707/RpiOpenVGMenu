#pragma once
#include "Shape.h"

class SRectangle :
	public Shape
{

public:
	SRectangle(int id, float x, float y, float width, float height);
	virtual void draw(OVERSCAN overscan, int screenWidth, int screenHeight);
	virtual void dispose();
	virtual ~SRectangle(void);

	void setX(float x);
	void setY(float y);
	void setWidth(float width);
	void setHeight(float height);

	void setColor(COLOR color);
	COLOR getColor();

	float getX();
	float getY();
	float getWidth();
	float getHeight();

protected: 
	float x;
	float y;
	float width;
	float height;
	COLOR color;

};

