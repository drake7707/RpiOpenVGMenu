#pragma once
#include <string>
#include "Shape.h"
#include "ImageController.h"

class SImage
	: public Shape
{

public:
	SImage(int id, float x, float y, float width, float height, std::string filename);
	virtual ~SImage(void);
	virtual void draw(OVERSCAN overscan, int screenWidth, int screenHeight);
	virtual void dispose();
	void setX(float x);
	void setY(float y);
	void setWidth(float width);
	void setHeight(float height);

	void setFilename(std::string filename);
	std::string getFilename();

	float getX();
	float getY();
	float getWidth();
	float getHeight();

protected:
	float x;
	float y;
	float width;
	float height;
	std::string filename;
};

