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
/* - added draw_screen_divider()                         */
/* - added average_color()                               */
/* - added draw_fixation_marker()                        */
/* - added struct objIniPosition *objInitPos()           */
/* - added currentRandomPosition currObjPos()            */
/* - added distanceBtwObjects()                          */
/* - added computeBounce()                               */
/* - added isObjectClosedToOther()                       */
/* - added moveObjects()                                 */
/* - added randomSpeed()                                 */
/* - added randomDirection()                             */
/* - added setupInitMoveObjects()                        */
/*                                                       */
/* Author: Bahar Akbal, Elena Khan                       */
/* - added *selectTargetObjects()                        */
/* - added writeToGELogFile()                            */
/* - changed FRAME_RATE from 120 to 85 (esk 20070524)    */
/*********************************************************/

#include <windows.h>
#include <windowsx.h>
#include <math.h>
#include <stdio.h>   //file io
#include <string.h>
#include <stdlib.h>

#include "eyelink.h"
#include "w32_exptsppt2.h"
#include "w32_demo.h"       /* header file for this experiment */
#include "interpreter.h"

#ifndef EVSMOUSE		//esk - added ifndef block
#include "evsmouse.h"	
#endif

#define DIVIDER_THICKNESS 6  
#define FRAME_RATE 85.0
#define ONE_THOUSAND 1000
#define TRIALNAME_MAX 117 // maximum full trial name (i.e. Exp1_Sess1_Tr2) length is 117 characters
#define DIRECTORY_MAX 137
#define FILENAME_MAX 260 // maximum gelog file name with full path 
						// (i.e. C:\EVS\experiments\Exp1\Sess1\Tr1\Exp1_Sess1_Tr1.gelog) length is 260 characters

MSG msg; // contains message information from the current 
		 // thread's message queue (for object selection)

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
struct speedPos *mySpeed;
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
  int m, n, p, r, s, t;          //for keep track of the loop
  struct objIniPosition *obj_ini_pos;
  
  int numSamples;      //number of positions for moving objects
  int first = 1;         //(first == 1) means first time is true. (first == 0) means second or more time 

   /*** Get the full trial name to be fed into logger as the gelog file name later ***/
  char trialName[TRIALNAME_MAX] = "";
  char sessionNumberBuffer[3]; // 3 digit buffer for the session number
  char trialNumberBuffer[3]; // 3 digit buffer for the trial number

  strcat(trialName, sessionParams.expName);
  strcat(trialName, "_Sess");
  sprintf(sessionNumberBuffer, "%d", sessionParams.sessNum);
  strcat(trialName, sessionNumberBuffer);
  strcat(trialName, "_Tr");
  sprintf(trialNumberBuffer, "%d", trialParams.trialNum);
  strcat(trialName, trialNumberBuffer);

  //for moving objects positions
  moveObjPos = malloc(sizeof(struct objectPositions) * trialParams.numObjects);
 
  //objects will start to move after 4000ms; each object position will be recorded. 
  numSamples = (duration)/2;

  mySpeed = malloc(sizeof(struct speedPos)* trialParams.numObjects);
  obj_ini_pos = objInitPos(trialNum, trialParams.numObjects, displayX, displayY, gbm, gridDisplay);
  
  for(r = 0; r < trialParams.numObjects; r++)
  {
		moveObjPos[r].objAllPos = malloc(sizeof(struct allPositions) * numSamples);
  }

  s = 0;   //index for all moving objects positions

 
    // NOTE: TRIALID AND TITLE MUST HAVE BEEN SET BEFORE DRIFT CORRECTION!
    // FAILURE TO INCLUDE THESE MAY CAUSE INCOMPATIBILITIES WITH ANALYSIS SOFTWARE!

  eyemsg_printf("TRIALID%d EVStime=%d",trialNum,current_time());	//esk-added 20070518

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
  
  for(k=0; k < trialParams.numObjects; k++)
  {
	  //last 2 params are location x,y positions
	display_bitmap(full_screen_window, gbm[k], obj_ini_pos[k].x, obj_ini_pos[k].y);  // COPY BITMAP to display
	drawing_time = current_msec()-drawing_time;    // delay from retrace (time to draw)
  }
 
  //setup for moving objects
  setupInitMoveObjects(gridDisplay, obj_ini_pos);
 
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
 
  brush = CreateSolidBrush(bgColor);
  pen = GetStockObject(NULL_PEN);
 
  // Trial loop: till timeout or response 
  while(1)   
    {                            // First, check if recording aborted 
      if((error=check_recording())!=0) return error;  

	   // start drawing horizontal and vertical line on the screen 
	  draw_screen_divider(gridDisplay, displayX, displayY, hdc, fgColor);

	  //draw fixation marker.
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
					display_bitmap(full_screen_window, gbm[n], obj_ini_pos[n].x, obj_ini_pos[n].y);  // COPY BITMAP to display
			}
	  }

	  //moving the objects
	  if(current_time() > trial_start+4000)
	  {
		  msec_delay((UINT32)(ONE_THOUSAND/FRAME_RATE));
          
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
			    SelectObject(hdc, pen);
				SelectObject(hdc, brush);
				Rectangle(hdc, (int)mySpeed[t].lastX, (int)mySpeed[t].lastY, (int)mySpeed[t].lastX+bitmap_width(gbm[t])+2, (int)mySpeed[t].lastY+bitmap_height(gbm[t])+2);
		  }
		 
		  for(p = 0; p < trialParams.numObjects; p++)
		  {
				moveObjPos[p].objAllPos[s].currentTime = current_time();
				moveObjPos[p].objAllPos[s].x = mySpeed[p].newX;
				moveObjPos[p].objAllPos[s].y = mySpeed[p].newY;

				display_bitmap(full_screen_window, gbm[p], (int)mySpeed[p].newX, (int)mySpeed[p].newY);  // COPY BITMAP to display
				
				mySpeed[p].lastX = mySpeed[p].newX;
				mySpeed[p].lastY = mySpeed[p].newY;
						
				moveObjects(gbm, gridDisplay, p);
		
		  }//end of for loop

		  s++;
	  }

      // Check if trial time limit expired
      if(current_time() > trial_start+duration)
        {
         //*****end of trial duration*******
	   		char* str = "object";
			char* sam = "number of samples";
			
			int* selectedObjects; // Holds Subject's target selections.
			evsShowMouse = EVSYESMOUSE;	// esk - tell GE to allow cursor display
			selectedObjects = selectTargetObjects(s, gbm, hdc); // Get the Subject's target selections. //esk-added hdc param
			evsShowMouse = EVSNOMOUSE;	// esk - disable cursor display again
			
			eyemsg_printf("TIMEOUT");    // message to log the timeout 
			writeToGELogFile(trialName, moveObjPos, s, selectedObjects); // flush trial data into the gelog file
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
    }                        // END OF RECORDING LOOP
	end_realtime_mode();      // safety cleanup code
	while(getkey());          // dump any accumulated key presses

			   // report response result: 0=timeout, else button number
	eyemsg_printf("TRIAL_RESULT %d", button);
			   // Call this at the end of the trial, to handle special conditions
   
	DeleteObject(pen);   //delete the object for pen 
	DeleteObject(brush); 
	free(mySpeed);
	free(obj_ini_pos);
  	ReleaseDC(full_screen_window, hdc);
 
	return check_record_exit();
}

