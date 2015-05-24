#include "main.h"
#include "ShapeController.h"
#include <stdlib.h>
#include "SRectangle.h"
#include "SText.h"
#include "SImage.h"

#ifdef rpi
#include <sys/time.h>
#endif

#include <stdlib.h>
#include <stdio.h>
#include <math.h>


using namespace std;

int main() {

	cout << "Size of SHAPE_IMAGE: " << sizeof(SHAPE_IMAGE);

	/*
	int sWidth, sHeight;
	init(&sWidth, &sHeight);
	Start(sWidth, sHeight);

	char * backfilename = "img/background.jpg";
	Image(0,0, sWidth,sHeight, backfilename);
	char* filename  = "frame.raw";
	SaveEnd(filename);

	finish();


	*/


	SHAPE_IMAGE img = SHAPE_IMAGE();
	img.x = 0;
	img.y = 0;
	img.width = 1;
	img.height = 1;
	img.filename = "img/background.jpg";

	int i = 0;

	SHAPE_RECT rect = SHAPE_RECT();
	rect.id = 1;
	rect.x = 0.25;
	rect.y = 0.25;
	rect.width=0.5;
	rect.height=0.5;
	rect.backColor = COLOR();
	rect.backColor.A = 192;
	rect.backColor.R = 64;
	rect.backColor.G = 64;
	rect.backColor.B = 64;

	SHAPE_TEXT txt = SHAPE_TEXT();
	txt.id = 2;
	txt.x = 0.25;
	txt.y = 0.25;
	txt.color  = COLOR();
	txt.color.R = 255;
	txt.color.G = 255;
	txt.color.B = 255;
	txt.color.A = 255;
	txt.width = 0.5f;
	txt.size = 0.5;
	txt.text = "Test";

	OVERSCAN overscan = OVERSCAN();
	Initialize(overscan);
	SetImage(img);
	SetRect(rect);
	SetText(txt);

	Draw();

	int key;
	cin >> key;

	Destroy();


	return 0;
}

ShapeController* controller;

extern "C" void SetRect(SHAPE_RECT r) {
#ifdef rpi
	cout << "Set rect (" << std::to_string(r.id) << ";" <<  std::to_string(r.x) <<  ";" << std::to_string(r.y) <<  ";" << std::to_string(r.width) <<  ";" << std::to_string(r.height) << ")\n";
#endif

	SRectangle* rect = new SRectangle(r.id, r.x, r.y, r.width, r.height);
	rect->setColor(r.backColor);
	rect->setVisible(r.visible);
	controller->Add(rect);
}

extern "C" void SetText(SHAPE_TEXT t) {

#ifdef rpi
	cout << "Set text (" << std::to_string(t.id) << ";" <<  std::to_string(t.x) <<  ";" << std::to_string(t.y) << ")" << string(t.text) << "\n";
#endif

	SText* text = new SText(t.id, t.x, t.y, t.width, t.height, t.size, string(t.text));
	text->setColor(t.color);
	text->setVisible(t.visible);
	controller->Add(text);
}


extern "C" void SetImage(SHAPE_IMAGE i) {
	//hexDump("SHAPE_IMG i", &i, sizeof(i));

#ifdef rpi
	cout << "Set image (" << std::to_string(i.id) << ";" <<  std::to_string(i.x) <<  ";" << std::to_string(i.y) <<  ";" << std::to_string(i.width) <<  ";" << std::to_string(i.height) << ";" << string(i.filename) << "\n";
#endif

	SImage* img = new SImage(i.id, i.x, i.y, i.width, i.height, string(i.filename));
	img->setVisible(i.visible);
	controller->Add(img);
}

extern "C" void Clear() {
	controller->Clear();
}

extern "C" void Remove(int id) {
	controller->Remove(id);
}


extern "C" void Initialize(OVERSCAN overscan) {
	cout << "Initialize\n";
	controller = new ShapeController(overscan);
	
}

extern "C" void OpenContext() {
	controller->OpenContext();
	// rebuild available images because they will be destroyed
	ImageController::getInstance().rebuildImages();
}

extern "C" void CloseContext() {
	controller->CloseContext();
}


#ifdef rpi
/* Return 1 if the difference is negative, otherwise 0.  */
int timeval_subtract(struct timeval *result, struct timeval *t2, struct timeval *t1)
{
	long int diff = (t2->tv_usec + 1000000 * t2->tv_sec) - (t1->tv_usec + 1000000 * t1->tv_sec);
	result->tv_sec = diff / 1000000;
	result->tv_usec = diff % 1000000;

	return (diff<0);
}

void timeval_print(struct timeval *tv)
{
	char buffer[30];
	time_t curtime;

	printf("%ld.%06ld", tv->tv_sec, tv->tv_usec);
	curtime = tv->tv_sec;
	strftime(buffer, 30, "%m-%d-%Y  %T", localtime(&curtime));
	printf(" = %s.%06ld\n", buffer, tv->tv_usec);
}
#endif


extern "C" void Draw() {
	cout << "Draw\n";

#ifdef rpi
	struct timeval tvBegin, tvEnd, tvDiff;
	gettimeofday(&tvBegin, NULL);
#endif

	controller->Draw();

#ifdef rpi
	//end
	gettimeofday(&tvEnd, NULL);

	// diff
	timeval_subtract(&tvDiff, &tvEnd, &tvBegin);
	printf("%ld.%06ld\n", tvDiff.tv_sec, tvDiff.tv_usec);
#endif
}

extern "C" void Destroy() {
	cout << "Destroy\n";

	if(controller != 0)
		controller->Clear(); // make sure to dispose shapes

	delete controller;
}

void hexDump (char *desc, void *addr, int len) {
	int i;
	unsigned char buff[17];
	unsigned char *pc = (unsigned char*)addr;

	// Output description if given.
	if (desc != NULL)
		printf ("%s:\n", desc);

	// Process every byte in the data.
	for (i = 0; i < len; i++) {
		// Multiple of 16 means new line (with line offset).

		if ((i % 16) == 0) {
			// Just don't print ASCII for the zeroth line.
			if (i != 0)
				printf ("  %s\n", buff);

			// Output the offset.
			printf ("  %04x ", i);
		}

		// Now the hex code for the specific character.
		printf (" %02x", pc[i]);

		// And store a printable ASCII character for later.
		if ((pc[i] < 0x20) || (pc[i] > 0x7e))
			buff[i % 16] = '.';
		else
			buff[i % 16] = pc[i];
		buff[(i % 16) + 1] = '\0';
	}

	// Pad out last line if not exactly 16 characters.
	while ((i % 16) != 0) {
		printf ("   ");
		i++;
	}

	// And print the final ASCII bit.
	printf ("  %s\n", buff);
}

