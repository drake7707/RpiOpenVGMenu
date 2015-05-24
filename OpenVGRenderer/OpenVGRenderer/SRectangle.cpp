#include "SRectangle.h"

#ifdef rpi

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



SRectangle::SRectangle(int id, float x, float y, float width, float height)
	: Shape(id)
{
	this->x = x;
	this->y = y;
	this->width = width;
	this->height = height;
}

void SRectangle::draw(OVERSCAN overscan, int screenWidth, int screenHeight) 
{
	int sW = screenWidth - overscan.paddingLeft - overscan.paddingRight;
	int sH = screenHeight - overscan.paddingTop - overscan.paddingBottom;


	// draw here
#ifdef rpi
	Fill(color.R, color.G, color.B, (float)color.A / 255);
	//Fill(255,0,0,1);
	Rect(overscan.paddingLeft + this->x * sW, overscan.paddingTop + (1 - this->y) * sH - this->height * sH, this->width * sW, this->height * sH);
#endif

}

SRectangle::~SRectangle(void)
{
}

void SRectangle::setX(float x) {
	this->x = x;
}

void SRectangle::setY(float y) {
	this->y = y;
}
void SRectangle::setWidth(float width) {
	this->width = width;
}

void SRectangle::setHeight(float height) {
	this->height = height;
}

void SRectangle::setColor(COLOR color) {
	this->color = color;
}

float SRectangle::getX() {
	return this->x;
}
float SRectangle::getY() {
	return this->y;
}
float SRectangle::getWidth() {
	return this->width;
}
float SRectangle::getHeight() {
	return this->height;
}

COLOR SRectangle::getColor() {
	return this->color;
}

void SRectangle::dispose() {

}