//Draw screen divider horizontal, vertical or both.
void draw_screen_divider(int lineType, int displayX, int displayY, HDC hdc, COLORREF fgColor)
{
	HPEN penFg;
	penFg = CreatePen(PS_SOLID, DIVIDER_THICKNESS, fgColor);
    SelectObject(hdc, penFg);
  
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

//Compute average color for drawing fixation marker
int average_color(int R, int G, int B)
{
	 int avg;

	 avg = (R + G + B)/3;

	 return avg;
}

//draw fixation marker at the center of the screen.
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
		
    startX = (displayX/2) - DIVIDER_THICKNESS;
	startY = (displayY/2) - DIVIDER_THICKNESS;
	endX = (displayX/2) + DIVIDER_THICKNESS;
	endY = (displayY/2) + DIVIDER_THICKNESS;

	MoveToEx (hdc, startX, startY, NULL);  //set the starting position
	LineTo (hdc, endX, endY);   //draw to end position

    startX = (displayX/2) + DIVIDER_THICKNESS;
	endX = (displayX/2) - DIVIDER_THICKNESS;

	MoveToEx (hdc, startX, startY, NULL);  //set the starting position
	LineTo (hdc, endX, endY);   //draw to end position
}

//Store initial position for all objects.
struct objIniPosition *objInitPos(int trialNum, int numObjects, int displayX, int displayY, HBITMAP *gbm, int gridDisplay)
{
	struct objIniPosition *op; 
	struct currentRandomPosition curRdPos;
	int bitmapWidth, bitmapHeight;
    int i, j, k;
	int distance;
	int newCurrentPos = 0;

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
			if(trialParams.objInfo[i].objPanel == 1)
			{
				y = rand()% (((displayY/2)-((DIVIDER_THICKNESS+1)/2))-bitmapHeight);
			}
			//If the object is in bottom portion
			else if(trialParams.objInfo[i].objPanel == 2)
			{
				y = rand() % ((displayY/2)+((DIVIDER_THICKNESS+1)/2))+ ((displayY/2)+((DIVIDER_THICKNESS+1)/2));
			}
		}while( (y+bitmapHeight) > displayY);
	}
	//One vertical screen divider
	else if(gridDisplay == 2)
	{
		do
		{
			//If object is in left portion.
			if(trialParams.objInfo[i].objPanel == 1)
			{
				x = rand() % (((displayX/2)-((DIVIDER_THICKNESS+1)/2))- bitmapWidth);
			}
			//If object is in right portion.
			else if(trialParams.objInfo[i].objPanel == 2)
			{
				x = rand() % ((displayX/2)+((DIVIDER_THICKNESS+1)/2))+((displayX/2)+((DIVIDER_THICKNESS+1)/2));
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
			if((trialParams.objInfo[i].objPanel == 1)||(trialParams.objInfo[i].objPanel == 3))
			{
				x = rand() % (((displayX/2)-((DIVIDER_THICKNESS+1)/2))- bitmapWidth);
			}
			//If object is in upper-right or bottom-right
			else if((trialParams.objInfo[i].objPanel == 2)||(trialParams.objInfo[i].objPanel == 4))
			{
				x = rand() % ((displayX/2)+((DIVIDER_THICKNESS+1)/2))+((displayX/2)+((DIVIDER_THICKNESS+1)/2));
			}
							
		}while( (x+bitmapWidth) > displayX); 

		do
		{
			//If object is in upper-left or upper-right
			if((trialParams.objInfo[i].objPanel == 1)||(trialParams.objInfo[i].objPanel == 2))
			{
				y = rand()% (((displayY/2)-((DIVIDER_THICKNESS+1)/2))-bitmapHeight);
			}
			//If object is in bottom-left or bottom-right.
			else if((trialParams.objInfo[i].objPanel == 3)||(trialParams.objInfo[i].objPanel == 4))
			{
				y = rand() % ((displayY/2)+((DIVIDER_THICKNESS+1)/2))+ ((displayY/2)+((DIVIDER_THICKNESS+1)/2));
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
//return 1 to compute bounce.
//return 0 not to compute bounce (stay in same position)
int computeBounce(HBITMAP *gbm, int gridDisplay, int p)
{
	int i;
	int tempXdirection;
	int tempYdirection;
	double tempXspeed;
	double tempYspeed;
	double tempX;
	double tempY;
	int isClosedToOther;

	isClosedToOther = isObjectClosedToOther(gbm, gridDisplay, p, mySpeed[p].newX, mySpeed[p].newY);
	
	//object is too closed to other.
	if(isClosedToOther)
	{
		for(i = 0; i < 50; i++)
		{
			tempXdirection = randomDirection();
			tempYdirection = randomDirection();
			tempXspeed = (double)(randomSpeed(trialParams.objInfo[p].objMinSpeedH, trialParams.objInfo[p].objMaxSpeedH));
			tempYspeed = (double)(randomSpeed(trialParams.objInfo[p].objMinSpeedV, trialParams.objInfo[p].objMaxSpeedV));

			tempXspeed = tempXspeed / FRAME_RATE;
			tempYspeed = tempYspeed / FRAME_RATE;

			if(tempXdirection == 0)
				tempX = mySpeed[p].newX + tempXspeed;
			else if(tempXdirection == 1)
				tempX = mySpeed[p].newX - tempXspeed;

			if(tempYdirection == 0)
				tempY = mySpeed[p].newY + tempYspeed;
			else
				tempY = mySpeed[p].newY - tempYspeed;

			isClosedToOther = isObjectClosedToOther(gbm, gridDisplay, p, tempX, tempY);
	
			if(!isClosedToOther)
			{
				//No divider.
				if(gridDisplay == 0)
				{
					//check screen boundry
					if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= sessionParams.displayX))||
					    ((tempXdirection == 1) && (tempX >= 0 ))) &&
					   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= sessionParams.displayY))||
					    ((tempYdirection == 1) && (tempY >= 0))))
					{
						mySpeed[p].Xdirection = tempXdirection;
						mySpeed[p].Ydirection = tempYdirection;
						mySpeed[p].speedX = tempXspeed;
						mySpeed[p].speedY = tempYspeed;
						return 1;
					}//end if
				}
				//horizontal divider
				else if(gridDisplay == 1)
				{
					//object in upper portion
					if(trialParams.objInfo[p].objPanel == 1)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= sessionParams.displayX))||
						    ((tempXdirection == 1) && (tempX >= 0 ))) &&
						   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= ((sessionParams.displayY/2)-((DIVIDER_THICKNESS+2)/2))))||
							((tempYdirection == 1) && (tempY >= 0))))
						{
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if
					}

					//object in bottom portion
					else if(trialParams.objInfo[p].objPanel == 2)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= sessionParams.displayX))||
						    ((tempXdirection == 1) && (tempX >= 0 ))) &&
						   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= sessionParams.displayY))||
							((tempYdirection == 1) && ((tempY+bitmap_height(gbm[p])) >= ((sessionParams.displayY/2)+((DIVIDER_THICKNESS+2)/2)))) ))
						{
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if
					}//end if
				}

				//vertical divider
				else if(gridDisplay == 2)
				{
					//object in left portion
					if(trialParams.objInfo[p].objPanel == 1)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= ((sessionParams.displayX/2)-((DIVIDER_THICKNESS+2)/2))))||
							((tempXdirection == 1) && (tempX >= 0 ))) &&
						   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= sessionParams.displayY))||
							((tempYdirection == 1) && (tempY >= 0))))
						{
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if
					}
					//object in right portion
					else if(trialParams.objInfo[p].objPanel == 2)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= sessionParams.displayX))||
							((tempXdirection == 1) && (tempX >= ((sessionParams.displayX/2)+((DIVIDER_THICKNESS+2)/2))))) &&
						   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= sessionParams.displayY))||
							((tempYdirection == 1) && (tempY >= 0))))
						{
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if
					}

				}

				//both horizontal and vertical dividers
				else if(gridDisplay == 3)
				{
					//object in upper left portion
					if(trialParams.objInfo[p].objPanel == 1)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= ((sessionParams.displayX/2)-((DIVIDER_THICKNESS+2)/2))))||
							((tempXdirection == 1) && (tempX >= 0 ))) &&
						   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= ((sessionParams.displayY/2)-((DIVIDER_THICKNESS+2)/2))))||
							((tempYdirection == 1) && (tempY >= 0))))
						{
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if

					}

					//object in upper right portion
					else if(trialParams.objInfo[p].objPanel == 2)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= sessionParams.displayX))||
							((tempXdirection == 1) && (tempX >= ((sessionParams.displayX/2)+((DIVIDER_THICKNESS+2)/2))))) &&
						   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= ((sessionParams.displayY/2)-((DIVIDER_THICKNESS+2)/2))))||
							((tempYdirection == 1) && (tempY >= 0))))
						{
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if
					}
					
					//object in bottom left portion
					else if(trialParams.objInfo[p].objPanel == 3)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= ((sessionParams.displayX/2)-((DIVIDER_THICKNESS+2)/2))))||
							((tempXdirection == 1) && (tempX >= 0 ))) &&
						   (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= sessionParams.displayY))||
							((tempYdirection == 1) && ((tempY+bitmap_height(gbm[p])) >= ((sessionParams.displayY/2)+((DIVIDER_THICKNESS+2)/2)))) ))
                   		{
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if
					}

					//object in bottom right portion
					else if(trialParams.objInfo[p].objPanel == 4)
					{
						if((((tempXdirection == 0) && ((tempX+bitmap_width(gbm[p])) <= sessionParams.displayX))||
							((tempXdirection == 1) && (tempX >= ((sessionParams.displayX/2)+((DIVIDER_THICKNESS+2)/2))))) &&
						    (((tempYdirection == 0) && ((tempY+bitmap_height(gbm[p])) <= sessionParams.displayY))||
							((tempYdirection == 1) && ((tempY+bitmap_height(gbm[p])) >= ((sessionParams.displayY/2)+((DIVIDER_THICKNESS+2)/2)))) ))
                        {
							mySpeed[p].Xdirection = tempXdirection;
							mySpeed[p].Ydirection = tempYdirection;
							mySpeed[p].speedX = tempXspeed;
							mySpeed[p].speedY = tempYspeed;
							return 1;
						}//end if
					}
				}//end of (gridDisplay == 3)
			}//end if
		}//end for
		return 0;

	}//end if
	return 1;
}

