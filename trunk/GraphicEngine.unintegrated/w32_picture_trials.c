/*****************************************************/
/* Windows 95/98/NT/2000/XP sample experiment in C   */
/* For use with Version 2.0 of EyeLink Windows API   */
/*                                                   */
/*      (c) 1997-2002 by SR Research Ltd.            */
/* For non-commercial use by Eyelink licencees only  */
/*                                                   */
/* THIS FILE: w32_picture_trials.c                   */
/* CONTENTS:                                         */
/* - run_trials() loops through trials, and handles  */
/*   aborted/repeated trial requests                 */
/* - do_text_trial() interperts trial number         */
/*   and supplies appropriate TRIALID label and      */
/*   creates a stimulus bitmap for each trial        */
/* - See documentation for more details              */  
/*                                                   */
/*                                                   */
/* CHANGES FOR Windows 2000/XP, EyeLink 2.0:         */
/* - use w32_exptsppt.h                              */
/* - add end_realtime_mode() to clean up trials      */
/* - use image_file_bitmap() to load images          */
/*                                                   */
/*                                                   */
/* - add functionalities for graphic engine          */
/* - more detail will be added later                 */
/*****************************************************/

#include <windows.h>
#include <windowsx.h>
#include <string.h>
#include <stdio.h>
#include "eyelink.h"
#include "w32_exptsppt2.h"
#include "w32_demo.h"  /* header file for this experiment */
#include "interpreter.h"
 
void initialize()
{
	int i;
	int j;
	
	sessionParams.displayX = 1024;
	sessionParams.displayY = 768;
	sessionParams.numTrials = 4;  //4

	//trialParams.duration = 35000L;
	trialParams.duration = 30000L;
	trialParams.gridDisplay = 0;
	trialParams.bgColorR = 32;
	trialParams.bgColorG = 178;
	trialParams.bgColorB = 170;
	trialParams.fgColorR = 255;
	trialParams.fgColorG = 0;
	trialParams.fgColorB = 0;
	trialParams.isBounce = 1;
	trialParams.minBounceDist = 100;
	trialParams.minStartDist = 200;
	trialParams.numObjects = 9;  //***change back to 7 objects later
	trialParams.numTargets = 4;
	trialParams.isFixMarker = 1;

	trialParams.objInfo = malloc(sizeof(struct oinfo) * trialParams.numObjects);

//***change back to i < 7 later
	for(i=0; i < 9; i++)
	{
		trialParams.objInfo[i].objNum = ++i;
		trialParams.objInfo[i].objPanel = 0;
	}
	
	trialParams.objInfo[0].objPath = "images\\test1.bmp";
	trialParams.objInfo[0].objIsTarget = 1;
	trialParams.objInfo[1].objPath = "images\\test2.bmp";
	trialParams.objInfo[1].objIsTarget = 1;  //***change back to 0 later
	trialParams.objInfo[2].objPath = "images\\test3.bmp";
	trialParams.objInfo[2].objIsTarget= 0;
	trialParams.objInfo[3].objPath = "images\\test4.bmp";
	trialParams.objInfo[3].objIsTarget= 0;
	trialParams.objInfo[4].objPath = "images\\test5.bmp";
	trialParams.objInfo[4].objIsTarget= 1;
	trialParams.objInfo[5].objPath = "images\\test6.bmp";
	trialParams.objInfo[5].objIsTarget= 0;
	trialParams.objInfo[6].objPath = "images\\test7.bmp";
	trialParams.objInfo[6].objIsTarget = 1;
    trialParams.objInfo[7].objPath = "images\\test8.bmp";
	trialParams.objInfo[7].objIsTarget= 1;
	trialParams.objInfo[8].objPath = "images\\test9.bmp";
	trialParams.objInfo[8].objIsTarget = 1;
	//trialParams.objInfo[9].objPath = "images\\test777.bmp";
	//trialParams.objInfo[9].objIsTarget = 0;
	//trialParams.objInfo[10].objPath = "images\\test666.bmp";
	//trialParams.objInfo[10].objIsTarget = 1;
	
	//***change back to 7 later
	for(j=0; j < 9; j++)
	{
		trialParams.objInfo[j].objMinSpeedH = 1;
		trialParams.objInfo[j].objMinSpeedV = 1;
		trialParams.objInfo[j].objMaxSpeedH = 300;
		trialParams.objInfo[j].objMaxSpeedV = 300;
	}
}

