#include "Structures.h"
#include <iostream>


int main();

extern "C" void Initialize(OVERSCAN overscan);

extern "C" void SetRect(SHAPE_RECT r);
extern "C" void SetText(SHAPE_TEXT t);
extern "C" void SetImage(SHAPE_IMAGE i);


extern "C" void Clear();
extern "C" void Remove(int id);
extern "C" void Draw();

extern "C" void Destroy();

extern "C" void CloseContext();
extern "C" void OpenContext();


void hexDump (char *desc, void *addr, int len);