//detect the bounce distance
//return 1 (true) if current object is too closed to one of the other objects
//Otherwise return 0 (false)
int isObjectClosedToOther(HBITMAP *gbm, int gridDisplay, int p, int newX, int newY)
{
    int q;
	double distance;

	for(q = 0; q < trialParams.numObjects; q++)
	{
		if(p != q)
		{
			distance = distanceBtwObjects(gbm[p], newX, newY, gbm[q], mySpeed[q].newX, mySpeed[q].newY);
			if(distance <= (double)trialParams.minBounceDist)
				return 1;
		}
	}
	return 0;
}

//Move the objects.  Occluding case if there is no handle for bouncing case.
void moveObjects(HBITMAP *gbm, int gridDisplay, int p)
{
	int doBounce = 0;

	if(trialParams.isBounce == 1)
		doBounce = computeBounce(gbm, gridDisplay, p);

	if(gridDisplay == 0)
	{
		//object moving from left to right within screen resolution X.
		if(((mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) <= sessionParams.displayX)&&
		   (mySpeed[p].Xdirection == 0))			
		{					
			// if no bouncing, objects will overlap each other.
			if(((trialParams.isBounce == 1)&&(doBounce == 1))||
			   (trialParams.isBounce == 0))
				mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;

			mySpeed[p].Xdirection = 0;
		}
		else //object moving from right to left within screen resolution X.
		{
			if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
				mySpeed[p].Xdirection = 0;
			else
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
					(trialParams.isBounce == 0))
					mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;

				mySpeed[p].Xdirection = 1;
			}
		}
	  //object moving from top to bottom within screen resolution Y.
		if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
			(mySpeed[p].Ydirection == 0))
		{
			if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				(trialParams.isBounce == 0))
				mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;

			mySpeed[p].Ydirection = 0;
		}
		
		else //object moving from bottom to top within screen resolution Y.
		{
			if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
				mySpeed[p].Ydirection = 0;
			else
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
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
			if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				(trialParams.isBounce == 0))
				mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;

			mySpeed[p].Xdirection = 0;
		}
		else //object moving from right to left within screen resolution X.
		{
			if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
				mySpeed[p].Xdirection = 0;
			else
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;

				mySpeed[p].Xdirection = 1;
			}
		}
		//object in upper portion
		if(trialParams.objInfo[p].objPanel == 1)
		{
			 //object moving from top to bottom within upper portion of screen resolution Y.
			if(((int)(mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) < ((sessionParams.displayY/2)-((DIVIDER_THICKNESS+2)/2)))&&
				(mySpeed[p].Ydirection == 0))
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;

				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within upper portion of screen resolution Y.
			{
				if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
					mySpeed[p].Ydirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;

					mySpeed[p].Ydirection = 1;
				}
			}
		}
		//object in bottom portion
		else if(trialParams.objInfo[p].objPanel == 2)
		{
			//object moving from top to bottom within bottom portion of screen resolution Y.
			if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
				(mySpeed[p].Ydirection == 0))
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
			       (trialParams.isBounce == 0))
					mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;

				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within bottom portion of screen resolution Y.
			{
				if((int)(mySpeed[p].newY - mySpeed[p].speedY) < ((sessionParams.displayY/2)+((DIVIDER_THICKNESS+2)/2)))
					mySpeed[p].Ydirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;

					mySpeed[p].Ydirection = 1;
				}
			}
		}
	}
	else if(gridDisplay == 2)
	{
		//object in left portion
		if(trialParams.objInfo[p].objPanel == 1)
		{
			//object moving from left to right within left portion of screen resolution X.
			if(((int)(mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) < ((sessionParams.displayX/2)-((DIVIDER_THICKNESS+2)/2)))&&
			   (mySpeed[p].Xdirection == 0))			
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;

				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within left portion of screen resolution X.
			{
				if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
					mySpeed[p].Xdirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;

					mySpeed[p].Xdirection = 1;
				}
			}
		}
		//object in right portion
		else if(trialParams.objInfo[p].objPanel == 2)
		{
			//object moving from left to right within right portion of screen resolution X.
			if(((mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) <= sessionParams.displayX)&&
			   (mySpeed[p].Xdirection == 0))			
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;

				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within right portion of screen resolution X.
			{
				if((int)(mySpeed[p].newX - mySpeed[p].speedX) < ((sessionParams.displayX/2)+((DIVIDER_THICKNESS+2)/2)))
					mySpeed[p].Xdirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;

					mySpeed[p].Xdirection = 1;
				}
			}
		}

	  //object moving from top to bottom within screen resolution Y.
		if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
			(mySpeed[p].Ydirection == 0))
		{
			if(((trialParams.isBounce == 1)&&(doBounce == 1))||
			   (trialParams.isBounce == 0))
				mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;

			mySpeed[p].Ydirection = 0;
		}
		else //object moving from bottom to top within screen resolution Y.
		{
			if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
				mySpeed[p].Ydirection = 0;
			else
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;

				mySpeed[p].Ydirection = 1;
			}
		}
	}
	else if(gridDisplay == 3)
	{
		//object in upper left or bottom left portion
		if((trialParams.objInfo[p].objPanel == 1)||(trialParams.objInfo[p].objPanel == 3))
		{
			//object moving from left to right within upper left or bottom left portion of screen resolution X.
			if(((int)(mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) < ((sessionParams.displayX/2)-((DIVIDER_THICKNESS+2)/2)))&&
			   (mySpeed[p].Xdirection == 0))			
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;

				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within upper left portion of screen resolution X.
			{
				if((mySpeed[p].newX - mySpeed[p].speedX) < 0)
					mySpeed[p].Xdirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;

					mySpeed[p].Xdirection = 1;
				}
			}
		}

		//object in upper right or bottom right portion
		else if((trialParams.objInfo[p].objPanel == 2)||(trialParams.objInfo[p].objPanel == 4))
		{
			//object moving from left to right within upper right or bottom right portion of screen resolution X.
			if(((mySpeed[p].newX + mySpeed[p].speedX+bitmap_width(gbm[p])) <= sessionParams.displayX)&&
			   (mySpeed[p].Xdirection == 0))			
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newX = mySpeed[p].newX + mySpeed[p].speedX;

				mySpeed[p].Xdirection = 0;
			}
			else //object moving from right to left within upper right or bottom right portion of screen resolution X.
			{
				if((int)(mySpeed[p].newX - mySpeed[p].speedX) < ((sessionParams.displayX/2)+((DIVIDER_THICKNESS+2)/2)))
					mySpeed[p].Xdirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newX = mySpeed[p].newX - mySpeed[p].speedX;

					mySpeed[p].Xdirection = 1;
				}
			}
		}
	
		//object in upper left or upper right portion
		if((trialParams.objInfo[p].objPanel == 1)||(trialParams.objInfo[p].objPanel == 2))
		{
			 //object moving from top to bottom within upper left or upper right portion of screen resolution Y.
			if(((int)(mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) < ((sessionParams.displayY/2)-((DIVIDER_THICKNESS+2)/2)))&&
				(mySpeed[p].Ydirection == 0))
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;

				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within upper left or upper right portion of screen resolution Y.
			{
				if((mySpeed[p].newY - mySpeed[p].speedY) < 0)
					mySpeed[p].Ydirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;

					mySpeed[p].Ydirection = 1;
				}
			}
		}
		//object in bottom left or bottom right portion
		else if((trialParams.objInfo[p].objPanel == 3)||(trialParams.objInfo[p].objPanel == 4))
		{
			//object moving from top to bottom within bottom left or bottom right portion of screen resolution Y.
			if(((mySpeed[p].newY + mySpeed[p].speedY+bitmap_height(gbm[p])) <= sessionParams.displayY)&&
				(mySpeed[p].Ydirection == 0))
			{
				if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				   (trialParams.isBounce == 0))
					mySpeed[p].newY = mySpeed[p].newY + mySpeed[p].speedY;

				mySpeed[p].Ydirection = 0;
			}
			else //object moving from bottom to top within bottom left or bottom right portion of screen resolution Y.
			{
				if((int)(mySpeed[p].newY - mySpeed[p].speedY) < ((sessionParams.displayY/2)+((DIVIDER_THICKNESS+2)/2)))
					mySpeed[p].Ydirection = 0;
				else
				{
					if(((trialParams.isBounce == 1)&&(doBounce == 1))||
				       (trialParams.isBounce == 0))
						mySpeed[p].newY = mySpeed[p].newY - mySpeed[p].speedY;

					mySpeed[p].Ydirection = 1;
				}
			}
		}
	}
}