void init23()
{
	int i,j;
	trialParams.duration = 30000L;
	trialParams.isBounce = 1;
	trialParams.numObjects = 9;  //change back to 10

	trialParams.objInfo = malloc(sizeof(struct oinfo) * trialParams.numObjects);

//***change back to i < 10 later
	for(i=0; i < 9; i++)
	{
		trialParams.objInfo[i].objNum = ++i;
		trialParams.objInfo[i].objPanel = 0;
	}
	
	trialParams.objInfo[0].objPath = "images\\rect.bmp";
	trialParams.objInfo[0].objIsTarget = 0;
	trialParams.objInfo[1].objPath = "images\\test55.bmp";
	trialParams.objInfo[1].objIsTarget = 1;  //***change back to 0 later
	trialParams.objInfo[2].objPath = "images\\test66.bmp";
	trialParams.objInfo[2].objIsTarget= 0;
	trialParams.objInfo[3].objPath = "images\\test77.bmp";
	trialParams.objInfo[3].objIsTarget= 0;
	trialParams.objInfo[4].objPath = "images\\test5.bmp";
	trialParams.objInfo[4].objIsTarget= 1;
	trialParams.objInfo[5].objPath = "images\\test6.bmp";
	trialParams.objInfo[5].objIsTarget= 1;
	trialParams.objInfo[6].objPath = "images\\test7.bmp";
	trialParams.objInfo[6].objIsTarget = 1;
    trialParams.objInfo[7].objPath = "images\\test555.bmp";
	trialParams.objInfo[7].objIsTarget= 0;
	trialParams.objInfo[8].objPath = "images\\test666.bmp";
	trialParams.objInfo[8].objIsTarget = 1;
	//trialParams.objInfo[9].objPath = "images\\test777.bmp";
	//trialParams.objInfo[9].objIsTarget = 0;
	
	
	//***change back to 10 later
	for(j=0; j < 9; j++)
	{
		trialParams.objInfo[j].objMinSpeedH = 1;
		trialParams.objInfo[j].objMinSpeedV = 1;
		trialParams.objInfo[j].objMaxSpeedH = 300;
		trialParams.objInfo[j].objMaxSpeedV = 300;
	}
}

// ************************************************************ 

	// Retrieves the width information for the specified bitmap
	// hbm: handle to the bitmap object
	// GetObject function retrieves information for the specified graphics object
int bitmap_width(HBITMAP hbm)
{
  BITMAP bm;
  GetObject(hbm, sizeof(bm), &bm);
  return bm.bmWidth;
}	

	// Retrieves the height information for the specified bitmap
	// hbm: handle to the bitmap object
	// GetObject function retrieves information for the specified graphics object
int bitmap_height(HBITMAP hbm)
{
  BITMAP bm;
  GetObject(hbm, sizeof(bm), &bm);
  return bm.bmHeight;
}	

	// Adds individual pieces of source bitmap to create a composite picture
	// Top-left placed at (x,y)
	// If either the width or height of the display as 0, then simply copy the bitmap 
	// to the display without chaning its size. Otherwise, stretches the bitmap to  
	// fit the dimensions of the destination display area
int add_bitmap(HBITMAP src, HBITMAP dst, int x, int y, int width, int height)
{
  HDC src_mdc, dst_mdc;
  HBITMAP osrc, odst;
	
  dst_mdc = CreateCompatibleDC(NULL);	// Create a memory context for BITMAP
  src_mdc = CreateCompatibleDC(NULL);
  odst = SelectObject(dst_mdc, dst);	// Select bitmap into memory DC
  osrc = SelectObject(src_mdc, src);
  if(width==0 || height==0)
    BitBlt( dst_mdc, x, y, width, height, src_mdc, 0, 0, SRCCOPY);	// Use BitBlt to copy to display
  else
    StretchBlt( dst_mdc, x, y, width, height, src_mdc, 0, 0, 
                bitmap_width(src), bitmap_height(src), SRCCOPY);	// Use StretchBlt to stretch and copy to display	
  
  SelectObject(dst_mdc, odst);			// release GDI resources
  SelectObject(src_mdc, osrc);
  return 0;
}



/*********** TRIAL SETUP AND RUN **********/

         // FOR EACH TRIAL:
         // - set title, TRIALID
         // - Create bitmaps and EyeLink display graphics
         // - Check for errors in creating bitmaps
         // - Run the trial recording loop
         // - Delete bitmaps
         // - Return any error code

              // Given trial number, execute trials
              // Returns trial result code
