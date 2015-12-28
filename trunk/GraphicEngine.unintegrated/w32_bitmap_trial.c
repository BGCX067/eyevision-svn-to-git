/*********************************************************/
/* Windows 95/98/NT/2000/XP sample experiment in C       */
/* For use with Version 2.0 of EyeLink Windows API       */
/*                                                       */
/*      (c) 1997-2002 by SR Research Ltd.                */
/* For non-commercial use by Eyelink licencees only      */
/*                                                       */
/* THIS FILE: w32_bitmap_trial.c                         */
/* CONTENTS:                                             */
/* - end_trial(): blank display, stop recording          */
/*                cleans up, gives Windows time          */
/* - run_trial(): perfoems actual trial sequence:        */
/*     - drift correction (possible recalibration)       */
/*     - start recording                                 */
/*     - display stmulus to subject                      */
/*     - wait for timeout, button press, or abort        */
/*     - send result message to EDF file                 */
/*     - blank display, stop recording                   */
/*     - report any errors or reqest for repeat          */
/* - See documentation for more details                  */  
/*                                                       */
/*                                                       */
/* CHANGES FOR Windows 2000/XP, EyeLink 2.0:             */
/* - use w32_exptsppt2.h                                 */
/* - add realtime mode during display start              */
/* - display onset message refs retrace sync before draw */
/* - eliminate getkey() for monitoring trial aborts      */
/* - add cleanup code after loop                         */
/* - Add 2000/XP realtime mode cleanup to end_trial()    */
/*                                                       */
/* CHANGES FOR Windows 2000/XP, EyeLink API 2.1:         */
/* - change delay numbers in messages to first item      */
/*                                                       */
/* Author: May Wong                                      */
/* - add functionalities for graphic engine              */
/* - more detail will be added later                     */
/*********************************************************/

#include <windows.h>
#include <windowsx.h>
#include <math.h>
#include <stdio.h>   //file io

#include "eyelink.h"
#include "w32_exptsppt2.h"
#include "w32_demo.h"       /* header file for this experiment */
#include "interpreter.h"

#define DIVIDER_THICKNESS 6  //must be always even number
#define FRAME_RATE 120.0

struct objIniPosition
{
   int x;
   int y;
};

struct currentRandomPosition
{
	int x;
	int y;
};

struct speedPos
{
	double speedX;
	double speedY;
	double newX;
	double newY;
	double lastX;
	double lastY; 
	int Xdirection;
	int Ydirection;

};
struct speedPos *mySpeed;
int panel0Objects[100];    //points all objects in panel0 
int panel1Objects[100];
int panel2Objects[100];
int panel3Objects[100];
int numP0Objects;
int numP1Objects;
int numP2Objects;
int numP3Objects;

//for moving objects positions
struct objectPositions
{
	struct allPositions *objAllPos;
};

struct allPositions
{
	int x;
	int y;
	UINT32 currentTime;
};

 struct objectPositions *moveObjPos;
 
/********* PERFORM AN EXPERIMENTAL TRIAL  *******/


		// End recording: adds 100 msec of data to catch final events
static void end_trial(void)
{
  clear_full_screen_window(target_background_color);    /* hide display */
  end_realtime_mode();   // NEW: ensure we release realtime lock
  pump_delay(100);       // CHANGED: allow Windows to clean up  
                         // while we record additional 100 msec of data 
  stop_recording();
}


	/* Run a single trial, recording to EDF file only */
	/* This example draws to a bitmap, then copies it to display for fast stimulus onset */ 

	// The order of operations is:
	// - Drift correction
	// - start recording
	// - Copy bitmap to display
	// - loop till button press, timeout, or abort	
	// - stop recording, handle abort and exit
	 
                // Trial using bitmap copying
                // Caller must create and destroy the bitmap
