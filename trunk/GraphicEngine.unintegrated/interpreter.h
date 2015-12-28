

struct sparams {
   int displayX ;    
   int displayY ;   
   int numTrials ;
}sessionParams;


struct tparams {
	int isFixMarker;
	UINT32 duration;
	int gridDisplay;
	int bgColorR;
	int bgColorG;
	int bgColorB;
	int fgColorR;
	int fgColorG;
	int fgColorB;
	int isBounce;
	int minBounceDist;
	int minStartDist;
	int numObjects;
	int numTargets;
	struct oinfo *objInfo;
}trialParams;

struct oinfo{
	int objNum;
	char *objPath;
	int objIsTarget;
	int objPanel;
	int objMinSpeedH;
	int objMaxSpeedH;
	int objMinSpeedV;
	int objMaxSpeedV;
};

