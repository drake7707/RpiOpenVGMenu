#include "ImageController.h"
#include <iostream>

ImageController::ImageController(void)
{
	imagesByPath = new std::map<std::string, IMAGE_CACHE>();
}


ImageController::~ImageController(void)
{
}

ImageController& ImageController::getInstance() {
    static ImageController  instance; 
    return instance;
}

VGImage ImageController::getImage(std::string str) {
	std::map<std::string, IMAGE_CACHE>::iterator it = imagesByPath->find(str);
	if(it != imagesByPath->end()) 
	{
		return it->second.image;
	}
	std::cout << "Image not found in cache: " << str;

	return VGImage();
}

std::string GetFileExtension(const std::string& fileName)
{
    if(fileName.find_last_of(".") != std::string::npos)
        return fileName.substr(fileName.find_last_of(".")+1);
    return "";
}

void ImageController::createImage(std::string str) {

	std::map<std::string, IMAGE_CACHE>::iterator it = imagesByPath->find(str);
	if(it != imagesByPath->end()) 
	{
		it->second.count++;
	}
	else 
	{
		IMAGE_CACHE ic = IMAGE_CACHE();
		ic.count = 1;

		VGImage img;
#ifdef rpi
		if(GetFileExtension(str) == "raw")
			img = createImageFromRaw(str.c_str());
		else
			img = createImageFromJpeg(str.c_str());
#endif
		ic.image = img;

		std::cout << "Adding " << str << " to the image cache\n";
		imagesByPath->operator[](str) = ic;
	}
}

void ImageController::releaseImage(std::string str) {

	std::map<std::string, IMAGE_CACHE>::iterator it = imagesByPath->find(str);
	if(it != imagesByPath->end()) {

		it->second.count--;
		if(it->second.count <= 0)
		{
#ifdef rpi
			vgDestroyImage(it->second.image);
#endif
			std::cout << "Removing " << str << " from the image cache\n";
			imagesByPath->erase(it);
		}

	}
}

void ImageController::rebuildImages() {
	for(std::map<std::string, IMAGE_CACHE>::iterator it = this->imagesByPath->begin(); it != this->imagesByPath->end(); ++it)
	{
		VGImage img = it->second.image;

#ifdef rpi
			vgDestroyImage(img);
#endif

#ifdef rpi
		if(GetFileExtension(it->first) == "raw")
			img = createImageFromRaw(it->first.c_str());
		else
			img = createImageFromJpeg(it->first.c_str());

#endif
		it->second.image = img;
	}



}