//int bitmap_recording_trial(HBITMAP gbm, UINT32 time_limit)
// ***
//int bitmap_recording_trial(HBITMAP *gbm, int trialNum, int displayX, int displayY, int gridDisplay, UINT32 duration, int fgColorR, int fgColorG, int fgColorB, COLORREF bgColor)
int bitmap_recording_trial(HBITMAP *gbm, int trialNum, int displayX, int displayY, int gridDisplay, UINT32 duration, COLORREF fgColor, COLORREF bgColor)
{
  UINT32 trial_start;	// trial start time (for timeout) 
  UINT32 drawing_time;  // retrace-to-draw delay
  UINT32 last_time;
  int button;		    // the button pressed (0 if timeout) 
  int error;            // trial result code
  HDC hdc;
  HPEN pen;
  HBRUSH brush;
  int k;
  int m, n, p, w, r, s, t;          //for keep track of the loop
  int q;
  int a;  // test change direction
 // int qq;  // to test the distance of already tested objects.
//  HBITMAP *bbitmap;
  struct objIniPosition *obj_ini_pos;
 // struct speedPos *mySpeed;
  double distance;
  double tempX1, tempX2, tempY1, tempY2;
 
  int numSamples;      //number of positions for moving objects
  int first = 1;         //(first == 1) means first time is true. (first == 0) means second or more time 
  FILE *f;
// int test;  //****for testing objects in panel **remove later**
 
  //*****for moving objects positions******
  //struct objectPositions *moveObjPos;
 moveObjPos = malloc(sizeof(struct objectPositions) * trialParams.numObjects);
 

  //objects will start to move after 4000ms; each object position will record every 2ms. 
  numSamples = (duration)/2;

//  FILE *f;
  //f = fopen("currentTime.txt", "w");
 // f = fopen("moveObjPosition.txt", "w");
  //  f = fopen("panel01Objects.txt", "w");
  f = fopen("moveObjPositions2.txt", "w");

  mySpeed = malloc(sizeof(struct speedPos)* trialParams.numObjects);
//  bbitmap = malloc(sizeof(HBITMAP) * trialParams.numObjects);

  //***uncomment this later
  obj_ini_pos = objInitPos(trialNum, trialParams.numObjects, displayX, displayY, gbm, gridDisplay);
  
  for(r = 0; r < trialParams.numObjects; r++)
  {
		moveObjPos[r].objAllPos = malloc(sizeof(struct allPositions) * numSamples);
  }

  s = 0;   //index for all moving objects positions

 
    // NOTE: TRIALID AND TITLE MUST HAVE BEEN SET BEFORE DRIFT CORRECTION!
    // FAILURE TO INCLUDE THESE MAY CAUSE INCOMPATIBILITIES WITH ANALYSIS SOFTWARE!

	   // DO PRE-TRIAL DRIFT CORRECTION 
	   // We repeat if ESC key pressed to do setup. 
  while(1)
    {              // Check link often so we can exit if tracker stopped
      if(!eyelink_is_connected()) return ABORT_EXPT;
	   // We let do_drift_correct() draw target in this example
	   // 3rd argument would be 0 if we already drew the fixation target
      error = do_drift_correct(SCRWIDTH/2, SCRHEIGHT/2, 1, 1);
           // repeat if ESC was pressed to access Setup menu 
      if(error!=27) break;
    }

  clear_full_screen_window(target_background_color);  // make sure display is blank
  
	// Start data recording to EDF file, BEFORE DISPLAYING STIMULUS 
	// You should always start recording 50-100 msec before required
	// otherwise you may lose a few msec of data 
  error = start_recording(1,1,0,0);   // record samples and events to EDF file only
  if(error != 0) return error;        // ERROR: couldn't start recording

                              // record for 100 msec before displaying stimulus 
  begin_realtime_mode(100);   // Windows 2000/XP: no interruptions till display start marked

    // DISPLAY OUR IMAGE TO SUBJECT  by copying bitmap to display
    // Because of faster drawing speeds and good refresh locking,
    // we now place the stimulus onset message just after display refresh 
    // and before drawing the stimulus.  This is accurate and will allow 
    // drawing a new stimulus each display refresh.
    // However, we do NOT send the message after the retrace--this may take too long
    // instead, we add a number to the message that represents the delay 
    // from the event to the message in msec
 // wait_for_video_refresh();	// synchronize before drawing so all seen at once
  drawing_time = current_msec();                  // time of retrace
  trial_start = drawing_time;  // record the display onset time 
  
  last_time = trial_start;
  //last 2 params are location x,y positions
  //*******************testing now. Need to change it later using params************

  for(k=0; k < trialParams.numObjects; k++)
  {
	display_bitmap(full_screen_window, gbm[k], obj_ini_pos[k].x, obj_ini_pos[k].y);  // COPY BITMAP to display
	//wait_for_drawing(full_screen_window);    // wait till bitmap copy finished
	drawing_time = current_msec()-drawing_time;    // delay from retrace (time to draw)
  }

  numP0Objects = 0;
  numP1Objects = 0;
  numP2Objects = 0;
  numP3Objects = 0;
  //*******************************uncomment it later*****************
  //setup for moving objects
  
  for(w=0; w < trialParams.numObjects; w++)
	{
		if(gridDisplay == 0)
		{
			do{
				//find random direction (0 == move to left) and (1 == move to right)***original
				// (0 == move to right) , (1 == move to left)
				if((w % 2) == 0)
					mySpeed[w].Xdirection = 0;
				else
					mySpeed[w].Xdirection = 1;
	
				//find the random speed within minimum and maximum horizontal speeds.
				//mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[0].objMaxSpeedH - 1)+ trialParams.objInfo[0].objMinSpeedH);
				mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedH - 1)+ trialParams.objInfo[w].objMinSpeedH);
				mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

				mySpeed[w].lastX = (double)obj_ini_pos[w].x;
				if(mySpeed[w].Xdirection == 0)
					mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
				else 
					mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
			}while((mySpeed[w].newX > sessionParams.displayX)||(mySpeed[w].newX < 0));

			do{
				//find random direction (0 == move to left) and (1 == move to right) ***original
				//(0 == move to bottom), (1 == move to top)
				if((w % 2) == 0)
					mySpeed[w].Ydirection = 0;
				else
					mySpeed[w].Ydirection = 1;

				//find the random speed within minimum and maximum vertical speeds.
				//mySpeed[w].speedY = (double)(rand()% (trialParams.objInfo[0].objMaxSpeedV - 1)+ trialParams.objInfo[0].objMinSpeedV);
				mySpeed[w].speedY = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedV - 1)+ trialParams.objInfo[w].objMinSpeedV);
				mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

				mySpeed[w].lastY = (double)obj_ini_pos[w].y;
				if(mySpeed[w].Ydirection == 0)
					mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
				else
					mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

			}while((mySpeed[w].newY > sessionParams.displayY)||(mySpeed[w].newY < 0));
		}
		//horizontal divider
		else if(gridDisplay == 1)
		{
			if(trialParams.objInfo[w].objPanel == 0)
				panel0Objects[numP0Objects++] =  w;

			else if(trialParams.objInfo[w].objPanel == 1)
				panel1Objects[numP1Objects++] = w;

			do{
				//find random direction (0 == move to left) and (1 == move to right)***original
				// (0 == move to right) , (1 == move to left)
				if((w % 2) == 0)
					mySpeed[w].Xdirection = 0;
				else
					mySpeed[w].Xdirection = 1;
	
				//find the random speed within minimum and maximum horizontal speeds.
				mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedH - 1)+ trialParams.objInfo[w].objMinSpeedH);
				mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

				mySpeed[w].lastX = (double)obj_ini_pos[w].x;
				if(mySpeed[w].Xdirection == 0)
					mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
				else 
					mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
			}while((mySpeed[w].newX > sessionParams.displayX)||(mySpeed[w].newX < 0));

			//upper portion
			if(trialParams.objInfo[w].objPanel == 0)
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					if((w % 2) == 0)
						mySpeed[w].Ydirection = 0;
					else
						mySpeed[w].Ydirection = 1;

					//find the random speed within minimum and maximum vertical speeds.
					
					mySpeed[w].speedY = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedV - 1)+ trialParams.objInfo[w].objMinSpeedV);
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > ((sessionParams.displayY/2)-(DIVIDER_THICKNESS/2)))||(mySpeed[w].newY < 0));
			}
            // bottom portion
			else if(trialParams.objInfo[w].objPanel == 1)
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					if((w % 2) == 0)
						mySpeed[w].Ydirection = 0;
					else
						mySpeed[w].Ydirection = 1;

					//find the random speed within minimum and maximum vertical speeds.
					mySpeed[w].speedY = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedV - 1)+ trialParams.objInfo[w].objMinSpeedV);
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > sessionParams.displayY)||((int)mySpeed[w].newY < ((sessionParams.displayY/2)+(DIVIDER_THICKNESS/2))));
			}		
		}
		//vertical divider
		else if(gridDisplay == 2)
		{
			if(trialParams.objInfo[w].objPanel == 0)
				panel0Objects[numP0Objects++] =  w;

			else if(trialParams.objInfo[w].objPanel == 1)
				panel1Objects[numP1Objects++] = w;

			//left portion
			if(trialParams.objInfo[w].objPanel == 0)
			{
				do{
					//find random direction (0 == move to left) and (1 == move to right)***original
					// (0 == move to right) , (1 == move to left)
					if((w % 2) == 0)
						mySpeed[w].Xdirection = 0;
					else
						mySpeed[w].Xdirection = 1;
	
					//find the random speed within minimum and maximum horizontal speeds.
					mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedH - 1)+ trialParams.objInfo[w].objMinSpeedH);
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > ((sessionParams.displayX/2)-(DIVIDER_THICKNESS/2)))||(mySpeed[w].newX < 0));
			}

			//right portion
			else if(trialParams.objInfo[w].objPanel == 1)
			{
				do{
					//find random direction (0 == move to left) and (1 == move to right)***original
					// (0 == move to right) , (1 == move to left)
					if((w % 2) == 0)
						mySpeed[w].Xdirection = 0;
					else
						mySpeed[w].Xdirection = 1;
	
					//find the random speed within minimum and maximum horizontal speeds.
					//mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[0].objMaxSpeedH - 1)+ trialParams.objInfo[0].objMinSpeedH);
					mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedH - 1)+ trialParams.objInfo[w].objMinSpeedH);
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > sessionParams.displayX)||((int)mySpeed[w].newX < ((sessionParams.displayX/2)+(DIVIDER_THICKNESS/2))));
			}

			do{
				//find random direction (0 == move to left) and (1 == move to right) ***original
				//(0 == move to bottom), (1 == move to top)
				if((w % 2) == 0)
					mySpeed[w].Ydirection = 0;
				else
					mySpeed[w].Ydirection = 1;

				//find the random speed within minimum and maximum vertical speeds.

				mySpeed[w].speedY = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedV - 1)+ trialParams.objInfo[w].objMinSpeedV);
				mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

				mySpeed[w].lastY = (double)obj_ini_pos[w].y;
				if(mySpeed[w].Ydirection == 0)
					mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
				else
					mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

			}while((mySpeed[w].newY > sessionParams.displayY)||(mySpeed[w].newY < 0));
		}
		//both horizontal and vertical dividers
		else if(gridDisplay == 3)
		{
			if(trialParams.objInfo[w].objPanel == 0)
				panel0Objects[numP0Objects++] =  w;

			else if(trialParams.objInfo[w].objPanel == 1)
				panel1Objects[numP1Objects++] = w;

			else if(trialParams.objInfo[w].objPanel == 2)
				panel2Objects[numP2Objects++] = w;

			else if(trialParams.objInfo[w].objPanel == 3)
				panel3Objects[numP3Objects++] = w;

			//check the X coordinate for vertical divider; objects in left portion
			if((trialParams.objInfo[w].objPanel == 0)||(trialParams.objInfo[w].objPanel == 2))
			{
				do{
					//find random direction (0 == move to left) and (1 == move to right)***original
					// (0 == move to right) , (1 == move to left)
					if((w % 2) == 0)
						mySpeed[w].Xdirection = 0;
					else
						mySpeed[w].Xdirection = 1;
	
					//find the random speed within minimum and maximum horizontal speeds.
					mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedH - 1)+ trialParams.objInfo[w].objMinSpeedH);
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > ((sessionParams.displayX/2)-(DIVIDER_THICKNESS/2)))||(mySpeed[w].newX < 0));
			}
			//object in right portion
			else if((trialParams.objInfo[w].objPanel == 1)||(trialParams.objInfo[w].objPanel == 3))
			{
				do{
					//find random direction (0 == move to left) and (1 == move to right)***original
					// (0 == move to right) , (1 == move to left)
					if((w % 2) == 0)
						mySpeed[w].Xdirection = 0;
					else
						mySpeed[w].Xdirection = 1;
	
					//find the random speed within minimum and maximum horizontal speeds.
					//mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[0].objMaxSpeedH - 1)+ trialParams.objInfo[0].objMinSpeedH);
					mySpeed[w].speedX = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedH - 1)+ trialParams.objInfo[w].objMinSpeedH);
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > sessionParams.displayX)||((int)mySpeed[w].newX < ((sessionParams.displayX/2)+(DIVIDER_THICKNESS/2))));
			}

			//check the Y coordinate for horizontal divider;object in upper portion
			if((trialParams.objInfo[w].objPanel == 0) || (trialParams.objInfo[w].objPanel == 1))
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					if((w % 2) == 0)
						mySpeed[w].Ydirection = 0;
					else
						mySpeed[w].Ydirection = 1;

					//find the random speed within minimum and maximum vertical speeds.
					
					mySpeed[w].speedY = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedV - 1)+ trialParams.objInfo[w].objMinSpeedV);
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > ((sessionParams.displayY/2)-(DIVIDER_THICKNESS/2)))||(mySpeed[w].newY < 0));
			}
			//object in bottom portion
			else if((trialParams.objInfo[w].objPanel == 2) || (trialParams.objInfo[w].objPanel == 3))
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					if((w % 2) == 0)
						mySpeed[w].Ydirection = 0;
					else
						mySpeed[w].Ydirection = 1;

					//find the random speed within minimum and maximum vertical speeds.
					mySpeed[w].speedY = (double)(rand()% (trialParams.objInfo[w].objMaxSpeedV - 1)+ trialParams.objInfo[w].objMinSpeedV);
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > sessionParams.displayY)||((int)mySpeed[w].newY < ((sessionParams.displayY/2)+(DIVIDER_THICKNESS/2))));
			}
		}
		
		//Create a blank bitmap with background color for blinking or moving objects.
