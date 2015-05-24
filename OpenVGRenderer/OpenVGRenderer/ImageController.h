#pragma once
#include <map>
#include <string>

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

#ifndef rpi
class VGImage {

};
#endif


struct IMAGE_CACHE {
	VGImage image;
	int count;
};


class ImageController
{
public:
	ImageController(void);
	~ImageController(void);
	
	static ImageController& getInstance();

	void createImage(std::string path);
	void releaseImage(std::string path);

	void rebuildImages();


	VGImage getImage(std::string path);

private:
	std::map<std::string, IMAGE_CACHE>* imagesByPath;
};

