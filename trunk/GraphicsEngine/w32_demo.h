/*****************************************************/
/* Windows 95/98/NT/2000/XP sample experiment in C   */
/* For use with Version 2.0 of EyeLink Windows API   */
/*                                                   */
/*      (c) 1997-2002 by SR Research Ltd.            */
/* For non-commercial use by Eyelink licencees only  */
/*                                                   */
/* THIS FILE: w32_demo.h     (all templates)         */
/* CONTENTS:                                         */
/* - Function and variable declarations              */
/*   to link C files used in experiments             */
/* - contains declarations for all templates         */  
/*                                                   */
/*                                                   */
/* CHANGES FOR Windows 2000/XP, EyeLink 2.0:         */
/* - added is_eyelink2 ( 1 if EyeLink II tracker)    */
/*                                                   */
/* Author: May Wong                                  */
/* - added draw_screen_divider()                     */
/* - added average_color()                           */
/* - added draw_fixation_marker()                    */
/* - added struct objIniPosition *objInitPos()       */
/* - added currentRandomPosition currObjPos()        */
/* - added distanceBtwObjects()                      */
/* - added computeBounce()                           */
/* - added isObjectClosedToOther()                   */
/* - added moveObjects()                             */
/* - added randomSpeed()                             */
/* - added randomDirection()                         */
/* - added setupInitMoveObjects()                    */
/*                                                   */
/* Author: Bahar Akbal                               */
/* - added *selectTargetObjects()                    */
/* - added writeToGELogFile()                        */
/*****************************************************/

          // These colors are used to draw the target and camera display
          // The background color is used to clear the display
          // for calibration, drift correction, and after recording is interrupted.
extern COLORREF target_foreground_color;  /* target display color */
extern COLORREF target_background_color;  /* background color */

// the application instance: required to create windows and get resources
extern HANDLE application_instance;  
//extern LPARAM mylParam;

/*************** CREATE FULL-SCREEN WINDOW **********/

extern HWND full_screen_window;          // window handle
extern int full_screen_window_active;    // set if we are topmost window

        // Create the full screen window
        // Ensure it is maximized, and no WM_PAINT messages are pending
        // Returns 0 if OK, else couldn't create
int make_full_screen_window(HINSTANCE hInstance);

        // Destroy or clean up window  
void close_full_screen_window(void);

        // Clear the window
void clear_full_screen_window(COLORREF c);

       // Handles all messages for full-scren experiment window
       // Repaint is handled by simply erasing the display: 
       // this should only happen if ALT-ESC used to switch apps 
       // or after dialog boxes (i.e. EDF file transfer) are erased.
       // NOTE: during calibration and drift correction, most messages are intercepted by EXPTSPPT
LRESULT CALLBACK full_screen_window_proc(HWND hwnd, UINT message, WPARAM wparam, LPARAM lparam);

/*********** BITMAP DRAWING AND COPYING *********/

               // Draw grid of letters on local display, 
               // Draw boxes for each letter on EyeLink screen
               // Creates memory bitmap 
               // After you're done, delete with DeleteObject(hBitmap)
HBITMAP draw_grid_to_bitmap(void);


       // Draws a grid of letters to a DDB bitmap                
       // This can be rapidly copied to the display
       // Also draw boxes to the EyeLink tracker display for feedback
	   // Save the segmentation information in the specified files.
       // Returns: Handle of bitmap
HBITMAP draw_grid_to_bitmap_segment(char *filename, char*path, int dotrack);


               // Copies bitmap to display 
               // Places top-left at (x,y)
void display_bitmap(HWND hwnd, HBITMAP hbm, int x, int y);


/************* FONT DRAWING SUPPORT ***********/

extern HFONT current_font;

              // Create a font, cache in current_font
HFONT get_new_font(char *name, int size, int bold);

                  // Release the font
void release_font(void);

		// Uses printf() formatting
		// Prints starting at x, y position
		// Uses current font, size, foreground color, bkgr. color & clr mode
                // <bg> is -1 to NOT clear background 
void graphic_printf(COLORREF fg, COLORREF bg, int center, int x, int y, char *fmt, ...);