//		bbitmap[w] = blank_bitmap(bgColor,bitmap_width(gbm[w]),bitmap_height(gbm[w]));
	}

  //eyemsg_printf("%d DISPLAY ON", drawing_time);	 // message for RT recording in analysis 
  //eyemsg_printf("SYNCTIME %d", drawing_time);	 // message marks zero-plot time for EDFVIEW 

                        // we would stay in realtime mode if timing is critical   
                        // for example, if a dynamic (changing) stimulus was used
                        // or if display duration accuracy of 1 video refresh. was needed
  end_realtime_mode();  // we don't care as much about time now, allow keyboard to work
     
                                 // Now get ready for trial loop
  eyelink_flush_keybuttons(0);   // reset keys and buttons from tracker 

                       // we don't use getkey() especially in a time-critical trial
                       // as Windows may interrupt us and cause an unpredicatable delay
                       // so we would use buttons or tracker keys only  

  hdc = GetDC(full_screen_window);      // make it ready to draw on window
 
  /* start drawing horizontal and vertical line on the screen */
  draw_screen_divider(gridDisplay, displayX, displayY, hdc, fgColor);

  brush = CreateSolidBrush(bgColor);
  pen = GetStockObject(NULL_PEN);
  
  // Trial loop: till timeout or response 
  while(1)   
    {                            // First, check if recording aborted 
      if((error=check_recording())!=0) return error;  

	  if(trialParams.isFixMarker == 1)
		draw_fixation_marker(gridDisplay, displayX, displayY, hdc);
	 
	 
	  // blink for 3 seconds (3000ms)
   
	  if((current_time() > trial_start+1000)&&(current_time() < trial_start+1500)||
		 (current_time() > trial_start+2000)&&(current_time() < trial_start+2500)||
		 (current_time() > trial_start+3000)&&(current_time() < trial_start+3500))
	  {		
          for(m = 0; m < trialParams.numObjects; m++)
		  {
				if(trialParams.objInfo[m].objIsTarget == 1)
				{
					//display_bitmap(full_screen_window, bbitmap[m], obj_ini_pos[m].x, obj_ini_pos[m].y); // COPY BITMAP to display
					
					SelectObject(hdc, pen);
					SelectObject(hdc, brush);
					Rectangle(hdc, obj_ini_pos[m].x, obj_ini_pos[m].y, obj_ini_pos[m].x+bitmap_width(gbm[m])+1, obj_ini_pos[m].y+bitmap_height(gbm[m])+1);
				}
		  }
	  }
	
	  else if((current_time() > trial_start+1500)&&(current_time() < trial_start+2000)||
		      (current_time() > trial_start+2500)&&(current_time() < trial_start+3000)||
			  (current_time() > trial_start+3500)&&(current_time() < trial_start+4000))
	  {		  
			for(n = 0; n < trialParams.numObjects; n++)
		    {
				if(trialParams.objInfo[n].objIsTarget == 1)
				{
					display_bitmap(full_screen_window, gbm[n], obj_ini_pos[n].x, obj_ini_pos[n].y);  // COPY BITMAP to display
					 
				}
			}
	  }

	  //moving the objects
	  if(current_time() > trial_start+4000)
	  {
		 // WaitForRetrace();
		  msec_delay(20);
          
		  //store the initial position for the first time
		  if(first == 1)
    	  {
			  for(t = 0; t < trialParams.numObjects; t++)
			  {
					moveObjPos[t].objAllPos[s].currentTime = current_time();
					moveObjPos[t].objAllPos[s].x = obj_ini_pos[t].x;
					moveObjPos[t].objAllPos[s].y = obj_ini_pos[t].y;
			  }
			  first = 0;
			  s++;
		  }
		 
		  for(t = 0; t < trialParams.numObjects; t++)
		  {
		  //	display_bitmap(full_screen_window, bbitmap[t], (int)mySpeed[t].lastX, (int)mySpeed[t].lastY); // COPY BITMAP to display
			    SelectObject(hdc, pen);
				SelectObject(hdc, brush);
				Rectangle(hdc, (int)mySpeed[t].lastX, (int)mySpeed[t].lastY, (int)mySpeed[t].lastX+bitmap_width(gbm[t])+1, (int)mySpeed[t].lastY+bitmap_height(gbm[t])+1);
		  }
		 
		  for(p = 0; p < trialParams.numObjects; p++)
		  {
				moveObjPos[p].objAllPos[s].currentTime = current_time();
				moveObjPos[p].objAllPos[s].x = mySpeed[p].newX;
				moveObjPos[p].objAllPos[s].y = mySpeed[p].newY;

	       
			display_bitmap(full_screen_window, gbm[p], (int)mySpeed[p].newX, (int)mySpeed[p].newY);  // COPY BITMAP to display
			//wait_for_drawing(full_screen_window);  // wait till bitmap copy finished

			mySpeed[p].lastX = mySpeed[p].newX;
			mySpeed[p].lastY = mySpeed[p].newY;
			
			//no screen divider
			if(gridDisplay == 0)
			{
				//if objects need to bounce
				if(trialParams.isBounce == 1)
				{
					computeBounce(gbm, gridDisplay);
				}// object bounces
			 
				moveObjects(gbm, gridDisplay, p);
			}
			//horizontal screen divider
			else if(gridDisplay == 1)
			{
				//if objects need to bounce
				if(trialParams.isBounce == 1)
				{
					computeBounce(gbm, gridDisplay);
				}// object bounces

				moveObjects(gbm, gridDisplay, p);
        	}
			
			//vertical screen divider
			else if(gridDisplay == 2)
			{
				//if objects need to bounce
				if(trialParams.isBounce == 1)
				{
					computeBounce(gbm, gridDisplay);
				}// object bounces

				moveObjects(gbm, gridDisplay, p);
			}
			
			//both horizontal and vertical screen dividers
			else if(gridDisplay == 3)
			{
				//if objects need to bounce
				if(trialParams.isBounce == 1)
				{
					computeBounce(gbm, gridDisplay);
				}// object bounces
               
				moveObjects(gbm, gridDisplay, p);
			}
		  }//end of for loop

		  s++;
	  }
                // Check if trial time limit expired
      if(current_time() > trial_start+duration)
        {

         //*****end of each trial duration*******
			int u, v;
			char* str = "object";
			char* sam = "number of samples";
		/*	while(1)
			{
				 if(escape_pressed())    // check for local ESC key to abort trial (useful in debugging)   
         {
				break;
         }*/
			for(u = 0; u < trialParams.numObjects; u++)
			{
				fprintf(f,"%s %d, %s %d\n",str,u, sam,numSamples);

				for(v = 0; v < s; v++)
				{
					fprintf(f,"%d: %d, %d\n",moveObjPos[u].objAllPos[v].currentTime,moveObjPos[u].objAllPos[v].x,moveObjPos[u].objAllPos[v].y);
				}
			}

			

			//}
          eyemsg_printf("TIMEOUT");    // message to log the timeout 
		  end_trial();                 // local function to stop recording
		  button = 0;                  // trial result message is 0 if timeout 
		  break;                       // exit trial loop
	}
	 

	 if(break_pressed())     // check for program termination or ALT-F4 or CTRL-C keys
	{
	   end_trial();         // local function to stop recording
	   return ABORT_EXPT;   // return this code to terminate experiment
	 }

      if(escape_pressed())    // check for local ESC key to abort trial (useful in debugging)   
         {
	   end_trial();         // local function to stop recording
	   return SKIP_TRIAL;   // return this code if trial terminated
         }

		/* BUTTON RESPONSE TEST */
		// Check for eye-tracker buttons pressed
		// This is the preferred way to get response data or end trials	
      button = eyelink_last_button_press(NULL);
      if(button!=0)       // button number, or 0 if none pressed
	{
	   eyemsg_printf("ENDBUTTON %d", button);  // message to log the button press
	   end_trial();                            // local function to stop recording
	   break;                                  // exit trial loop
	 }
    }                       // END OF RECORDING LOOP
  end_realtime_mode();      // safety cleanup code
  while(getkey());          // dump any accumulated key presses

			   // report response result: 0=timeout, else button number
  eyemsg_printf("TRIAL_RESULT %d", button);
			   // Call this at the end of the trial, to handle special conditions

   
  DeleteObject(pen);   //delete the object for pen 
  DeleteObject(brush); 

