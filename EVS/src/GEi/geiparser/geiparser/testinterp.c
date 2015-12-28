// testinterp.c
//  o test interpreter functions
//  o ekhan@cs 20070227, 20070327, 20070520 (added output example)


#include "interpreter.h"
#include <stdio.h>


int main(int argc, char *argv[]) {
	int i;
	int returnCode;
	
	printf("This is testinterp.c ...\n\n");

	printf("Initialization - begin ...\n");
	returnCode=initInterp();
	if (returnCode !=0) {
		printf("Error with initialization. Exiting.\n");
		return 1;
	}
	printf("Initialization - end\n");

	printf("\n");
	printf("Get Session Info - begin ...\n");
	printSessionInfo();
	printf("Get Session Info - end\n");

	printf("\n");
	printf("Get Trial Info - begin ...\n");

	for (i=1; i <= sessionParams.numTrials; i++) {
		getTrialParams(i);
		printTrialInfo();
	}
	printf("Get Trial Info - end\n");

	printf("\n");
	printf("Cleanup - begin ...\n");
	endInterp();
	printf("Cleanup - end\n");

	printf("\n");
	printf("Press <Enter> to close ...\n");
	getchar();

	return;
}

/* Example output for a session with 1 trial
 * -----------------------------------------
 *
 * This is testinterp.c ...
 * 
 * Initialization - begin ...
 *   printTLookupTable - begin
 *     param name=bgColorA offset in lookup table=24
 *     param name=bgColorB offset in lookup table=36
 *     param name=bgColorG offset in lookup table=32
 *     param name=bgColorR offset in lookup table=28
 *     param name=condName offset in lookup table=8
 *     param name=condNum  offset in lookup table=4
 *     param name=duration offset in lookup table=16
 *     param name=fgColorA offset in lookup table=40
 *     param name=fgColorB offset in lookup table=52
 *     param name=fgColorG offset in lookup table=48
 *     param name=fgColorR offset in lookup table=44
 *     param name=gridDisplay      offset in lookup table=20
 *     param name=isBounce offset in lookup table=56
 *     param name=isFixMarker      offset in lookup table=12
 *     param name=minBounceDist    offset in lookup table=60
 *     param name=minStartDist     offset in lookup table=64
 *     param name=numObjects       offset in lookup table=68
 *     param name=numTargets       offset in lookup table=72
 *     param name=objInfo  offset in lookup table=76
 *     param name=trialNum offset in lookup table=0
 *   printTLookupTable - end
 *   printOLookupTable - begin
 *     param name=objIsTarget      offset in lookup table=8
 *     param name=objMaxSpeedH     offset in lookup table=20
 *     param name=objMaxSpeedV     offset in lookup table=28
 *     param name=objMinSpeedH     offset in lookup table=16
 *     param name=objMinSpeedV     offset in lookup table=24
 *     param name=objNum   offset in lookup table=0
 *     param name=objPanel offset in lookup table=12
 *     param name=objPath  offset in lookup table=4
 *   printTLookupTable - end
 *   getSessionFile - begin
 *     session file length = 54
 *     session file = C:\EVS\experiments\GEdemo1\Sess1\GEdemo1_Sess1.session
 *     return code = 0
 *   getSessionFile - end
 *   getXmlTree - begin
 *   getXmlTree - end
 *   getXmlRootNode - begin
 *     root element: "experiment"
 *   getXmlRootNode - end
 *   getXmlSessionNode - begin
 *     session element: "session"
 *   getXmlSessionNode - end
 *   getSessionInfo - begin
 *   getSessionInfo - end
 * Initialization - end
 * 
 * Get Session Info - begin ...
 *   printSessionInfo - begin
 *     sessionParams.expName = GEdemo1
 *     sessionParams.sessNum = 1
 *     sessionParams.displayX = 1400
 *     sessionParams.displayY = 1050
 *     sessionParams.numTrials = 1
 *   printSessionInfo - end
 * Get Session Info - end
 * 
 * Get Trial Info - begin ...
 *   getTrialParams - begin - trial #1
 *     checking if struct needs to be reset ... no (first run)
 *   getTrialParams - calling getObjsParams
 *   getObjsParams - begin
 *   getObjsParams - end
 *   getTrialParams - returned from getObjsParams
 *   getTrialParams - end
 *   printTrialInfo - begin
 *     trialParams.trialNum=1
 *     trialParams.condNum=1
 *     trialParams.condName=demo
 *     trialParams.isFixMarker=0
 *     trialParams.duration=30000
 *     trialParams.gridDisplay=0
 *     trialParams.bgColorA=255
 *     trialParams.bgColorR=128
 *     trialParams.bgColorG=255
 *     trialParams.bgColorB=255
 *     trialParams.fgColorA=255
 *     trialParams.fgColorR=192
 *     trialParams.fgColorG=192
 *     trialParams.fgColorB=192
 *     trialParams.isBounce=1
 *     trialParams.minBounceDist=80
 *     trialParams.minStartDist=150
 *     trialParams.numObjects=5
 *     trialParams.numTargets=2
 *     trialParams.objInfo[1].objnum=1
 *     trialParams.objInfo[1].objPath=c:\\EVS\\experiments\\GEdemo1\\images\\elli_50x50_-65536.bmp
 *     trialParams.objInfo[1].objIsTarget=1
 *     trialParams.objInfo[1].objPanel=1
 *     trialParams.objInfo[1].objMinSpeedH=2
 *     trialParams.objInfo[1].objMaxSpeedH=300
 *     trialParams.objInfo[1].objMinSpeedV=2
 *     trialParams.objInfo[1].objMaxSpeedV=300
 *     trialParams.objInfo[2].objnum=2
 *     trialParams.objInfo[2].objPath=c:\\EVS\\experiments\\GEdemo1\\images\\elli_50x50_-8372224.bmp
 *     trialParams.objInfo[2].objIsTarget=0
 *     trialParams.objInfo[2].objPanel=1
 *     trialParams.objInfo[2].objMinSpeedH=2
 *     trialParams.objInfo[2].objMaxSpeedH=300
 *     trialParams.objInfo[2].objMinSpeedV=2
 *     trialParams.objInfo[2].objMaxSpeedV=300
 *     trialParams.objInfo[3].objnum=3
 *     trialParams.objInfo[3].objPath=c:\\EVS\\experiments\\GEdemo1\\images\\rect_50x50_-16777088.bmp
 *     trialParams.objInfo[3].objIsTarget=0
 *     trialParams.objInfo[3].objPanel=1
 *     trialParams.objInfo[3].objMinSpeedH=2
 *     trialParams.objInfo[3].objMaxSpeedH=300
 *     trialParams.objInfo[3].objMinSpeedV=2
 *     trialParams.objInfo[3].objMaxSpeedV=300
 *     trialParams.objInfo[4].objnum=4
 *     trialParams.objInfo[4].objPath=c:\\EVS\\experiments\\GEdemo1\\images\\elli_50x50_-16744384.bmp
 *     trialParams.objInfo[4].objIsTarget=1
 *     trialParams.objInfo[4].objPanel=1
 *     trialParams.objInfo[4].objMinSpeedH=2
 *     trialParams.objInfo[4].objMaxSpeedH=300
 *     trialParams.objInfo[4].objMinSpeedV=2
 *     trialParams.objInfo[4].objMaxSpeedV=300
 *     trialParams.objInfo[5].objnum=5
 *     trialParams.objInfo[5].objPath=c:\\EVS\\experiments\\GEdemo1\\images\\elli_50x50_-8355840.bmp
 *     trialParams.objInfo[5].objIsTarget=0
 *     trialParams.objInfo[5].objPanel=1
 *     trialParams.objInfo[5].objMinSpeedH=2
 *     trialParams.objInfo[5].objMaxSpeedH=300
 *     trialParams.objInfo[5].objMinSpeedV=2
 *     trialParams.objInfo[5].objMaxSpeedV=300
 *   printTrialInfo - end
 * Get Trial Info - end
 * 
 * Cleanup - begin ...
 * Cleanup - end
 * 
 */