//int do_picture_trial(int num)
//int do_picture_trial(int trialNum, int displayX, int displayY, int gridDisplay, UINT32 duration, int fgColorR, int fgColorG, int fgColorB, COLORREF bgColor, int numObjects)
int do_picture_trial(int trialNum, int displayX, int displayY, int gridDisplay, UINT32 duration, COLORREF fgColor, COLORREF bgColor, int numObjects)
{
  HBITMAP *bitmap;
 // struct bitmapWidthHeight *bWidthHeight;
  int i;
  int j;
//  int k;

   bitmap = malloc(sizeof(HBITMAP)*numObjects);

  
   //bWidthHeight = malloc(sizeof(struct bitmapWidthHeight) * numObjects);

        // Before recording, we place reference graphics on the EyeLink display
  set_offline_mode();		     // Must be offline to draw to EyeLink screen

  	  for(j=0; j <numObjects;  j++)
	  {
     
		bitmap[j] = image_file_bitmap(trialParams.objInfo[j].objPath,1,SCRWIDTH,SCRHEIGHT,bgColor);
     
			if(!bitmap[j])
			{
			  alert_printf("ERROR: could not create image %d", 0);
			  return SKIP_TRIAL;
			}
	  }
	
	   if((gridDisplay == 1)||(gridDisplay == 2))
	  {
			trialParams.objInfo[0].objPanel = 0;
			trialParams.objInfo[1].objPanel = 0;
			trialParams.objInfo[2].objPanel = 0;  //***uncomment later
			trialParams.objInfo[3].objPanel = 0;
			trialParams.objInfo[4].objPanel = 1;
			trialParams.objInfo[5].objPanel = 1;
			trialParams.objInfo[6].objPanel = 1;
			trialParams.objInfo[7].objPanel = 1;
			trialParams.objInfo[8].objPanel = 1;
			//trialParams.objInfo[9].objPanel = 1;

	  }
	  else if(gridDisplay == 3)
	  {
			trialParams.objInfo[0].objPanel = 0;
			trialParams.objInfo[1].objPanel = 0;
			trialParams.objInfo[2].objPanel = 0; //***uncomment later
			trialParams.objInfo[3].objPanel = 1;
			trialParams.objInfo[4].objPanel = 2;
			trialParams.objInfo[5].objPanel = 2;
			trialParams.objInfo[6].objPanel = 2;
			trialParams.objInfo[7].objPanel = 3;
			trialParams.objInfo[8].objPanel = 3;
			//trialParams.objInfo[9].objPanel = 3;
	  }
	  

		  
  //*******************

/*
  // NOTE:  *** THE FOLLOWING TEXT SHOULD NOT APPEAR IN A REAL EXPERIMENT!!!! ****
  clear_full_screen_window(target_background_color);
  get_new_font("Arial", 24, 1);
  graphic_printf(target_foreground_color, -1, 1, SCRWIDTH/2, SCRHEIGHT/2, "Sending image to EyeLink...");
 // graphic_printf(target_foreground_color, -1, 1, SCRWIDTH/2, SCRHEIGHT/2, "My test for debug .....1...");
*/

  // Transfer bitmap to tracker as backdrop for gaze cursors
 // bitmap_to_backdrop(bitmap, 0, 0, 0, 0,
//			0, 0, BX_MAXCONTRAST|(is_eyelink2?0:BX_GRAYSCALE));  

    // record the trial
  
  i = bitmap_recording_trial(bitmap, trialNum, displayX, displayY, gridDisplay, duration, fgColor, bgColor);
  
 free(bitmap);
 DeleteObject(bitmap);
 //  graphic_printf(target_foreground_color, -1, 1, SCRWIDTH/2, SCRHEIGHT/2, "My test for debug 2...");


   
  return i;
}


/*********** TRIAL LOOP **************/

	/* This code sequences trials within a block */
	/* It calls run_trial() to execute a trial, */
	/* then interperts result code. */
	/* It places a result message in the EDF file */
	/* This example allows trials to be repeated */
	/* from the tracker ABORT menu. */
