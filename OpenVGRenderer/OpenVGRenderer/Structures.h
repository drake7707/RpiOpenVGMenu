#pragma once


struct OVERSCAN {
	int paddingLeft;
	int paddingTop;
	int paddingRight;
	int paddingBottom;
};

struct COLOR {
	char R;
	char G;
	char B;
	char A;
};

struct SHAPE {
	int id;
};

struct SHAPE_RECT {
public:
	int id;
	bool visible;
	float x;
	float y;
	float width;
	float height;
	COLOR backColor;
};

struct SHAPE_TEXT {
public:
	int id;
	bool visible;
	char* text;
	float x;
	float y;
	float width;
	float height;
	float size;
	COLOR color;
};

struct SHAPE_IMAGE {
public:
	int id;
	bool visible;
	char* filename;
	float x;
	float y;
	float width;
	float height;
};