//compute random speed between minimum and maximum speed
int randomSpeed(int minSpeed, int maxSpeed)
{
	int r = rand()% (maxSpeed - 1)+ minSpeed;
	return r;
}

//compute random direction 0 or 1.
int randomDirection()
{
	int dir = rand() % 2;
	return dir;
}

//setup the new position and direction for moving objects.
void setupInitMoveObjects(int gridDisplay, struct objIniPosition *obj_ini_pos)
{
	int w;
	for(w=0; w < trialParams.numObjects; w++)
	{
		if(gridDisplay == 0)
		{
			do{
				//find random direction (0 == move to right) , (1 == move to left)
				mySpeed[w].Xdirection = randomDirection();
	
				//find the random speed within minimum and maximum horizontal speeds.
				mySpeed[w].speedX = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedH, trialParams.objInfo[w].objMaxSpeedH));
				mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

				mySpeed[w].lastX = (double)obj_ini_pos[w].x;
				if(mySpeed[w].Xdirection == 0)
					mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
				else 
					mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
			}while((mySpeed[w].newX > sessionParams.displayX)||(mySpeed[w].newX < 0));

			do{
				//find random direction (0 == move to bottom), (1 == move to top)
				mySpeed[w].Ydirection = randomDirection();

				//find the random speed within minimum and maximum vertical speeds.
				mySpeed[w].speedY = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedV, trialParams.objInfo[w].objMaxSpeedV));
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
			do{
				//find random direction (0 == move to right) , (1 == move to left)
				mySpeed[w].Xdirection = randomDirection();

				//find the random speed within minimum and maximum horizontal speeds.
				mySpeed[w].speedX = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedH, trialParams.objInfo[w].objMaxSpeedH));
				mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

				mySpeed[w].lastX = (double)obj_ini_pos[w].x;
				if(mySpeed[w].Xdirection == 0)
					mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
				else 
					mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
			}while((mySpeed[w].newX > sessionParams.displayX)||(mySpeed[w].newX < 0));

			//upper portion
			if(trialParams.objInfo[w].objPanel == 1)
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					mySpeed[w].Ydirection = randomDirection();

					//find the random speed within minimum and maximum vertical speeds.
					mySpeed[w].speedY = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedV, trialParams.objInfo[w].objMaxSpeedV));
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > ((sessionParams.displayY/2)-((DIVIDER_THICKNESS+1)/2)))||(mySpeed[w].newY < 0));
			}
            // bottom portion
			else if(trialParams.objInfo[w].objPanel == 2)
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					mySpeed[w].Ydirection = randomDirection();

					//find the random speed within minimum and maximum vertical speeds.
				    mySpeed[w].speedY = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedV, trialParams.objInfo[w].objMaxSpeedV));
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > sessionParams.displayY)||((int)mySpeed[w].newY < ((sessionParams.displayY/2)+((DIVIDER_THICKNESS+1)/2))));
			}		
		}
		//vertical divider
		else if(gridDisplay == 2)
		{
			//left portion
			if(trialParams.objInfo[w].objPanel == 1)
			{
				do{
					//find random direction (0 == move to right) , (1 == move to left)
					mySpeed[w].Xdirection = randomDirection();

					//find the random speed within minimum and maximum horizontal speeds.
					mySpeed[w].speedX = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedH, trialParams.objInfo[w].objMaxSpeedH));
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > ((sessionParams.displayX/2)-((DIVIDER_THICKNESS+1)/2)))||(mySpeed[w].newX < 0));
			}

			//right portion
			else if(trialParams.objInfo[w].objPanel == 2)
			{
				do{
					//find random direction (0 == move to right) , (1 == move to left)
					mySpeed[w].Xdirection = randomDirection();

					//find the random speed within minimum and maximum horizontal speeds.
					mySpeed[w].speedX = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedH, trialParams.objInfo[w].objMaxSpeedH));
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > sessionParams.displayX)||((int)mySpeed[w].newX < ((sessionParams.displayX/2)+((DIVIDER_THICKNESS+1)/2))));
			}

			do{
				//find random direction (0 == move to bottom), (1 == move to top)
				mySpeed[w].Ydirection = randomDirection();

				//find the random speed within minimum and maximum vertical speeds.
				mySpeed[w].speedY = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedV, trialParams.objInfo[w].objMaxSpeedV));
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
			//check the X coordinate for vertical divider; objects in left portion
			if((trialParams.objInfo[w].objPanel == 1)||(trialParams.objInfo[w].objPanel == 3))
			{
				do{
					//find random direction (0 == move to right) , (1 == move to left)
					mySpeed[w].Xdirection = randomDirection();

					//find the random speed within minimum and maximum horizontal speeds.
				    mySpeed[w].speedX = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedH, trialParams.objInfo[w].objMaxSpeedH));
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > ((sessionParams.displayX/2)-((DIVIDER_THICKNESS+1)/2)))||(mySpeed[w].newX < 0));
			}
			//object in right portion
			else if((trialParams.objInfo[w].objPanel == 2)||(trialParams.objInfo[w].objPanel == 4))
			{
				do{
					//find random direction (0 == move to right) , (1 == move to left)
					mySpeed[w].Xdirection = randomDirection();

					//find the random speed within minimum and maximum horizontal speeds.
					mySpeed[w].speedX = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedH, trialParams.objInfo[w].objMaxSpeedH));
					mySpeed[w].speedX = mySpeed[w].speedX / FRAME_RATE;

					mySpeed[w].lastX = (double)obj_ini_pos[w].x;
					if(mySpeed[w].Xdirection == 0)
						mySpeed[w].newX = (double)obj_ini_pos[w].x + mySpeed[w].speedX;
					else 
						mySpeed[w].newX = (double)obj_ini_pos[w].x - mySpeed[w].speedX;
	
				}while((mySpeed[w].newX > sessionParams.displayX)||((int)mySpeed[w].newX < ((sessionParams.displayX/2)+((DIVIDER_THICKNESS+1)/2))));
			}

			//check the Y coordinate for horizontal divider;object in upper portion
			if((trialParams.objInfo[w].objPanel == 1) || (trialParams.objInfo[w].objPanel == 2))
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					mySpeed[w].Ydirection = randomDirection();

					//find the random speed within minimum and maximum vertical speeds.
					mySpeed[w].speedY = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedV, trialParams.objInfo[w].objMaxSpeedV));
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > ((sessionParams.displayY/2)-((DIVIDER_THICKNESS+1)/2)))||(mySpeed[w].newY < 0));
			}
			//object in bottom portion
			else if((trialParams.objInfo[w].objPanel == 3) || (trialParams.objInfo[w].objPanel == 4))
			{
				do{
					//(0 == move to bottom), (1 == move to top)
					mySpeed[w].Ydirection = randomDirection();

					//find the random speed within minimum and maximum vertical speeds.
					mySpeed[w].speedY = (double)(randomSpeed(trialParams.objInfo[w].objMinSpeedV, trialParams.objInfo[w].objMaxSpeedV));
					mySpeed[w].speedY = mySpeed[w].speedY / FRAME_RATE;

					mySpeed[w].lastY = (double)obj_ini_pos[w].y;
					if(mySpeed[w].Ydirection == 0)
						mySpeed[w].newY = (double)obj_ini_pos[w].y + mySpeed[w].speedY;
					else
						mySpeed[w].newY = (double)obj_ini_pos[w].y - mySpeed[w].speedY;

				}while((mySpeed[w].newY > sessionParams.displayY)||((int)mySpeed[w].newY < ((sessionParams.displayY/2)+((DIVIDER_THICKNESS+1)/2))));
			}
		}
	}
}

