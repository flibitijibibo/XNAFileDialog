all: clean
	c++ -O2 -fpic -fPIC -shared -o libXNAFileDialog.so *.cpp -I. -lSDL2

clean:
	rm -f libXNAFileDialog.so
