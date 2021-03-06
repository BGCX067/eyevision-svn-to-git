/*****************************************************/
/* Windows 95/98/NT/2000/XP sample experiment in C   */
/* For use with Version 2.0 of EyeLink Windows API   */
/*                                                   */
/*      (c) 1997-2002 by SR Research Ltd.            */
/* For non-commercial use by Eyelink licencees only  */
/*                                                   */
/* THIS FILE: w32_demo_main.c (all templates)        */
/* CONTENTS:                                         */
/* - Stub for Windows (WinMain(), etc)               */
/* - app_main() for experiment program:              */
/* - connect to tracker                              */  
/* - check display mode, create full-screen window   */
/* - set up calibration colors, targets              */
/* - open EDF file on tracker                        */
/* - configure tracker                               */
/* - call run_trials() to do experiment              */
/* - close EDF file, transfer over link              */
/* - clean up, exit                                  */
/*                                                   */
/*                                                   */
/* CHANGES FOR Windows 2000/XP, EyeLink 2.0:         */
/* - change header file to w32_exptsppt2.h           */
/* - add is_eyelink2 variable and check version      */
/* - add required data types to file, link data      */
/* - use EyeLink II saccade detector settings		 */
/*													 */
/* Author: Bahar Akbal, Elena Khan					 */
/* - added steps to save the edf file on display	 */
/*		   pc according to "expName_SessNum.EDF"	 */
/*		   naming convention. (bhr)					 */
/* - changed intro text (esk 20070521)               */
/*****************************************************/
 
#include <windows.h>
#include <windowsx.h>
#include <stdlib.h>
#include <time.h>

#include "eyelink.h"
#include "w32_exptsppt2.h"    // CHANGED FOR DLL VERSION 2.0
#include "w32_demo.h"

int is_eyelink2;   // set if we are connected to EyeLink II tracker

           // the application instance: required to create windows and get resources
HANDLE application_instance = NULL;  

           // display information: size, colors, refresh rate
DISPLAYINFO dispinfo;

           // The colors of the target and background for calibration and drift correction
COLORREF target_foreground_color = RGB(0,0,0);
COLORREF target_background_color = RGB(192,192,192); 

           // Name for experiment: goes in task bar, and in EDF file
char program_name[100] = "Windows Sample Experiment 2.0";


