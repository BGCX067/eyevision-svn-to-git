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
/* Author: May Wong, Bahar Akbal, Elena Khan         */
/*****************************************************/

#include <windows.h>
#include <windowsx.h>
#include <string.h>
#include <stdio.h>
#include "eyelink.h"
#include "w32_exptsppt2.h"
#include "w32_demo.h"  /* header file for this experiment */
#include "interpreter.h"

// bhr - variables & functions added for edf file name for the session, experiment name, session name.
char edfFileName[260] = "";
char experimentName[250]= "";
char sessionName[10]= "";

char* get_edf_name();
char* get_experiment_name();
char* get_session_name();

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
int do_picture_trial(int trialNum, int displayX, int displayY, int gridDisplay, UINT32 duration, COLORREF fgColor, COLORREF bgColor, int numObjects)
{
  HBITMAP *bitmap;
  int i;
  int j;

  bitmap = malloc(sizeof(HBITMAP)*numObjects);

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

    // record the trial
 i = bitmap_recording_trial(bitmap, trialNum, displayX, displayY, gridDisplay, duration, fgColor, bgColor);
  
 free(bitmap);
 DeleteObject(bitmap);
   
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
  char sessionNumberBuffer[3]; // bhr - holds the session number

  //esk-initialize the interpreter (added return code check 20070520)
  //    o first run initInterp function (in interpreter.c)
  //    o check return code (initInterp() will return -1 if gui closed prematurely)
  if (initInterp() == ERRDOGUI)  return ABORT_EXPT;

  // bhr - get the experiment name
  strcat(experimentName, sessionParams.expName);
  // bhr - generate the session name
  strcat(sessionName, "Sess");
  sprintf(sessionNumberBuffer, "%d", sessionParams.sessNum);
  strcat(sessionName, sessionNumberBuffer);
  // bhr - generate the edf file name for this session
  strcat(edfFileName, experimentName);
  strcat(edfFileName, "_");
  strcat(edfFileName, sessionName);
  strcat(edfFileName, ".EDF");
  

  target_foreground_color = RGB(0,0,0);          // set background for calibration
  target_background_color = RGB(128,128,128);    // This should match the display 
  set_calibration_colors(target_foreground_color, target_background_color); 

  		/* PERFORM CAMERA SETUP, CALIBRATION */
  do_tracker_setup();

  /* loop through trials */
  for(trial=1; trial < sessionParams.numTrials + 1; trial++)
  {
      if(eyelink_is_connected()==0 || break_pressed())    /* drop out if link closed */
	{
	  return ABORT_EXPT;
	}
				/* RUN THE TRIAL */

    //////////////////////////////////////////////////////////////////// (esk - begin add)
	  //esk-get next set of params from GEi
	  getTrialParams(trial);

	  //esk-have to set color for each trial
	  target_foreground_color = RGB(trialParams.fgColorR,trialParams.fgColorG,trialParams.fgColorB);
	  target_background_color = RGB(trialParams.bgColorR,trialParams.bgColorG,trialParams.bgColorB);
	  
	  //esk-copied from original GE code
      i = do_picture_trial(trial, sessionParams.displayX, sessionParams.displayY, trialParams.gridDisplay, trialParams.duration, target_foreground_color, target_background_color,trialParams.numObjects); 
	/////////////////////////////////////////////////////////////////////////// (esk - end add)

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

	/////////////////////////////////////////////////////////////////// (esk - begin add)
	//esk-trials are over - free mem from GEi
	endInterp();
    //////////////////////////////////////////////////////////////////// (esk - end add)

  return 0;
}

// bhr - returns edf file name for the session
char* get_edf_name()
{
	char *fileName = edfFileName;

	return fileName;
}

// bhr - returns experiment name
char* get_experiment_name()
{
	char *expName = experimentName;

	return expName;
}

// bhr - returns session name
char* get_session_name()
{
	char *sessName = sessionName;

	return sessName;
}