/********** DRAW FORMATTED TEXT ***********/

       // Draw text within margins, left-justified, with word-wrap.
       // Draw to <hdc> context, so it supports display and bitmaps   
       // (bitmap must already be created and selected into context).
       // Draw boxes for each word on EyeLink tracker display if <dotrack> set
       // Use font selected with get_new_font(), and draws it in <color>
       // RECT <margins> sets margins, and <lspace> sets pixels between lines
void draw_text_box(HDC hdc, char *txt, COLORREF color, RECT margins, int lspace, int dotrack);

/********** DRAW FORMATTED TEXT AND SAVE SEGMENTATION ***********/

       // Draw text within margins, left-justified, with word-wrap.
       // Draw to <hdc> context, so it supports display and bitmaps   
       // (bitmap must already be created and selected into context).
       // Draw boxes for each word on EyeLink tracker display if <dotrack> set
       // Use font selected with get_new_font(), and draws it in <color>
       // RECT <margins> sets margins, and <lspace> sets pixels between lines
	   // Save the segmentation info into the text. 
	   // Currently, the bottom edge of the line N overlaps with the top edge of line N+1
	   // In each line, the left edge of word N overlaps with the right edge of word N+1
	   // Extra space (the width of letter 'M') is added to the the beginning and ending segment of each line 
void draw_text_box_segment(HDC hdc, char *txt, COLORREF color, 
		RECT margins, int lspace, int dotrack, char *filename);

/************ CREATE BITMAP WITH FORMATTED TEXT ***********/
      // Draw text to full-screen sized bitmap, cleared to <bgcolor>
      // Draw boxes for each word on EyeLink tracker display if <dotrack> set
      // Use font selected with get_new_font(), and draws it in <fgcolor>
      // RECT <margins> sets margins, and <lspace> sets pixels between lines
HBITMAP text_bitmap(char *txt, COLORREF fgcolor, COLORREF bgcolor, 
                               RECT margins, int lspace, int dotrack);

/************ CREATE BITMAP WITH FORMATTED TEXT AND SAVE SEGMENTATION INFO ***********/

      // Draw text to full-screen sized bitmap, cleared to <bgcolor>
      // Draw boxes for each word on EyeLink tracker display if <dotrack> set
      // Use font selected with get_new_font(), and draws it in <fgcolor>
      // RECT <margins> sets margins, and <lspace> sets pixels between lines
	  // <dottrack> determines whether outlines of the word segment is displayed over the tracker PC.
	  // Splice the path and filename to record the segmentation information
	  // If the segmentation file already exists, it will not overwritten the file 
	  // if <sv_options> is set as SV_NOREPLACE
HBITMAP text_bitmap_segment(char *txt, COLORREF fgcolor, COLORREF bgcolor, 
                  RECT margins, int lspace, int dotrack, char *fname, char *path, INT16 sv_options);


/*********** LOAD IMAGE FILE, CREATE BITMAP ************/

           // Creates DDB bitmap from loaded image file
           // if <keepsize>, makes bitmap the same size as image file
           // otherwise, creates a full-screen sized bitmap
           // If <dx, dy> are not 0, will resize image to those dimensions
           // If resized image won't fill display, 
           // it clears the bitmap to <bgcolor> and centers the image in it
           // NOTE: intermediate bitmap is 24-bit color, 
           // so won't work well in 256-color display modes

HBITMAP image_file_bitmap(char *fname, int keepsize, int dx, int dy, COLORREF bgcolor);


/************* EXPERIMENT TRIALS ************/

      // Several types of trials are demonstrated
      // Some support multiple displays 
      // <trial> is trial number
      // <type> selects the stimulus: 0 for text, 1 for picture images, 2 for grid of letter
      // <trialid> sets the contents of the TRIALID message
      // <time_limit> is the maximum time the stimuli are displayed 

    // NOTE: TRIALID AND TITLE MUST HAVE BEEN SET BEFORE CALLING THESE FUNCTIONS!
    // FAILURE TO INCLUDE THESE MAY CAUSE INCOMPATIBILITIES WITH ANALYSIS SOFTWARE!

                // Simple trial, displays a single line of text
