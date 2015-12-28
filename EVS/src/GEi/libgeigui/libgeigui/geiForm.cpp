// geiForm.cpp
//  o acts as a "front-end" for the Graphics Engine GUI (geiForm.h)
//  o compiles into a static library (along with geiForm.h)
//
//  o This code allows the GEinterpreter_parser to access the session_file_name that a user chooses 
//    in the GEinterpreter_GUI (geiForm.h)
//
//    The process is as follows:
//    -the Graphics Engine starts -> calls GEinterpreter (the parser, which is interpreter.c/.h) 
//    -the GEinterpreter (interpreter.c) calls doGUI() in this file
//    -doGUI() runs the Form (geiForm.h), saves the session filename that the user chooses, 
//     and returns the length of the filename to the GEinterpreter_parser (interpreter.c)
//    -the GEinterpreter_parser (interpreter.c) allocates a buffer for the filename
//    -the GEinterpreter_parser (interpreter.c) calls getFileName() in this file to copy the filename into its buffer
//
//    Simply, we're doing this:
//    GE (C code) <--> GEinterpreter (C code) <--> GEgui (C++ code)
//
//  o history
//    -v1 20070302 bakbal - original file using main()
//    -v2 20070428 ekhan  - changed from .exe to .lib; added C-accessible functions doGUI() and getFileName() 
//    -v3 20070519 ekhan  - added catch()


#include "geiForm.h"

#include <string.h>

using namespace libgeigui;
using namespace System::Runtime::InteropServices;

extern "C" __declspec( dllexport ) int doGUI(void);
extern "C" __declspec( dllexport ) int getFileName(char * buf);

const char* fname;


extern "C" __declspec( dllexport ) int doGUI(void)
{
	// o if successful, returns length of session file name
	// o else, returns -1

	int len;
	
	// Enabling Windows XP visual effects before any controls are created
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false); 

	// Create the main window and run it
	Application::Run(gcnew geiForm());

	try {
		String^ s = geiForm::Sfilename::get();

		fname = (char*)(void*)Marshal::StringToHGlobalAnsi(s);
	
		len=strlen(fname);
	
	} catch (System::Exception ^e) {
		// o handle case where user closes gui without choosing a file
		// o else (for debug version): 
		//    Unhandled Exception: System.AccessViolationException: Attempted to read or write
		//    protected memory. This is often an indication that other memory is corrupt.
		//    at strlen(SByte* )
		//    at doGUI() in c:\documents and settings\elena\my documents\visual studio 2005
		//    \projects\libgeigui\libgeigui\geiform.cpp:line 53
		// o or (for release version):
		//    Unhandled Exception: System.NullReferenceException: 
		//    Object reference not set to an instance of an object.
		//    at doGUI()
		//    ...
		
		len = -1;
	}

	return len;
}


extern "C" __declspec( dllexport ) int getFileName(char * buf){
	strcpy(buf,fname);
	
	return 0;	
		// a return value is not really necessary for this function
		//   (since return value of strcpy is buf), but returning an
		//    int allows error-return-codes in the future)
}