//  free(bbitmap);
//  DeleteObject(bbitmap);
  free(mySpeed);
  free(obj_ini_pos);
  
  ReleaseDC(full_screen_window, hdc);
  fclose(f);
 
 // DeleteObject(SelectObject(hdc,GetStockObject(BLACK_PEN)));
  return check_record_exit();
}

//Draw screen divider horizontal, vertical or both.
void draw_screen_divider(int lineType, int displayX, int displayY, HDC hdc, COLORREF fgColor)
{
	HPEN penFg;
	//penFg = CreatePen(PS_SOLID, DIVIDER_THICKNESS, trialParams.fgColorR, trialParams.fgColorG, trialParams.fgColorB);
	penFg = CreatePen(PS_SOLID, DIVIDER_THICKNESS, fgColor);
    SelectObject(hdc, penFg);
    //  SelectObject(hdc, penFg);

	  if (lineType == 1)  /* draw horizontal line */
	  {
		
	  MoveToEx (hdc, 0, displayY/2, NULL);  //set the starting position
	  LineTo (hdc, displayX, displayY/2);   //draw to end position

	  }
	  else if (lineType == 2)  /* draw vertical line */
	  {
	
	  MoveToEx (hdc, displayX/2, 0, NULL);  //set the starting position
	  LineTo (hdc, displayX/2, displayY);   //draw to end position
	
	  }
	  else if (lineType == 3) /* draw both horizontal and vertical lines */
	  {
	
		 MoveToEx (hdc, 0, displayY/2, NULL);  //set the starting position
		 LineTo (hdc, displayX, displayY/2);   //draw to end position
		
		 MoveToEx (hdc, displayX/2, 0, NULL);  //set the starting position
		 LineTo (hdc, displayX/2, displayY);   //draw to end position
	  }
	  DeleteObject(penFg);

}

