// testgui.c
//  o ekhan 20070428
//
//  o example of how to use libgeigui.lib


#include <stdlib.h>
#include <stdio.h>


extern __declspec( dllexport ) int doGUI(void);
//extern __declspec( dllexport ) int getFileName(char * buf);


int main()
{
	doGUI();
	return 0;
}