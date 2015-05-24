// first OpenVG program
// Anthony Starks (ajstarks@gmail.com)
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include "VG/openvg.h"
#include "VG/vgu.h"
#include "fontinfo.h"
#include "shapes.h"

int main() {
	int width, height;
	char s[3];

	init(&width, &height);				   // Graphics initialization

	Start(width, height);				   // Start the picture
	Background(0, 0, 0);				   // Black background
	Fill(44, 77, 232, 1);				   // Big blue marble
	//Circle(width / 2, 0, width);			   // The "world"

	int size = 40;

	float w = 0.5 * width;
	float h = 0.5 * height;
	float x = 0.25 * width;
	float y = height - 0.25 * height;
	
	Rect(x, (1 - 0.25) * height - 0.5 * height,w,h);

	
	//TextMid(width / 2, height / 2, "hello, world", SerifTypeface, width / 10);	// Greetings 


	char* text = "Three years ago, Despicable Me launched Illumination Entertainment and announced Universal Studios as a viable player in the animation game (only Disney/Pixar and DreamWorks used to show up to these box-office battles). The film wasn't even the only supervillain animation to hit the theaters that year, but it did one-up its rival Megamind both in critical acclaim and commercial success.\n\nNow, the original film's creative team returns with Despicable Me 2, continuing the adventures of former supervillain father Gru, his precocious daughters Margo, Edith, and Agnes, and his little...";

	printf("Writing text\n");

	Fill(255, 255, 255, 1);				   // White text
	TextWrap(x, y, text,
		    w, h, 0.2, SerifTypeface, size);

	printf("End\n");

	
	End();						   // End the picture

	fgets(s, 2, stdin);				   // look at the pic, end with [RETURN]
	finish();					   // Graphics cleanup
	exit(0);
}