int average_color(int R, int G, int B)
{
	 int avg;

	 avg = (R + G + B)/3;

	 return avg;
}
void draw_fixation_marker(int gridDisplay, int displayX, int displayY, HDC hdc)
{
	int avgColor;
	int startX, startY, endX, endY;

	//if no divider, check background color.
	if(gridDisplay == 0)
		avgColor = average_color(trialParams.bgColorR, trialParams.bgColorG,trialParams.bgColorB);
	
	else //if there is a divider, check foreground color.
		avgColor = average_color(trialParams.fgColorR, trialParams.fgColorG,trialParams.fgColorB);

	//dark background
	if(avgColor < 128)
		SelectObject(hdc, GetStockObject(WHITE_PEN));
	
	else  //light background
		SelectObject(hdc, GetStockObject(BLACK_PEN));

/*  startX = (displayX/2) - (DIVIDER_THICKNESS/2);
	startY = (displayY/2) - (DIVIDER_THICKNESS/2);
	endX = (displayX/2) + (DIVIDER_THICKNESS/2);
	endY = (displayY/2) + (DIVIDER_THICKNESS/2);
*/

	startX = (displayX/2) - 6;
	startY = (displayY/2) - 6;
	endX = (displayX/2) + 6;
	endY = (displayY/2) + 6;

	MoveToEx (hdc, startX, startY, NULL);  //set the starting position
	LineTo (hdc, endX, endY);   //draw to end position
/*
    startX = (displayX/2) + (DIVIDER_THICKNESS/2);
	endX = (displayX/2) - (DIVIDER_THICKNESS/2);
*/

	startX = (displayX/2) + 6;
	endX = (displayX/2) - 6;

	MoveToEx (hdc, startX, startY, NULL);  //set the starting position
	LineTo (hdc, endX, endY);   //draw to end position
}

struct objIniPosition *objInitPos(int trialNum, int numObjects, int displayX, int displayY, HBITMAP *gbm, int gridDisplay)
{
	struct objIniPosition *op; 
	struct currentRandomPosition curRdPos;
	//int x, y, bitmapWidth, bitmapHeight;
	int bitmapWidth, bitmapHeight;
    int i, j, k;
	int distance;
	int newCurrentPos = 0;

	//FILE *f;

	op = malloc(sizeof(struct objIniPosition) * trialParams.numObjects);

	for(i = 0; i < numObjects; i++)
	{
		bitmapWidth = bitmap_width(gbm[i]);
		bitmapHeight = bitmap_height(gbm[i]);

		distance = 0;

		curRdPos = currObjPos(trialNum, i, gridDisplay, displayX, displayY, bitmapWidth, bitmapHeight);
				
		//The first object position
		
		if(i == 0){
			op[i].x = curRdPos.x;
			op[i].y = curRdPos.y;
		}
		//compare with previous object position for minimum distance
		else
		{
            
			for(j = 1; j <= i; j++)
			{
				distance = distanceBtwObjects(gbm[j-1], op[j-1].x, op[j-1].y, gbm[i], curRdPos.x, curRdPos.y);
				
				while(distance < trialParams.minStartDist)
				{
					newCurrentPos = 1;
					curRdPos = currObjPos(trialNum, i, gridDisplay, displayX, displayY, bitmapWidth, bitmapHeight);
					
					distance = distanceBtwObjects(gbm[j-1], op[j-1].x, op[j-1].y, gbm[i], curRdPos.x, curRdPos.y);
				}
				
				if(newCurrentPos == 1){
					j=1;   //check all over again.
					newCurrentPos = 0;
					continue;
				}
				
				
				distance = 0;

				//compare the current position with all previous positions.
				for(k=0; k < i; k++){
				
				    distance = distanceBtwObjects(gbm[k], op[k].x, op[k].y, gbm[i], curRdPos.x, curRdPos.y);

					while(distance < trialParams.minStartDist)
					{
						newCurrentPos = 1;
						curRdPos = currObjPos(trialNum, i, gridDisplay, displayX, displayY, bitmapWidth, bitmapHeight);
				
						distance = distanceBtwObjects(gbm[k], op[k].x, op[k].y, gbm[i], curRdPos.x, curRdPos.y);
					}

					if(newCurrentPos == 1){
						j=1;
						newCurrentPos = 0;
						break;
					}
				
				}

			}//end of for loop
			op[i].x = curRdPos.x;
			op[i].y = curRdPos.y;
		}
		
		
	}//end of for loop

	return op;
}

//Find the currrent random position of objects.
struct currentRandomPosition currObjPos(int trialNum, int i, int gridDisplay, int displayX, int displayY, int bitmapWidth, int bitmapHeight)
{
	struct currentRandomPosition curPos;
    int x, y;
//	curPos = (struct currentPosition) malloc(sizeof(struct currentPosition));


	//No screen divider
		if(gridDisplay == 0)
		{
			do
			{
				//random number between the screen resolution X.
				x = rand()% displayX;

			}while( (x+bitmapWidth) > displayX); 

			do
			{
				//random number between the screen resolution Y.
				y = rand()% displayY;
		
			}while( (y+bitmapHeight) > displayY);

		}
		//One horizontal screen divider
		else if(gridDisplay == 1)
		{
			do
			{
				//random number between the screen resolution X.
				x = rand()% displayX;

			}while( (x+bitmapWidth) > displayX); 

			do
			{
				//If the object is in upper portion
				if(trialParams.objInfo[i].objPanel == 0)
				{
					y = rand()% (((displayY/2)-(DIVIDER_THICKNESS/2))-bitmapHeight);
				}
				//If the object is in bottom portion
				else if(trialParams.objInfo[i].objPanel == 1)
				{
					y = rand() % ((displayY/2)+(DIVIDER_THICKNESS/2))+ ((displayY/2)+(DIVIDER_THICKNESS/2));
				}
		
			}while( (y+bitmapHeight) > displayY);

		}
		//One vertical screen divider
		else if(gridDisplay == 2)
		{
			do
			{
				//If object is in left portion.
				if(trialParams.objInfo[i].objPanel == 0)
				{
					x = rand() % (((displayX/2)-(DIVIDER_THICKNESS/2))- bitmapWidth);
				}
				//If object is in right portion.
				else if(trialParams.objInfo[i].objPanel == 1)
				{
					x = rand() % ((displayX/2)+(DIVIDER_THICKNESS/2))+((displayX/2)+(DIVIDER_THICKNESS/2));
				}				
			}while( (x+bitmapWidth) > displayX); 
			
			do
			{
				y = rand()% displayY;
		
			}while( (y+bitmapHeight) > displayY);
		}
		//Both horizontal and vertical screen dividers
		else if(gridDisplay == 3)
		{
			do
			{
				//If object is in upper-left or bottom-left
				if((trialParams.objInfo[i].objPanel == 0)||(trialParams.objInfo[i].objPanel == 2))
				{
					x = rand() % (((displayX/2)-(DIVIDER_THICKNESS/2))- bitmapWidth);
				}
				//If object is in upper-right or bottom-right
				else if((trialParams.objInfo[i].objPanel == 1)||(trialParams.objInfo[i].objPanel == 3))
				{
					x = rand() % ((displayX/2)+(DIVIDER_THICKNESS/2))+((displayX/2)+(DIVIDER_THICKNESS/2));
				}
							
			}while( (x+bitmapWidth) > displayX); 

			do
			{
				//If object is in upper-left or upper-right
				if((trialParams.objInfo[i].objPanel == 0)||(trialParams.objInfo[i].objPanel == 1))
				{
					y = rand()% (((displayY/2)-(DIVIDER_THICKNESS/2))-bitmapHeight);
				}
				//If object is in bottom-left or bottom-right.
				else if((trialParams.objInfo[i].objPanel == 2)||(trialParams.objInfo[i].objPanel == 3))
				{
					y = rand() % ((displayY/2)+(DIVIDER_THICKNESS/2))+ ((displayY/2)+(DIVIDER_THICKNESS/2));
				}		
			}while( (y+bitmapHeight) > displayY);
		}