//user select the target objects
int *selectTargetObjects(int sampleNumber, HBITMAP *gbm, HDC theHDC)
{
	int i, allObjects, selections, count = 0, submitTargets = 0;
	int s = sampleNumber-1;
	//HBITMAP *selectedBitmaps;
	int *selectedObjects;
	HCURSOR cursor = LoadCursor( NULL, IDC_ARROW );
	POINT pt;
	int click_x = 0, click_y = 0;
	int xLow, xHigh, yLow, yHigh;
	int isMarking;

	//esk - vars for marking objects
	HDC hdc = theHDC;				// the whole window

	HPEN plusSignPen;		        // use same pen (i.e., color & size) per trial
	int plusSignFracLength = 4;	    // for length of plussign lines
                                    // (i.e., 4 => plus-sign length will be 1/4 length of shorter bitmap dimension)
    int plusSignLineLength;			// actual length of plus-sign line
	int plusSignFracWidth = 5;	    // for width of plussign lines
	                                // (i.e., 5 => plus-sign width will be 1/5 size of shorter bitmap dimension -
	                                //        for 50-pixel sized bitmap, plus-sign width will be 10 pixels)
	int plusSignLineWidth;			// actual value width of plus-sign line

	int objMiddleX;					// center of bitmap
	int objMiddleY;                 // center of bitmap
	int bmpSmallerDimSize;          // size of smaller dimension of bitmap
	//esk - end vars for marking objects

	//esk - begin var for highlighting targets
	HPEN targetPen;
	int targetPenFracWidth = 5;
	int targetPenLineWidth;
    int loopIndex;
	int increaseRadiusSizeFrac = 5;	// increase radius of arc; else draws over obj
	int increaseRadiusSize;
	//esk - end var for highlighting targets

	//selectedBitmaps= malloc(sizeof(HBITMAP)* trialParams.numTargets);
	selectedObjects = malloc(sizeof(int)* trialParams.numTargets);
	
	SetCursor(cursor);

	while ( submitTargets == 0 )
	{
		ShowCursor(1);
		
		isMarking = 1;

		if (escape_pressed()) break;
		
		if (GetMessage(&msg, full_screen_window, 0, 0) > 0)
		{
			// Get the ENTER key press
			// Subject must press the "ENTER" key to submit his/her target selections.
			if(msg.message == WM_KEYDOWN)
			{
				if(msg.wParam == VK_RETURN)
				{
					//esk 20070521 - removing the pop-up box telling user that not all targets have been marked
					//   (1) can't figure out why the MessageBox code produces stack overflow
					//       "An unhandled exception of type 'System.StackOverflowException' occurred in Unknown Module."
					//   (2) MessageBox draws over (erases) anything beneath it.  If it pops up where an object is located,
					//       that object does not automatically get redrawn when the box disappears.
					//if(count < trialParams.numTargets)
					//	MessageBox(NULL, TEXT("You did not mark all targets."), "EyeVision Warning", MB_OK);
					//else

					if(count == trialParams.numTargets)
					{
						submitTargets = 1;
					}
				}
			}

			// Get the mouse-click on screen
			else if (msg.message == WM_LBUTTONDOWN) 
			{
				GetCursorPos(&pt);
				click_x = pt.x;
				click_y = pt.y;

				for(allObjects = 0; allObjects < trialParams.numObjects; allObjects++)
				{
					xLow = moveObjPos[allObjects].objAllPos[s].x;
					yLow = moveObjPos[allObjects].objAllPos[s].y;

					// assuming moveObjPos[i] and bitmap[i] are same:
					xHigh = xLow + bitmap_width( gbm[allObjects] );
					yHigh = yLow + bitmap_height( gbm[allObjects] );

					// Check if the clicked point corresponds to an object on the screen
					if( (xLow <= click_x) && (click_x <= xHigh) ) 
					{
						if( (yLow <= click_y) && (click_y <= yHigh) )
						{
							// Check if Subject is unmarking:
							for( selections = 0; selections < trialParams.numTargets; selections++ )
							{
								if( selectedObjects[selections] == allObjects+1)
								{
									//esk - begin unmarking
									//    - unmarking is just redrawing bitmap 
									//      (there might be display "problems" if there are overlapping objs
									//       i.e., an unmarked object is redrawn over the plus-sign of a chosen obj)
									display_bitmap(full_screen_window, gbm[allObjects], xLow, yLow);  // from bitmap_recording_trial()
									//esk - end unmarking
									
									isMarking = 0;
									count--;
									
									selectedObjects[selections] = NULL; // delete the unmarked object from the list
									// slide the next selectedObjects elements to the left
									for( i = selections; i < trialParams.numTargets; i++ )
									{
										if( (selectedObjects[i] == NULL) && (selectedObjects[i+1] != NULL) )
										{
											selectedObjects[i] = selectedObjects[i+1];
											selectedObjects[i+1] = NULL;
										}
									}
								}
							}

							// Subject is marking
							if(isMarking == 1 && count < trialParams.numTargets)
							{
								//esk - begin marking
								//      (required making hdc global so we could have access)
								
								//esk - need to know middle of obj in order to draw lines
								objMiddleX = xLow + (xHigh-xLow)/2;	
								objMiddleY = yLow + (yHigh-yLow)/2;

								//esk - find out which axis is smaller 
								if (bitmap_width(gbm[allObjects]) == bitmap_height(gbm[allObjects])) {
									bmpSmallerDimSize = bitmap_width(gbm[allObjects]);
								} else if (bitmap_width(gbm[allObjects]) < bitmap_height(gbm[allObjects])) {
									bmpSmallerDimSize = bitmap_width(gbm[allObjects]); 
								} else if (bitmap_height(gbm[allObjects]) < bitmap_width(gbm[allObjects])) {
									bmpSmallerDimSize = bitmap_height(gbm[allObjects]);
								}

								//esk - determine dimensions of plus-sign
								plusSignLineLength = bmpSmallerDimSize/plusSignFracLength;
								plusSignLineWidth = bmpSmallerDimSize/plusSignFracWidth;

								//esk - create pen for plus-sign
								plusSignPen = CreatePen(PS_SOLID, plusSignLineWidth, RGB(trialParams.bgColorR,trialParams.bgColorG,trialParams.bgColorB));

								//esk - associate pen with whole window (we're drawing lines on hdc, not on bitmap)
								SelectObject(hdc, plusSignPen);

								//esk - draw lines
								MoveToEx(hdc, objMiddleX - plusSignLineLength/2, objMiddleY, NULL); 
								LineTo(hdc, objMiddleX + plusSignLineLength/2, objMiddleY);
								MoveToEx(hdc, objMiddleX, objMiddleY - plusSignLineLength/2, NULL);
								LineTo(hdc, objMiddleX, objMiddleY + plusSignLineLength/2);
								
								//esk - end marking

								selectedObjects[count] = allObjects+1;
								//selectedBitmaps[count] = gbm[allObjects];
								count++;
							}
						}
					}
				}
			}
		}
	}

	DeleteObject(plusSignPen);		//esk - clean up pen after choosing is over

	// esk - begin show targets
	//http://msdn.microsoft.com/archive/default.asp?url=/archive/en-us/dnargdi/html/msdn_w32pens.asp
	// now we have to show the targets to the user
	targetPenLineWidth = plusSignLineWidth;
	//targetPen = CreatePen(PS_SOLID, targetPenLineWidth, RGB(trialParams.bgColorR,trialParams.bgColorG,trialParams.bgColorB));
	
	// bhr - If the background color is white, then mark targets with black,
	if(trialParams.bgColorR == 255)
	{
		if(trialParams.bgColorG == 255)
		{
			if(trialParams.bgColorB == 255)
				targetPen = CreatePen(PS_SOLID, targetPenLineWidth, RGB(0, 0, 0));
		}
	}
	// bhr - Other than that, default color for target marking is white.
	else
		targetPen = CreatePen(PS_SOLID, targetPenLineWidth, RGB(255, 255, 255));
	
	
	//--> fix hard-coding of rgb
	increaseRadiusSize = bmpSmallerDimSize / increaseRadiusSizeFrac;

	//hpen = CreatePen(PS_SOLID, 20, RGB(255, 0, 0));
	SelectObject(hdc, targetPen);
	//
	//Create the path for a circle.
	//

	for (loopIndex=0; loopIndex < trialParams.numObjects; loopIndex++) {	//esk-loop thru objects
		if(trialParams.objInfo[loopIndex].objIsTarget == 1) {
			//SelectObject(hdc, targetPen);
			//SelectObject(hdc, brush);

			//esk - assuming that (1) objects are stored in the same order (have the same obj# in .session and in gbm[])
			//      and (2) moveObjPos[i] and bitmap[i] are same (same assumption from user-choosing part)
			//    - If assumptions not correct, only display to user is affected
			
			int rectULx = moveObjPos[loopIndex].objAllPos[s].x; //esk-copied from above
			int rectULy = moveObjPos[loopIndex].objAllPos[s].y;
			int rectLRx = rectULx + bitmap_width( gbm[loopIndex] );
			int rectLRy = rectULy + bitmap_height( gbm[loopIndex] );
		
			//Rectangle(hdc, obj_ini_pos[m].x, obj_ini_pos[m].y, obj_ini_pos[m].x+bitmap_width(gbm[m])+1, obj_ini_pos[m].y+bitmap_height(gbm[m])+1);
			//Ellipse(hdc, rectULx, rectULy, rectLRx, rectLRy);
			Arc(hdc, rectULx-increaseRadiusSize, rectULy-increaseRadiusSize, rectLRx+increaseRadiusSize, rectLRy+increaseRadiusSize, 
				     rectULx-increaseRadiusSize, rectULy-increaseRadiusSize, rectULx-increaseRadiusSize, rectULy-increaseRadiusSize);
		}

	}

	//BeginPath(hdc);
	//Ellipse(hdc, 100, 100, 400, 400);
	//EndPath(hdc);
	//
	//This transform will only affect the pen shape!
	//The pen shape will be elliptical.
	//
	//SetMapMode(hdc, MM_ANISOTROPIC);
	//SetWindowExtEx(hdc, 1, 5, NULL);
	//
	//Draw the circle with the elliptical pen.
	//
	//StrokePath(hdc);

	DeleteObject(targetPen);

	// esk - end show targets

	MessageBox(NULL, TEXT("Target selection is done!"), "EyeVision", MB_OK);

	return selectedObjects;
}

