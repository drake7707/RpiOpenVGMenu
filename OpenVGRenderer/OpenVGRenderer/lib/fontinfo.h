typedef struct {
	const short *CharacterMap;
	const int *GlyphAdvances;
	int Count;
	VGPath Glyphs[256];
} Fontinfo;

extern "C" Fontinfo SansTypeface, SerifTypeface, MonoTypeface;
