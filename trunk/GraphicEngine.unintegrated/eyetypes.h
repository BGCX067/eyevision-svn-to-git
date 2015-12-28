/* EYELINK PORTABLE EXPT SUPPORT    */
/* (c) 1996-2002 by SR Research    */
/*     8 June '97 by Dave Stampe    */
/*     For non-commercial use only  */
/*				    */
/*  Platform-portable types         */

/* UPDATED June 2002 for .NET type reservations */

/* This module is for user applications   */
/* Use is granted for non-commercial      */
/* applications by Eyelink licencees only */

/************** WARNING **************/
/*                                   */
/* UNDER NO CIRCUMSTANCES SHOULD     */
/* PARTS OF THESE FILES BE COPIED OR */
/* COMBINED.  This will make your    */
/* code impossible to upgrade to new */
/* releases in the future, and       */
/* SR Research will not give tech    */
/* support for reorganized code.     */
/*                                   */
/* This file should not be modified. */
/* If you must modify it, copy the   */
/* entire file with a new name, and  */
/* change the the new file.          */
/*                                   */
/************** WARNING **************/

/* 22 May '97: Macintosh types validated (Same as DOS) */
/* 5 June '97: WIN32 types validated (Same as DOS) */

/*#define FARTYPE _far */  /* unusual--for some mixed-model builds */
#define FARTYPE            /* make blank for most DOS, 32-bit, ANSI C */

#ifdef _WIN32
  #include <windows.h>   /* needed for prior declarations of types */
#endif

#ifdef __cplusplus 	/* For C++ definitions */
extern "C" {
#endif

#ifndef BYTEDEF
   #define BYTEDEF 1
//   #if(_MSC_VER < 1300)   // non-reserved
     typedef unsigned char  byte;
     typedef signed short   INT16;
     typedef unsigned short UINT16;
     #ifndef _BASETSD_H_   // VC++ 6.0 defines these types already 
       typedef signed long    INT32;
       typedef unsigned long  UINT32;
     #endif
//   #else                  // VC++ 7.0 (.NET) reserves these types
//     typedef unsigned char  byte;
//     typedef signed short   INT16;
//     typedef signed long     INT32;
//     typedef unsigned short UINT16;
//     typedef unsigned long   UINT32;
//   #endif
#endif

#ifndef MICRODEF            /* Special high-resolution time structure */
	#define MICRODEF 1
	typedef struct {
		INT32  msec;	/* SIGNED for offset computations */
		INT16  usec;
	} MICRO ;
#endif

#ifdef __cplusplus	/* For C++ definitions */
}
#endif