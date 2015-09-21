# Makefile for XNAFileDialog
# Written by Ethan "flibitijibibo" Lee

# System information
UNAME = $(shell uname)
ARCH = $(shell uname -m)

# Compiler
ifeq ($(UNAME), Darwin)
	CXX = clang++ -arch i386 -arch x86_64 -mmacosx-version-min=10.6
	TARGET = dylib
else
	CXX = g++
	TARGET = so
endif

all: clean
	$(CXX) -g -fpic -fPIC -shared -o libXNAFileDialog.$(TARGET) *.cpp -I. -lSDL2

clean:
	rm -f libXNAFileDialog.$(TARGET)
