/**********************************************************/
/* Windows 95/98/NT/2000/XP experiment C support          */
/* For use with Version 2.0 of EyeLink Windows API        */
/*                                                        */
/*      (c) 1997-2002 by SR Research Ltd.                 */
/* For non-commercial use by Eyelink licencees only       */
/*                                                        */
/* THIS FILE: w32_text_bitmap.c                           */
/* CONTENTS:                                              */
/* - copy bitmaps to display                              */
/* - create new, bank bitmap                              */
/*                                                        */
/*                                                        */
/* CHANGES FOR Windows 2000/XP, EyeLink 2.0:              */
/* - change header file to w32_exptsppt2.h                */
/* - add GdiFlush after drawing poerations                */
/**********************************************************/
/*
/* for EVS - esk 06May2007
/*
/* - changed display_bitmap() to use
/*   TransparentBlt() instead of BitBlt()
/* - that means that all objects (rect, circle, bmp)
/*   are transparently blted, not just copied
/*   (don't know how this will affect trial speeds.  
/*    May have to consider only transparently blting ellipses)
/* - edits -- added #include for interpreter.h (doesn't seem to need ifndef)
/*         -- commented out call to BitBlt and added TransparentBlt
/*         -- function call requires include-ing windows.h (already in file)
/*         -- function call requires adding Msimg32.lib to Linker's "Additional Dependencies"
/*            else we get the following error:
/*            error LNK2019: unresolved external symbol __imp__TransparentBlt@44 referenced in function _display_bitmap	w32_bitmap_sppt.obj
/* - refs -- msdn2.microsoft.com/en-us/library/ms532278.aspx (BitBlt)
/*        -- msdn2.microsoft.com/en-us/library/ms532303.aspx (TransparentBlt)
/*        -- msdn.microsoft.com/archive/default.asp?url=/archive/en-us/dnargdi/html/msdn_transblt.asp
/*           (Ron Gery article on GDI "Bitmaps with Transparency")
/*        -- msdn2.microsoft.com/en-us/library/ms533266.aspx (GetDeviceCaps)
/*           (didn't use - including ref as extra info)
/*        -- support.microsoft.com/kb/77934 (didn't use - including as extra info)
/*           ("Determining That Display Driver Supports Transparent Mode")
/*
/**********************************************************/

#include <windows.h>
#include <windowsx.h>
#include "w32_demo.h"

#include "interpreter.h"	// esk-added to get bg color for trial

/************ DISPLAY ENTIRE BITMAP **************/

         // Copy a DDB bitmap to the display 
         // Top-left placed at (x,y)
         // Speed depends on video mode and graphics card
void display_bitmap(HWND hwnd, HBITMAP hbm, int x, int y)
{
  BITMAP bm;        // Bitmap data structure
  HDC hdc;          // Display context (to create memory context)
  HDC mdc;          // Memory context to draw bitmap in
  HBITMAP obm;      // old GDI bitmap

  hdc = GetDC(hwnd);                     // Display DC 
  mdc = CreateCompatibleDC(hdc);         // Memory DC
  obm = SelectObject(mdc, hbm);          // Select bitmap into memory DC
  
  GetObject(hbm, sizeof(BITMAP), &bm);   // Get data on bitmap
                                         // Use BitBlt to copy to display

  //esk-commented out in favor of TransparentBlt
  //BitBlt(hdc, x, y, bm.bmWidth, bm.bmHeight, mdc, 0, 0, SRCCOPY);
  TransparentBlt(hdc, x, y, bm.bmWidth, bm.bmHeight, mdc, 0, 0, bm.bmWidth, bm.bmHeight, RGB(trialParams.bgColorR,trialParams.bgColorG,trialParams.bgColorB));
  
  GdiFlush();   // ADDED for Windows 2000/XP: Forces drawing to be immediate

  SelectBitmap(mdc, obm);                // Deselect bitmap
  DeleteDC(mdc);                         // get rid of memory DC
  ReleaseDC(hwnd, hdc);
}