int run_trials(void)
{
  int i;
  int trial;
  
  initialize();  //**********temporary will remove after actual function's return value. :May****

  target_foreground_color = RGB(0,0,0);          // set background for calibration
  target_background_color = RGB(128,128,128);    // This should match the display 
  set_calibration_colors(target_foreground_color, target_background_color); 

  		/* PERFORM CAMERA SETUP, CALIBRATION */
  do_tracker_setup();

  /* loop through trials */
  for(trial=0; trial < sessionParams.numTrials; trial++)
    {
      if(eyelink_is_connected()==0 || break_pressed())    /* drop out if link closed */
	{
	  return ABORT_EXPT;
	}
				/* RUN THE TRIAL */
	  if(trial == 0){

		 // trialParams.minStartDist = 100;
		target_background_color = RGB(32,178,170); //green
		trialParams.bgColorR = 32;
		trialParams.bgColorG = 178;
		trialParams.bgColorB = 170;

		target_foreground_color = RGB(255,0,0); 
		trialParams.fgColorR = 255;
		trialParams.fgColorG = 0;
		trialParams.fgColorB = 0;
		
		/* splitScreen == 0 for no divider */
			  
		//i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, trialParams.gridDisplay, trialParams.duration, 255, 0, 0,target_background_color,trialParams.numObjects); // the last param is the number of object
		i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, trialParams.gridDisplay, trialParams.duration, target_foreground_color, target_background_color,trialParams.numObjects); // the last param is the number of object
	  }
	  else if (trial == 1){
		//i = do_picture_trial(trial, displayX, displayY, 0, 1000L);  // splitScreen == 1 for horizontal line 
		// trialParams.minStartDist = 300;
		 
	    init23();
		target_background_color = RGB(255,255,0); //yellow

		trialParams.bgColorR = 255;
		trialParams.bgColorG = 255;
		trialParams.bgColorB = 0;

		target_foreground_color = RGB(255,0,0); 
		trialParams.fgColorR = 255;
		trialParams.fgColorG = 0;
		trialParams.fgColorB = 0;
		
		//i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, 1, trialParams.duration, 255, 0, 0,target_background_color,trialParams.numObjects); // the last "2" is the number of object
		i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, 1, trialParams.duration, target_foreground_color,target_background_color,trialParams.numObjects); // the last "2" is the number of object
		
	  }
	  
	  else if(trial == 2){
		//i = do_picture_trial(trial, displayX, displayY, 1, 2000L);  // splitScreen == 2 for vertical line 	
	//trialParams.minStartDist = 300;
		target_background_color = RGB(0,255,255); //cyan

		trialParams.bgColorR = 0;
		trialParams.bgColorG = 255;
		trialParams.bgColorB = 255;

		target_foreground_color = RGB(255,255,0); 
		trialParams.fgColorR = 255;
		trialParams.fgColorG = 255;
		trialParams.fgColorB = 0;

		//i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, 2, trialParams.duration, 255, 255, 0,target_background_color,trialParams.numObjects);
	    i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, 2, trialParams.duration, target_foreground_color, target_background_color,trialParams.numObjects);
	  }
	  else if(trial == 3){
		//i = do_picture_trial(trial, displayX, displayY, 2, 3000L);  // splitScreen == 3 for both horizontal and vertical lines 
		
		 // trialParams.minStartDist = 300;
		target_background_color = RGB(32,178,170); //green

		trialParams.bgColorR = 255;
		trialParams.bgColorG = 0;
		trialParams.bgColorB = 0;

		target_foreground_color = RGB(255,0,255); 

		trialParams.fgColorR = 32;
		trialParams.fgColorG = 178;
		trialParams.fgColorB = 170;

		//i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, 3, trialParams.duration, 32, 178, 170,target_background_color,trialParams.numObjects);
	    i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, 3, trialParams.duration, target_foreground_color, target_background_color,trialParams.numObjects);
	  
	  }
	  
      end_realtime_mode();

      switch(i)         	/* REPORT ANY ERRORS */
	{
	  case ABORT_EXPT:        /* handle experiment abort or disconnect */
	    eyemsg_printf("EXPERIMENT ABORTED");
	    return ABORT_EXPT;
	  case REPEAT_TRIAL:	  /* trial restart requested */
	    eyemsg_printf("TRIAL REPEATED");
	    trial--;
	    break;
	  case SKIP_TRIAL:	  /* skip trial */
	    eyemsg_printf("TRIAL ABORTED");
	    break;
	  case TRIAL_OK:          // successful trial
	    eyemsg_printf("TRIAL OK");
	    break;
	  default:                // other error code
	    eyemsg_printf("TRIAL ERROR");
	    break;
	}
    }  // END OF TRIAL LOOP
  return 0;
}
