#include "Shape.h"
#include <string>

class SText :
	public Shape
{

	
public:
	SText(int id, float x, float y, float width, float height, float fontsize, std::string text);
	virtual void draw(OVERSCAN overscan, int screenWidth, int screenHeight);
	virtual void dispose();
	virtual ~SText(void);

	void setText(std::string text);
	void setX(float x);
	void setY(float y);
	void setWidth(float width);
	void setHeight(float height);

	void setColor(COLOR color);
	COLOR getColor();

	std::string getText();
	float getX();
	float getY();
	float getWidth();
	float getHeight();
	float getSize();
	void setSize(float size);

protected: 
	float x;
	float y;
	float size;
	float width;
	float height;
	COLOR color;
	std::string text;

};
