CC = gcc
CFLAGS= -std=gnu99
LDFLAGS = -Llib
INCLUDES = -Iinc

all: 
	     $(CC) $(CFLAGS) $(LDFLAGS) $(INCLUDES) analyse.c -o analyse lib\libfftw3.a lib\sndfile.lib

clean:
		rm -rf analyse
