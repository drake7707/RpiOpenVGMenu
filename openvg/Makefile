LIBFLAGS=-I/opt/vc/include -I/opt/vc/include/interface/vmcs_host/linux -I/opt/vc/include/interface/vcos/pthreads
CXXFLAGS=-mfloat-abi=hard

FONTLIB=/usr/share/fonts/truetype/ttf-dejavu/

all:	libshapes.o oglinit.o

clean:
	rm -f libshapes.o oglinit.o

libshapes.o:	libshapes.c shapes.h fontinfo.h oglinit.o
	gcc -O2  -Wall $(CXXFLAGS) $(LIBFLAGS) -c libshapes.c
	cp libshapes.o ../OpenVGRenderer/OpenVGRenderer/lib/
	cp shapes.h ../OpenVGRenderer/OpenVGRenderer/lib/
	cp oglinit.o ../OpenVGRenderer/OpenVGRenderer/lib/

gopenvg:	openvg.go
	go install .

oglinit.o:      oglinit.c
	gcc  -O2  -Wall $(CXXFLAGS) $(LIBFLAGS) -c oglinit.c

font2openvg:    fontutil/font2openvg.cpp
	g++ $(CXXFLAGS) -I /usr/include/freetype2 fontutil/font2openvg.cpp -o font2openvg -lfreetype

fonts: font2openvg
	for f in $(FONTLIB)/*.ttf; do fn=`basename $$f .ttf`; ./font2openvg $$f $$fn.inc $$fn; done

indent:
	indent -linux -brf -l 132  libshapes.c oglinit.c shapes.h fontinfo.h
