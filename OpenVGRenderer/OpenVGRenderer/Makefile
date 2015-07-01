# Application name; change if needed.
APP = openvgrenderer

CXX =g++

TGT =obj
SRC=.

# Listing of source code files
SRCS = $(wildcard *.cpp)

# Formulate the names of the respective object files
OBJS = $(addprefix $(TGT)/, $(notdir $(SRCS:.cpp=.o)))

# Define appropriate compiler options
CXXFLAGS =-mfloat-abi=hard  -Wall -pedantic -Drpi -std=c++0x -shared

# Define appropriate link options and libraries
LDFLAGS =-L/opt/vc/lib  -lGLESv2 -ljpeg -shared
LIBS =-I./lib  -I/opt/vc/include -I/opt/vc/include/interface/vcos/pthreads


.PHONY = all clean


all : $(APP)


$(APP) : $(OBJS)
	$(CXX) $^ ./lib/libshapes.o ./lib/oglinit.o  -o $(APP).lib $(LDFLAGS) $(LIBS)
	cp $(APP).lib ../VGMenu/$(APP).lib

$(TGT)/%.o: $(SRC)/%.cpp
	$(CXX) $(CXXFLAGS) -c $< -o $@ $(LDFLAGS) $(LIBS)


clean :
	$(RM) $(OBJS) $(APP).lib

#all:	*.cpp ./lib/libshapes.o ./lib/oglinit.o
#	
#	g++  -Wall -Drpi -I/opt/vc/include -I/opt/vc/include/interface/vcos/pthreads -I./lib -o  openVGRenderer.lib *.cpp ./lib/libshapes.o ./lib/oglinit.o -L/opt/vc/lib -lGLESv2  -lGLESv2 -ljp

