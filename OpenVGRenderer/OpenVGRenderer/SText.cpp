#include "SText.h"
#include <cstring>

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


SText::SText(int id, float x, float y, float width, float height, float fontsize, std::string text)
	: Shape(id)
{
	this->id = id;
	this->x = x;
	this->y = y;
	this->text = text;
	this->width = width;
	this->height = height;
	this->size = fontsize;
}


SText::~SText(void)
{

}

void SText::draw(OVERSCAN overscan, int screenWidth, int screenHeight) 
{
	int sW = screenWidth - overscan.paddingLeft - overscan.paddingRight;
	int sH = screenHeight - overscan.paddingTop - overscan.paddingBottom;

	char* charTxt = strdup(this->text.c_str());
	
	float lineSpacing = 0.2;

#ifdef rpi
	//Fill(0,255,0, 1);
	//Text(overscan.paddingLeft + this->x * sW,  overscan.paddingTop + (1 - this->y) * sH - this->size * sH, charTxt, SansTypeface, this->size * sH);

	//Fill(255, 0,0, (float)128 / 255);
	//Rect(overscan.paddingLeft + this->x * sW, overscan.paddingTop + (1 - this->y) * sH - this->height * sH, this->width * sW, this->height * sH);

	Fill(this->color.R, this->color.G, this->color.B, (float)this->color.A / 255);
	TextWrap(overscan.paddingLeft + this->x * sW,  
			 overscan.paddingTop + (1 - this->y) * sH - this->size * sH, 
			 charTxt,  
			 this->width * sW, 
			 this->height * sH, 
			 lineSpacing,
			 SansTypeface, this->size * sH);
#endif
	free(charTxt);
}

void SText::setX(float x) {
	this->x = x;
}

void SText::setY(float y) {
	this->y = y;
}
void SText::setWidth(float width) {
	this->width = width;
}

void SText::setHeight(float height) {
	this->height = height;
}

void SText::setSize(float size) {
	this->size = size;
}


void SText::setColor(COLOR color) {
	this->color = color;
}

float SText::getX() {
	return this->x;
}
float SText::getY() {
	return this->y;
}
float SText::getWidth() {
	return this->width;
}
float SText::getHeight() {
	return this->height;
}

float SText::getSize() {
	return this->size;
}


COLOR SText::getColor() {
	return this->color;
}

void SText::dispose() {

}
