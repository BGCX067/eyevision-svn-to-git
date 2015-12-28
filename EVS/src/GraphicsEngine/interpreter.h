// interpreter.h 
//  o header file for interpreter
//  o ekhan 20070226, 20070407, 20070520 (corrected typo, added ERRDOGUI)

#include <windows.h>				// for UINT32

#define ERRDOGUI -1					// doGUI() will return -1 if user closes gui (i.e., doesn't choose a session file)

extern int initInterp(void);		// for GE
extern void printSessionInfo(void);	// for debug
extern int getTrialParams(int);		// for GE
extern void printTrialInfo(void);	// for debug
extern void endInterp(void);		// for GE


// sessionParams and trialParams will be accessed directly by GE
struct sparams {
   char * expName;
   int sessNum;
   int displayX ;    
   int displayY ;   
   int numTrials ;
} sessionParams ;


struct oparams {         // info for each obj - this struct has to precede "struct tparams"
   int objNum;
   char * objPath;
   int objIsTarget;
   int objPanel;
   int objMinSpeedH;
   int objMaxSpeedH;
   int objMinSpeedV;
   int objMaxSpeedV;
} ;


struct tparams {
   int trialNum ;
   int condNum;
   char * condName;
   int isFixMarker ;
   UINT32 duration ;  
   int gridDisplay ;
   int bgColorA;
   int bgColorR;
   int bgColorG;
   int bgColorB;
   int fgColorA;
   int fgColorR;
   int fgColorG;
   int fgColorB;
   int isBounce ;
   int minBounceDist ;
   int minStartDist ;
   int numObjects ; 
   int numTargets ;
   struct oparams * objInfo;	// pointer to an array of "struct oinfo" (created at runtime)
} trialParams ;  