		curPos.x = x;
		curPos.y = y;

		return curPos;
}

//find the distance between two objects
double distanceBtwObjects(HBITMAP bitmap1, double x1, double y1, HBITMAP bitmap2, double x2, double y2)
{
	int x1centerW;
	int y1centerH;
	int x2centerW;
	int y2centerH;
	double xSquare;
	double ySquare;
	double distance;

	x1centerW = bitmap_width(bitmap1)/2;
	y1centerH = bitmap_height(bitmap1)/2;
	x2centerW = bitmap_width(bitmap2)/2;
	y2centerH = bitmap_height(bitmap2)/2;

	xSquare = ((x1+x1centerW)-(x2+x2centerW))*((x1+x1centerW)-(x2+x2centerW));
	ySquare = ((y1+y1centerH)-(y2+y2centerH))*((y1+y1centerH)-(y2+y2centerH));

	distance = sqrt(xSquare + ySquare);

	return distance;
}

// Handle the bouncing case.
void computeBounce(HBITMAP *gbm, int gridDisplay)
{
	int q;
	double distance;

	if(gridDisplay == 0)
	{
		for(q=0; q < trialParams.numObjects-1; q++)
			//if(p < trialParams.numObjects-1)
		{
			int z;
			for(z = q+1; z < trialParams.numObjects; z++)
			{
				distance = distanceBtwObjects(gbm[q], mySpeed[q].newX, mySpeed[q].newY, gbm[z], mySpeed[z].newX, mySpeed[z].newY);
			
				 // two objects are too closed as minimum distance
				//while(distance <= (double)trialParams.minBounceDist)
				if((int)distance <= trialParams.minBounceDist)
				{
					//change horizontal direction for q
					if(mySpeed[q].Xdirection == 0)
						mySpeed[q].Xdirection = 1;
					else
						mySpeed[q].Xdirection = 0;

					//change vertical direction for q
					if(mySpeed[q].Ydirection == 0)
						mySpeed[q].Ydirection = 1;
					else
						mySpeed[q].Ydirection = 0;

                    //change horizontal direction for z
					if(mySpeed[z].Xdirection == 0)
						mySpeed[z].Xdirection = 1;
					else
						mySpeed[z].Xdirection = 0;

					 //change vertical direction for z
					if(mySpeed[z].Ydirection == 0)
						mySpeed[z].Ydirection = 1;
					else
						mySpeed[z].Ydirection = 0;

				 }//if for testing distance

			}//end of for
		}// end of outer for loop	
	}
	else if((gridDisplay == 1)||(gridDisplay == 2))
	{
		//check objects in panel 0 (upper portion) or (left portion)
		if(numP0Objects > 1)
		{
			for(q=0; q < numP0Objects-1; q++)
				//if(p < trialParams.numObjects-1)
			{
				int z;
				for(z = q+1; z < numP0Objects; z++)
				{
					//if(trialParams.objInfo[q].objPanel == trialParams.objInfo[q].objPanel)
					//{
					distance = distanceBtwObjects(gbm[panel0Objects[q]], mySpeed[panel0Objects[q]].newX, mySpeed[panel0Objects[q]].newY, gbm[panel0Objects[z]], mySpeed[panel0Objects[z]].newX, mySpeed[panel0Objects[z]].newY);
			
					// two objects are too closed as minimum distance
					//while(distance <= (double)trialParams.minBounceDist)
					if((int)distance <= trialParams.minBounceDist)
					{
						//change horizontal direction for q
						if(mySpeed[panel0Objects[q]].Xdirection == 0)
							mySpeed[panel0Objects[q]].Xdirection = 1;
						else
							mySpeed[panel0Objects[q]].Xdirection = 0;

						//change vertical direction for q
						if(mySpeed[panel0Objects[q]].Ydirection == 0)
							mySpeed[panel0Objects[q]].Ydirection = 1;
						else
							mySpeed[panel0Objects[q]].Ydirection = 0;

					  //change horizontal direction for z
						if(mySpeed[panel0Objects[z]].Xdirection == 0)
							mySpeed[panel0Objects[z]].Xdirection = 1;
						else
							mySpeed[panel0Objects[z]].Xdirection = 0;

						 //change vertical direction for z
						if(mySpeed[panel0Objects[z]].Ydirection == 0)
							mySpeed[panel0Objects[z]].Ydirection = 1;
						else
							mySpeed[panel0Objects[z]].Ydirection = 0;
					}//if for testing distance

				}//end of for
			//} // end of if check same panel
			}// end of outer for loop	
		}//end if(numP0Objects>1)
		
		//check objects in panel 1 (bottom portion) or (right portion)
		if(numP1Objects > 1)
		{
			for(q=0; q < numP1Objects-1; q++)
			//if(p < trialParams.numObjects-1)
			{
				int z;
				for(z = q+1; z < numP1Objects; z++)
				{
					//if(trialParams.objInfo[q].objPanel == trialParams.objInfo[q].objPanel)
					//{
					distance = distanceBtwObjects(gbm[panel1Objects[q]], mySpeed[panel1Objects[q]].newX, mySpeed[panel1Objects[q]].newY, gbm[panel1Objects[z]], mySpeed[panel1Objects[z]].newX, mySpeed[panel1Objects[z]].newY);
	
					// two objects are too closed as minimum distance
					//while(distance <= (double)trialParams.minBounceDist)
					if((int)distance <= trialParams.minBounceDist)
					{
						//change horizontal direction for q
						if(mySpeed[panel1Objects[q]].Xdirection == 0)
							mySpeed[panel1Objects[q]].Xdirection = 1;
						else
							mySpeed[panel1Objects[q]].Xdirection = 0;

						//change vertical direction for q
						if(mySpeed[panel1Objects[q]].Ydirection == 0)
							mySpeed[panel1Objects[q]].Ydirection = 1;
						else
							mySpeed[panel1Objects[q]].Ydirection = 0;

					  //change horizontal direction for z
						if(mySpeed[panel1Objects[z]].Xdirection == 0)
							mySpeed[panel1Objects[z]].Xdirection = 1;
						else
							mySpeed[panel1Objects[z]].Xdirection = 0;

						//change vertical direction for z
						if(mySpeed[panel1Objects[z]].Ydirection == 0)
							mySpeed[panel1Objects[z]].Ydirection = 1;
						else
							mySpeed[panel1Objects[z]].Ydirection = 0;

					}//if for testing distance

				}//end of for
				//} // end of if check same panel
			}// end of outer for loop	
		}//end of if(numP1Objects>1)
	}
	else if(gridDisplay == 3)
	{
		//check objects in panel 0 (upper left portion)
		if(numP0Objects > 1)
		{
			for(q=0; q < numP0Objects-1; q++)
			//if(p < trialParams.numObjects-1)
			{
				int z;
				for(z = q+1; z < numP0Objects; z++)
				{
					//if(trialParams.objInfo[q].objPanel == trialParams.objInfo[q].objPanel)
					//{
					distance = distanceBtwObjects(gbm[panel0Objects[q]], mySpeed[panel0Objects[q]].newX, mySpeed[panel0Objects[q]].newY, gbm[panel0Objects[z]], mySpeed[panel0Objects[z]].newX, mySpeed[panel0Objects[z]].newY);
			
					// two objects are too closed as minimum distance
					//while(distance <= (double)trialParams.minBounceDist)
					if((int)distance <= trialParams.minBounceDist)
					{
						//change horizontal direction for q
						if(mySpeed[panel0Objects[q]].Xdirection == 0)
							mySpeed[panel0Objects[q]].Xdirection = 1;
						else
							mySpeed[panel0Objects[q]].Xdirection = 0;

						//change vertical direction for q
						if(mySpeed[panel0Objects[q]].Ydirection == 0)
							mySpeed[panel0Objects[q]].Ydirection = 1;
						else
							mySpeed[panel0Objects[q]].Ydirection = 0;

						//change horizontal direction for z
						if(mySpeed[panel0Objects[z]].Xdirection == 0)
							mySpeed[panel0Objects[z]].Xdirection = 1;
						else
							mySpeed[panel0Objects[z]].Xdirection = 0;

						 //change vertical direction for z
						if(mySpeed[panel0Objects[z]].Ydirection == 0)
							mySpeed[panel0Objects[z]].Ydirection = 1;
						else
							mySpeed[panel0Objects[z]].Ydirection = 0;

					}//if for testing distance
				}//end of for
					//} // end of if check same panel
			}// end of outer for loop	
		}//end if(numP0Objects>1)
		
		//check objects in panel 1 (upper right portion)
		if(numP1Objects > 1)
		{
			for(q=0; q < numP1Objects-1; q++)
			//if(p < trialParams.numObjects-1)
			{
				int z;
				for(z = q+1; z < numP1Objects; z++)
				{
					//if(trialParams.objInfo[q].objPanel == trialParams.objInfo[q].objPanel)
					//{
					distance = distanceBtwObjects(gbm[panel1Objects[q]], mySpeed[panel1Objects[q]].newX, mySpeed[panel1Objects[q]].newY, gbm[panel1Objects[z]], mySpeed[panel1Objects[z]].newX, mySpeed[panel1Objects[z]].newY);
			
					// two objects are too closed as minimum distance
					//while(distance <= (double)trialParams.minBounceDist)
					if((int)distance <= trialParams.minBounceDist)
					{
						//change horizontal direction for q
						if(mySpeed[panel1Objects[q]].Xdirection == 0)
							mySpeed[panel1Objects[q]].Xdirection = 1;
						else
							mySpeed[panel1Objects[q]].Xdirection = 0;

						//change vertical direction for q
						if(mySpeed[panel1Objects[q]].Ydirection == 0)
							mySpeed[panel1Objects[q]].Ydirection = 1;
						else
							mySpeed[panel1Objects[q]].Ydirection = 0;

					  //change horizontal direction for z
						if(mySpeed[panel1Objects[z]].Xdirection == 0)
							mySpeed[panel1Objects[z]].Xdirection = 1;
						else
							mySpeed[panel1Objects[z]].Xdirection = 0;

						 //change vertical direction for z
						if(mySpeed[panel1Objects[z]].Ydirection == 0)
							mySpeed[panel1Objects[z]].Ydirection = 1;
						else
							mySpeed[panel1Objects[z]].Ydirection = 0;

					}//if for testing distance

				}//end of for
				//} // end of if check same panel
			}// end of outer for loop	
		}//end of if(numP1Objects>1)

		//check objects in panel 2 (bottom left portion)
		if(numP2Objects > 1)
		{
			for(q=0; q < numP2Objects-1; q++)
			//if(p < trialParams.numObjects-1)
			{
				int z;
				for(z = q+1; z < numP2Objects; z++)
				{
					//if(trialParams.objInfo[q].objPanel == trialParams.objInfo[q].objPanel)
					//{
					distance = distanceBtwObjects(gbm[panel2Objects[q]], mySpeed[panel2Objects[q]].newX, mySpeed[panel2Objects[q]].newY, gbm[panel2Objects[z]], mySpeed[panel2Objects[z]].newX, mySpeed[panel2Objects[z]].newY);
		
					// two objects are too closed as minimum distance
					//while(distance <= (double)trialParams.minBounceDist)
					if((int)distance <= trialParams.minBounceDist)
					{
						//change horizontal direction for q
						if(mySpeed[panel2Objects[q]].Xdirection == 0)
							mySpeed[panel2Objects[q]].Xdirection = 1;
						else
							mySpeed[panel2Objects[q]].Xdirection = 0;

						//change vertical direction for q
						if(mySpeed[panel2Objects[q]].Ydirection == 0)
							mySpeed[panel2Objects[q]].Ydirection = 1;
						else
							mySpeed[panel2Objects[q]].Ydirection = 0;

					  //change horizontal direction for z
						if(mySpeed[panel2Objects[z]].Xdirection == 0)
							mySpeed[panel2Objects[z]].Xdirection = 1;
						else
							mySpeed[panel2Objects[z]].Xdirection = 0;

						 //change vertical direction for z
						if(mySpeed[panel2Objects[z]].Ydirection == 0)
							mySpeed[panel2Objects[z]].Ydirection = 1;
						else
							mySpeed[panel2Objects[z]].Ydirection = 0;

					}//if for testing distance

				}//end of for
				//} // end of if check same panel
			}// end of outer for loop	
		}//end if(numP3Objects>1)
		
		//check objects in panel 3 (bottom right portion)
		if(numP3Objects > 1)
		{
			for(q=0; q < numP3Objects-1; q++)
			//if(p < trialParams.numObjects-1)
			{
				int z;
				for(z = q+1; z < numP3Objects; z++)
				{
					//if(trialParams.objInfo[q].objPanel == trialParams.objInfo[q].objPanel)
					//{
					distance = distanceBtwObjects(gbm[panel3Objects[q]], mySpeed[panel3Objects[q]].newX, mySpeed[panel3Objects[q]].newY, gbm[panel3Objects[z]], mySpeed[panel3Objects[z]].newX, mySpeed[panel3Objects[z]].newY);
	
					// two objects are too closed as minimum distance
					//while(distance <= (double)trialParams.minBounceDist)
					if((int)distance <= trialParams.minBounceDist)
					{
						//change horizontal direction for q
						if(mySpeed[panel3Objects[q]].Xdirection == 0)
							mySpeed[panel3Objects[q]].Xdirection = 1;
						else
							mySpeed[panel3Objects[q]].Xdirection = 0;

						//change vertical direction for q
						if(mySpeed[panel3Objects[q]].Ydirection == 0)
							mySpeed[panel3Objects[q]].Ydirection = 1;
						else
							mySpeed[panel3Objects[q]].Ydirection = 0;

					  //change horizontal direction for z
						if(mySpeed[panel3Objects[z]].Xdirection == 0)
							mySpeed[panel3Objects[z]].Xdirection = 1;
						else
							mySpeed[panel3Objects[z]].Xdirection = 0;

						 //change vertical direction for z
						if(mySpeed[panel3Objects[z]].Ydirection == 0)
							mySpeed[panel3Objects[z]].Ydirection = 1;
						else
							mySpeed[panel3Objects[z]].Ydirection = 0;
					}//if for testing distance

				}//end of for
			//} // end of if check same panel
			}// end of outer for loop	
		}//end of if(numP3Objects>1)
	}
}

