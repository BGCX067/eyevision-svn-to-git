// evsmouse.h 
// esk - 04May07
//
// o using a global var to tell GE whether to show the mouse cursor or not
//   (needed for the user-chooses-objects at the end of each trial)
// o the var is needed by w32_demo_window.c and w32_bitmap_trial.c
// o w32_demo_window.c - contains the case statement for windows_messages
// o w32_bitmap_trial.c - contains the code for user-choosing
 

// var for #include statements 
#define EVSMOUSE 1

// using names so it's clear in the code exactly what's happenning
#define EVSNOMOUSE 0		// no, do not show mouse cursor (default behavior of case statement)
#define EVSYESMOUSE 1		// yes, show the mouse cursor (code added for this behavior) 

// boolean to tell WM_SETCURSOR (case statement in w32_demo_window.c) what to do
extern int evsShowMouse;


////////////////////////////////////////////////
// changes made to implement new cursor behavior
//
// o added this file 
// o w32_demo_window.c - added ifndef block
// o w32_demo_window.c - initialize evsShowMouse
//                     - if it's not initialized, we get a link error:
//                       "error LNK2001: unresolved external symbol _evsShowMouse"
// o w32_demo_window.c - edited "case WM_SETCURSOR" block
// o w32_bitmap_trial.c - added ifndef block
// o w32_bitmap_trial.c - enable cursor display before call to selectTargetObjects
//                      - disable cursor display after call