app_main()
{
  int i, j;
  char tempEdfName[260] = "TEST"; //name for the edf stored in the host pc
  char *edfName, *experimentName, *sessionName; // pointers to the edf name to be stored in display pc,
												// experiment name and the session name.
  char edfPath[120] = ""; //directory to save the edf file under in display pc

  //if(open_eyelink_connection(1)) return -1;                                   //esk - 1=run offline
  if(open_eyelink_connection(0)) return -1;    // abort if we can't open link	//esk - 0=use Eyelink
  set_offline_mode();                          
  flush_getkey_queue();                        // initialize getkey() system
  is_eyelink2 = (2 == eyelink_get_tracker_version(NULL) );

  srand((unsigned)time(NULL));

  get_display_information(&dispinfo);          // get window size, characteristics
      
     // NOTE: Camera display does not support 16-color modes
     // NOTE: Picture display examples don't work well with 256-color modes
     //       However, all other sample programs should work well.
  if(dispinfo.palsize==16)      // 16-color modes not functional
    {
      alert_printf("This program cannot use 16-color displays");
      goto shutdown;
    }
  if(dispinfo.refresh < 40)  // wait_for_refresh doesn't work!
    {
      alert_printf("No refresh synchroniztion available!");
    }
  if(dispinfo.palsize)     // 256-color modes: palettes not supported by this example
    {
      alert_printf("This program is not optimized for 256-color displays");
    }

  if(make_full_screen_window(application_instance)) goto shutdown;  // create the window
  if(init_expt_graphics(full_screen_window, NULL)) goto shutdown;   // register window with EXPTSPPT

  i = SCRWIDTH/60;        // select best size for calibration target
  j = SCRWIDTH/300;       // and focal spot in target
  if(j < 2) j = 2;
  set_target_size(i, j);  // tell DLL the size of target features

  target_foreground_color = RGB(0,0,0);    // color of calibration target
  target_background_color = RGB(128,128,128);       // background for calibration and drift correction
  set_calibration_colors(target_foreground_color, target_background_color); // tell EXPTSPPT the colors

  set_cal_sounds("", "", "");
  set_dcorr_sounds("", "off", "off");
                                     // draw a title screen
  clear_full_screen_window(target_background_color);    // clear screen
  get_new_font("Times Roman", SCRHEIGHT/32, 1);         // select a font
                                                        // Draw text

  //esk 20070521 - change text (function call and other parameters not changed)
  graphic_printf(target_foreground_color, target_background_color, 1, SCRWIDTH/2, 1*SCRHEIGHT/30, 
                 "EVS Multi-object Tracking Experiment");
  graphic_printf(target_foreground_color, target_background_color, 1, SCRWIDTH/2, 2*SCRHEIGHT/30,
                 "Visual Lab/M. Pomplun - UMassBoston");
  graphic_printf(target_foreground_color, target_background_color, 1, SCRWIDTH/2, 3*SCRHEIGHT/30, "Have fun!");
  //graphic_printf(target_foreground_color, target_background_color, 0, SCRWIDTH/5, 4*SCRHEIGHT/30, 
  //               "Source code may be used as template for your experiments.");
  
  if(tempEdfName[0])    // If file name set, open it
    {        
		if(!strstr(tempEdfName, ".")) strcat(tempEdfName, ".EDF");  // add extension
      i = open_data_file(tempEdfName);                              // open file
      if(i!=0)                                                        // check for error
        {
          alert_printf("Cannot create EDF file '%s'", tempEdfName);
          goto shutdown;
        }                                                              // add title to preamble
      eyecmd_printf("add_file_preamble_text 'RECORDED BY %s' ", program_name);
    }
                                      // Now configure tracker for display resolution
  eyecmd_printf("screen_pixel_coords = %ld %ld %ld %ld",    // Set display resolution   
                 dispinfo.left, dispinfo.top, dispinfo.right, dispinfo.bottom);
  eyecmd_printf("calibration_type = HV9");                  // Setup calibration type
  eyemsg_printf("DISPLAY_COORDS %ld %ld %ld %ld",           // Add resolution to EDF file
                 dispinfo.left, dispinfo.top, dispinfo.right, dispinfo.bottom);
  if(dispinfo.refresh>40)
    eyemsg_printf("FRAMERATE %1.2f Hz.", dispinfo.refresh);

		// SET UP TRACKER CONFIGURATION 
		      // set parser saccade thresholds (conservative settings)
  if(is_eyelink2)
    {
      eyecmd_printf("select_parser_configuration 0");  // 0 = standard sensitivity
    }
  else
    {
      eyecmd_printf("saccade_velocity_threshold = 35");
      eyecmd_printf("saccade_acceleration_threshold = 9500");
    }
		      // set EDF file contents 
  eyecmd_printf("file_event_filter = LEFT,RIGHT,FIXATION,SACCADE,BLINK,MESSAGE,BUTTON");
  eyecmd_printf("file_sample_data  = LEFT,RIGHT,GAZE,AREA,GAZERES,STATUS");
		      // set link data (used for gaze cursor) 
  eyecmd_printf("link_event_filter = LEFT,RIGHT,FIXATION,SACCADE,BLINK,BUTTON");
  eyecmd_printf("link_sample_data  = LEFT,RIGHT,GAZE,GAZERES,AREA,STATUS");
                      // Program button #5 for use in drift correction
  eyecmd_printf("button_function 5 'accept_target_fixation'");

  if(!eyelink_is_connected() || break_pressed()) goto end_expt;  // make sure we're still alive

		// RUN THE EXPERIMENTAL TRIALS (code depends on type of experiment)
                // Calling run_trials() performs a calibration followed by trials
                // This is equivalent to one block of an experiment 
                // It will return ABORT_EXPT if the program should exit
  i = run_trials();

end_expt:                // END: close, transfer EDF file
  set_offline_mode();               // set offline mode so we can transfer file
  pump_delay(500);                  // delay so tracker is ready
  eyecmd_printf("close_data_file"); // close data file

  if(break_pressed()) goto shutdown;          // don't get file if we aborted experiment


  // bhr - steps to save the edf on display pc
  experimentName = get_experiment_name(); // get the experiment name
  sessionName = get_session_name(); // get the session name
  strcat(edfPath, "C:\\EVS\\experiments\\"); // generate the path to save the edf on display pc
  strcat(edfPath, experimentName);
  strcat(edfPath, "\\");
  strcat(edfPath, sessionName);
  strcat(edfPath, "\\");

  edfName = get_edf_name(); // get the edf file name to be stored on host pc

  strcat(edfPath, edfName);

  if( edfName )  // make sure we got the edf name (not null)
	  receive_data_file(tempEdfName, edfPath, 1);  // transfer the file


shutdown:                // CLEANUP
  close_expt_graphics();           // tell EXPTSPPT to release window
  close_eyelink_connection();      // disconnect from tracker
  close_full_screen_window();
  return 0;
}


        // WinMain - Windows calls this to execute application
int PASCAL WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
  application_instance = hInstance;  // record the application instance for accessing resources
  full_screen_window = NULL;
  app_main();                        // call our real program
  close_eyelink_connection();        // make sure EYELINK DLL is released
  if(full_screen_window)
    close_full_screen_window();
  return 0;
} 