//write moving objects data to GE log file
void writeToGELogFile(char *trialName, struct objectPositions *moveObjPos, int samples, int *selectedObjects)
{
	FILE *gelog;
	char gelogFileName[FILENAME_MAX] = "";
	int eachObject, eachSample = 0;
	int timeStamp;
	int tempTimeStamp = moveObjPos[0].objAllPos[0].currentTime;
	char trialDirectory[DIRECTORY_MAX] = "C:\\EVS\\experiments\\";
	char sessionNumberBuffer[3];
	char trialNumberBuffer[3];

	// Retrieve the trial directory
	strcat(trialDirectory, sessionParams.expName);
	strcat(trialDirectory, "\\Sess");
	sprintf(sessionNumberBuffer, "%d", sessionParams.sessNum);
	strcat(trialDirectory, sessionNumberBuffer);
	strcat(trialDirectory, "\\Tr");
	sprintf(trialNumberBuffer, "%d", trialParams.trialNum);
	strcat(trialDirectory, trialNumberBuffer);
	strcat(trialDirectory, "\\");

	// Create the trial directory and put the gelog file under.
	if(CreateDirectory (trialDirectory, NULL) == 1)
	{
		// Get the full path for the gelog file by appending the gelog file name to the trial directory.
		strcat(gelogFileName, trialDirectory);
		strcat(gelogFileName, trialName);
		strcat(gelogFileName, ".gelog");

		if( ( gelog = fopen(gelogFileName, "w" ) ) == NULL )
			alert_printf("GELog file could not be open.");
		else
		{
			fprintf(gelog, "GELogStart\n");

			// The trial name
			fprintf(gelog, "TrialName %s\n", trialName);

			// DisplayX value DisplayY value
			fprintf(gelog, "DisplayX %d ", sessionParams.displayX);
			fprintf(gelog, "DisplayY %d \n", sessionParams.displayY);			

			// Condition value
			fprintf(gelog, "Condition %d\n", trialParams.condNum);

			// GridDisplay value
			fprintf(gelog, "GridDisplay %d\n", trialParams.gridDisplay);

			// BGA value BGR value BGG value BGB value
			fprintf(gelog, "BGA %d ", trialParams.bgColorA);
			fprintf(gelog, "BGR %d ", trialParams.bgColorR);
			fprintf(gelog, "BGG %d ", trialParams.bgColorG);
			fprintf(gelog, "BGB %d \n", trialParams.bgColorB);
     
			// FGA value FGR value FGG value FGB value
			fprintf(gelog, "FGA %d ", trialParams.fgColorA);
			fprintf(gelog, "FGR %d ", trialParams.fgColorR);			
			fprintf(gelog, "FGG %d ", trialParams.fgColorG);
			fprintf(gelog, "FGB %d \n", trialParams.fgColorB);

			// NumObjects value
			fprintf(gelog, "NumObjects %d \n", trialParams.numObjects);
     
			// NumTargets value
			fprintf(gelog, "NumTargets %d \n", trialParams.numTargets);

			for(eachObject = 0 ; eachObject < trialParams.numObjects ; eachObject++)
			{
				fprintf(gelog, "ObjStart \n");
				fprintf(gelog, "ObjName Obj%d \n", trialParams.objInfo[eachObject].objNum);
				fprintf(gelog, "ObjectFilePath %s \n", trialParams.objInfo[eachObject].objPath);
				fprintf(gelog, "IsTargetObject %d \n", trialParams.objInfo[eachObject].objIsTarget);
			}

			fprintf(gelog, "TrialStart \n");

			for(eachSample = 0; eachSample < samples; eachSample++)
			{
				timeStamp = moveObjPos[0].objAllPos[eachSample].currentTime;
			
				/*
				// padding between the timeStamps
				while( tempTimeStamp+1 < timeStamp )
				{
					tempTimeStamp++;

					// write previous x-y values
					fprintf(gelog, "TimeStart %d \n", tempTimeStamp); // only first object's timeStamp is recorded
					for(eachObject = 0; eachObject < trialParams.numObjects; eachObject++)
					{
						fprintf(gelog, "Obj%dx %d Obj%dy %d \n",
									eachObject+1, moveObjPos[eachObject].objAllPos[eachSample-1].x,
									eachObject+1, moveObjPos[eachObject].objAllPos[eachSample-1].y );
					}

					fprintf(gelog, "TimeEnd \n");
				}*/

				// logging the next actual object positions in memory
				fprintf(gelog, "TimeStart %d \n", timeStamp); // only first object's timeStamp is recorded
				for(eachObject = 0; eachObject < trialParams.numObjects; eachObject++)
				{
					fprintf(gelog, "Obj%dx %d Obj%dy %d \n",
								eachObject+1, moveObjPos[eachObject].objAllPos[eachSample].x,
								eachObject+1, moveObjPos[eachObject].objAllPos[eachSample].y );
				}

				fprintf(gelog, "TimeEnd \n");
				//tempTimeStamp = timeStamp;		
			}

			fprintf(gelog, "TrialEnd\n");
			fprintf(gelog, "SubjectTargetStart\n");

			for(eachObject = 0; eachObject < trialParams.numTargets; eachObject++)
			{
				fprintf(gelog, "Obj%d\n", selectedObjects[eachObject]);
			}

			fprintf(gelog, "SubjectTargetEnd\n");
			fprintf(gelog, "GELogEnd");

			fclose(gelog);
		}
	}
}

