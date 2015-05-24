#include "SImage.h"
#include <cstring>

SImage::SImage(int id, float x, float y, float width, float height, std::string filename)
	: Shape(id)
{
	this->id = id;
	this->x = x;
	this->y = y;
	this->width = width;
	this->height = height;
	this->filename =filename;
	
	ImageController::getInstance().createImage(this->filename);
}


SImage::~SImage(void)
{
}

void SImage::draw(OVERSCAN overscan, int screenWidth, int screenHeight) 
{
	int sW = screenWidth - overscan.paddingLeft - overscan.paddingRight;
	int sH = screenHeight - overscan.paddingTop - overscan.paddingBottom;

	int imgWidth = (int)(this->width * sW);
	int imgHeight = (int)(this->height * sH);

	VGImage image = ImageController::getInstance().getImage(this->filename);
	// draw here
#ifdef rpi
	//Image(this->x * screenWidth, (1 - this->y) * screenHeight, imgWidth, imgHeight, strdup(this->filename.c_str()));
	ImageI(overscan.paddingLeft + this->x * screenWidth, overscan.paddingTop + (1 - this->y) * sH - imgHeight, imgWidth, imgHeight, image);
#endif
}

void SImage::setX(float x) {
	this->x = x;
}

void SImage::setY(float y) {
	this->y = y;
}
void SImage::setWidth(float width) {
	this->width = width;
}

void SImage::setHeight(float height) {
	this->height = height;
}

void SImage::setFilename(std::string filename) {
	this->filename = filename;
}

std::string SImage::getFilename() {
	return this->filename;
}

float SImage::getX() {
	return this->x;
}
float SImage::getY() {
	return this->y;
}
float SImage::getWidth() {
	return this->width;
}
float SImage::getHeight() {
	return this->height;
}

void SImage::dispose() {
	ImageController::getInstance().releaseImage(this->filename);
}