//Move the objects.  Occluding case if there is no handle for bouncing case.
void moveObjects(HBITMAP *gbm, int gridDisplay, int p)
{
	if(gridDisplay == 0)
	{
		//object moving from left to right within screen resolution X.
		if(((mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) <= sessionParams.displayX)&&
		   (mySpeed[p].Xdirection == 0))			
		{					
			// no bouncing; objects will overlap each other.
			mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;
			mySpeed[p].Xdirection = 0;
		}
		else //object moving from right to left within screen resolution X.
		{
			if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
				mySpeed[p].Xdirection = 0;
			else
			{
				mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;
				mySpeed[p].Xdirection = 1;
			}

		}
	  //object moving from top to bottom within screen resolution Y.
		if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
			(mySpeed[p].Ydirection == 0))
		{
			mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;
			mySpeed[p].Ydirection = 0;
		}
		
		else //object moving from bottom to top within screen resolution Y.
		{
			if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
				mySpeed[p].Ydirection = 0;
			else
			{
				mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;
				mySpeed[p].Ydirection = 1;
			}
		 }
	}
	else if(gridDisplay == 1)
	{
		//object moving from left to right within screen resolution X.
		if(((mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) <= sessionParams.displayX)&&
		   (mySpeed[p].Xdirection == 0))			
		{
			mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;
			mySpeed[p].Xdirection = 0;
		}
		else //object moving from right to left within screen resolution X.
		{
			if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
				mySpeed[p].Xdirection = 0;
			else
			{
				mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;
				mySpeed[p].Xdirection = 1;
			}
		}
		//object in upper portion
		if(trialParams.objInfo[p].objPanel == 0)
		{
			 //object moving from top to bottom within upper portion of screen resolution Y.
			if(((int)(mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) < ((sessionParams.displayY/2)-(DIVIDER_THICKNESS/2)))&&
				(mySpeed[p].Ydirection == 0))
			{
				mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;
				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within upper portion of screen resolution Y.
			{
				if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
					mySpeed[p].Ydirection = 0;
				else
				{
					mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;
					mySpeed[p].Ydirection = 1;
				}
			}
		}
		//object in bottom portion
		else if(trialParams.objInfo[p].objPanel == 1)
		{
			//object moving from top to bottom within bottom portion of screen resolution Y.
			if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
				(mySpeed[p].Ydirection == 0))
			{
				mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;
				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within bottom portion of screen resolution Y.
			{
				if((int)(mySpeed[p].newY - mySpeed[p].speedY) < ((sessionParams.displayY/2)+(DIVIDER_THICKNESS/2)))
					mySpeed[p].Ydirection = 0;
				else
				{
					mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;
					mySpeed[p].Ydirection = 1;
				}
			}
		}
	}
	else if(gridDisplay == 2)
	{
		//object in left portion
		if(trialParams.objInfo[p].objPanel == 0)
		{
			//object moving from left to right within left portion of screen resolution X.
			if(((int)(mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) < ((sessionParams.displayX/2)-(DIVIDER_THICKNESS/2)))&&
			   (mySpeed[p].Xdirection == 0))			
			{
				mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;
				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within left portion of screen resolution X.
			{
			
				if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
					mySpeed[p].Xdirection = 0;
				else
				{
					mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;
					mySpeed[p].Xdirection = 1;
				}
			}
		}
		//object in right portion
		else if(trialParams.objInfo[p].objPanel == 1)
		{
			//object moving from left to right within right portion of screen resolution X.
			if(((mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) <= sessionParams.displayX)&&
			   (mySpeed[p].Xdirection == 0))			
			{
				mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;
				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within right portion of screen resolution X.
			{
				if((int)(mySpeed[p].newX - mySpeed[p].speedX) < ((sessionParams.displayX/2)+(DIVIDER_THICKNESS/2)))
					mySpeed[p].Xdirection = 0;
				else
				{
					mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;
					mySpeed[p].Xdirection = 1;
				}
			}
		}

	  //object moving from top to bottom within screen resolution Y.
		if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
			(mySpeed[p].Ydirection == 0))
		{
			mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;
			mySpeed[p].Ydirection = 0;
		}
		else //object moving from bottom to top within screen resolution Y.
		{
			if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
				mySpeed[p].Ydirection = 0;
			else
			{
				mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;
				mySpeed[p].Ydirection = 1;
			}
		}
	}
	else if(gridDisplay == 3)
	{
		//object in upper left or bottom left portion
		if((trialParams.objInfo[p].objPanel == 0)||(trialParams.objInfo[p].objPanel == 2))
		{
			//object moving from left to right within upper left or bottom left portion of screen resolution X.
			if(((int)(mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) < ((sessionParams.displayX/2)-(DIVIDER_THICKNESS/2)))&&
			   (mySpeed[p].Xdirection == 0))			
			{
				mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;
				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within upper left portion of screen resolution X.
			{
				if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
					mySpeed[p].Xdirection = 0;
				else
				{
					mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;
					mySpeed[p].Xdirection = 1;
				}
			}
		}

		//object in upper right or bottom right portion
		else if((trialParams.objInfo[p].objPanel == 1)||(trialParams.objInfo[p].objPanel == 3))
		{
			//object moving from left to right within upper right or bottom right portion of screen resolution X.
			if(((mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) <= sessionParams.displayX)&&
			   (mySpeed[p].Xdirection == 0))			
			{
				mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;
				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within upper right or bottom right portion of screen resolution X.
			{
				if((int)(mySpeed[p].newX - mySpeed[p].speedX) < ((sessionParams.displayX/2)+(DIVIDER_THICKNESS/2)))
					mySpeed[p].Xdirection = 0;
				else
				{
					mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;
					mySpeed[p].Xdirection = 1;
				}
			}
		}
	
		//object in upper left or upper right portion
		if((trialParams.objInfo[p].objPanel == 0)||(trialParams.objInfo[p].objPanel == 1))
		{
			 //object moving from top to bottom within upper left or upper right portion of screen resolution Y.
			if(((int)(mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) < ((sessionParams.displayY/2)-(DIVIDER_THICKNESS/2)))&&
				(mySpeed[p].Ydirection == 0))
			{
				mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;
				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within upper left or upper right portion of screen resolution Y.
			{
				if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
					mySpeed[p].Ydirection = 0;
				else
				{
					mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;
					mySpeed[p].Ydirection = 1;
				}
			}
		}
		//object in bottom left or bottom right portion
		else if((trialParams.objInfo[p].objPanel == 2)||(trialParams.objInfo[p].objPanel == 3))
		{
			//object moving from top to bottom within bottom left or bottom right portion of screen resolution Y.
			if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
				(mySpeed[p].Ydirection == 0))
			{
				mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;
				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within bottom left or bottom right portion of screen resolution Y.
			{
				if((int)(mySpeed[p].newY - mySpeed[p].speedY) < ((sessionParams.displayY/2)+(DIVIDER_THICKNESS/2)))
					mySpeed[p].Ydirection = 0;
				else
				{
					mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;
					mySpeed[p].Ydirection = 1;
				}
			}
		}
	}
}

/*
//Wait for start of next CRT retrace
void WaitForRetrace()
{
	int vert;

	do 
	{
		vert = inportb(0x3DA) & 8;
	}
	while (vert != 0);
	do 
	{
		vert = inportb(0x3DA) & 8;
	}
	while (vert == 0);
}
*/