int simple_recording_trial(char *text, UINT32 time_limit);

                // Trial using bitmap copying
                // Caller must create and destroy the bitmap
int bitmap_recording_trial(HBITMAP *gbm, int trialNum, int displayX, int displayY, int gridDisplay, UINT32 duration, COLORREF fgColor, COLORREF bgColor);
               
                //Draw screen divider
                //lineType==1 (Horizontal line)
                //lineType==2 (Vertical line)
                //lineType==3 (both horizontal and vertical lines)
void draw_screen_divider(int lineType, int displayX, int displayY, HDC hdc, COLORREF fgColor);

//Compute the average color values.
int average_color(int R, int G, int B);

//Draw fixation marker
void draw_fixation_marker(int gridDisplay, int displayX, int displayY, HDC hdc);

//Pointer to all initial objects' position
struct objIniPosition *objInitPos(int trialNum, int numObjects, int displayX, int displayY, HBITMAP *gbm, int gridDisplay);

//find the current random position of the object
struct currentRandomPosition currObjPos(int trialNum, int i, int gridDisplay, int displayX, int displayY, int bitmapWidth, int bitmapHeight);

//find the distance between two objects
double distanceBtwObjects(HBITMAP bitmap1, double x1, double y1, HBITMAP bitmap2, double x2, double y2);

//compute the bouncing object's new speed and direction
int computeBounce(HBITMAP *gbm, int gridDisplay, int p);

//test an object is closed to any other objects.
int isObjectClosedToOther(HBITMAP *gbm, int gridDisplay, int p, int newX, int newY);

//move the object to a new position.
void moveObjects(HBITMAP *gbm, int gridDisplay,int p);

//compute random speed between minimum and maximum speed.
int randomSpeed(int minSpeed, int maxSpeed);

//compute random direction 0 or 1
int randomDirection();

//setup the position, speed, and direction for moving objects.
void setupInitMoveObjects(int gridDisplay, struct objIniPosition *obj_ini_pos);

//select target objects
int *selectTargetObjects(int sampleNumber, HBITMAP *gbm, HDC theHDC);  //esk-added hdc param

//write object movement data to GE log file
void writeToGELogFile( char *trialName, struct objectPositions *moveObjPos, int samples, int *selectedObjects);
 
//get the bitmap width
int bitmap_width(HBITMAP hbm);

//get the bitmap height
int bitmap_height(HBITMAP hbm);

                // trial with real-time gaze cursor
                // Caller must create and destroy the bitmap
int realtime_data_trial(HBITMAP bitmap, UINT32 time_limit);

      // Run gaze-contingent window trial
      // <fgbm> is bitmap to display within window
      // <bgbm> is bitmap to display outside window
      // <wwidth, wheight> is size of window in pixels
      // <mask> flags whether to treat window as a mask
      // <time_limit> is the maximum time the stimuli are displayed 
int gc_window_trial(HBITMAP fgbm, HBITMAP bgbm, 
                    int wwidth, int wheight, int mask, UINT32 time_limit);

                // trial with gaze control
                // this trial is assumend to draw its own bitmap, 
                // which matches the control regions
int gaze_control_trial(UINT32 time_limit);

	// Plays back last trial data
	// Prints white "F" for fixations
	// Connects samples with black line
int playback_trial(void);


/************ TRIAL SEQUENCING *********/

	/* This code sequences trials within a block. */
	/* It calls do_xxx_trial() to execute a trial, then interperts result code. */
	/* It places a result message in the EDF file */
int run_trials(void);

    // Each type of experiment has its own function which executes trials by number.
    // For each trial, it must:
         // - set title, TRIALID
         // - Create bitmaps and EyeLink display graphics
         // - Check for errors in creating bitmaps
         // - Run the trial recording loop
         // - Delete bitmaps
         // - Return any error code

int do_simple_trial(int num); 

int do_text_trial(int num);

int do_picture_trial(int num);

int do_data_trial(int num);

int do_control_trial(int num);

int do_gcwindow_trial(int num);

extern int is_eyelink2;
