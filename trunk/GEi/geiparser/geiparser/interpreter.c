// interpreter.c 
//  o not a stand-alone application
//  o contains functions for Graphics Engine to access data from session files
//    -initInterp() - initializes interpreter and reads session params into sparams struct
//    -getTrialParams() - called once per trial, reads trial params from session file into tparams and oparams structs
//    -endInterp() - cleans up (frees memory)
//  o .session files are in xml format.  The libxml2 library (xmlsoft.org) is used to parse the xml.
//  o the following third-party libs (www.zlatkovic.com/libxml.en.html) were used:
//    -iconv-1.9.2.win32     
//    -libxml2-2.6.27.win32
//    -zlib-1.2.3.win32
//    It is assumed that they live in C:\EVS\ext\GEi
//  o see also (1) refs and (2) notes on lookup table at end of file 
//  o history
//    -v1 ekhan 20070214
//    -v2 ekhan 20070307
//    -v3 ekhan 20070318 - current version
//    -v4 ekhan 20070407 - turn off debug (no printf)
//    -v5 ekhan 20070520 - tidy up
//    -v6 ekhan 20070524 - fixed typo in getTrialParams() - was retrieving the trialNum again instead of the condNum


#include <stdio.h>
#include <stdlib.h> // for atoi
#include <string.h>

#include <libxml/parser.h>
#include <libxml/xmlmemory.h>

#include "interpreter.h"

# define DEBUG 0			// for debugging (console output)  0=false, 1=true (enable printf statements)

char * sessionFile;

xmlDocPtr xDoc;			// ptr to xml doc
xmlNodePtr rootNode;	// keep ptr to root node in case we have to traverse again
xmlNodePtr sessNode;	// ptr to session node
xmlNodePtr trialsNode;	// ptr to trials node (set during getSessionParams function)

xmlNodePtr currNode;	// ptr to current node (convenience var for parsing child nodes)
xmlNodePtr currTrNode;	// hold current trial node 

int currTrNum=0;		// hold current trial number

char * xmlRootName = "experiment" ;	// name of root node (for error-checking)
char * xmlSessName = "session" ;    // name of session node (for looping)
char * xmlTrialsName = "trials" ;	// name of trials node
char * xmlTrialName  = "trial";
char * xmlObjectsName = "objects";
char * xmlObjectName  = "object";
char * xmlNumberName  = "number";
char * xmlConditionName = "condition";


/* function declarations */
int initTablesValues(void);		// for init: calculate size/count values for lookup tables
void initTLookupTable(void);	// for init: load lookup table with offset values (trial info)
void initOLookupTable(void);	// for init: load lookup table with offset values (object info)

void * findTableEntry (const char *targetName, const void *arrayName, int count);	// for init: lookup into array
int param_cmp (const struct offsets *str1, const struct offsets *str2);				// for init: sorting lookup table

int getXmlTree(void);				// for init: get xml doc pointer
int getXmlRootNode(void);			// for init: get xml root node
int getXmlSessionNode(void);		// for init: get xml session node and parameters

extern int doGUI(void);				// for init (from libgeigui.lib): run gui (user chooses session file)
extern int getFileName(char * buf);	// for init (from libgeigui.lib): get session file name from gui

int getSessionFile(void);			// for init: get session file
int getSessionParams(void);			// for init: get session parameters & set trialsNode
int getTrialParams(int tNum);		// get trial parameters & set objsNode
int getObjsParams (xmlNodePtr objsNodePtr);
int resetTparams(void);				// reset trialParams struct to initial values

/* print functions */
void printTLookupTable(void);		// for init: debug
void printOLookupTable(void);		// for init: debug
void printNodes(xmlNodePtr xnptr, int boolPrintTextNodes); // for debug
void printTrialInfo(void);			// for debug


/* lookup table info */	
/*	 o tables provide location of parameters in their respective struct
 *   o for example: the objNum parameter is the first parameter in the oparams struct, so its offset is 0
 *     (see testinterp.c in the 'geiparser' Solution for all the offsets)
 *   o the lookup tables themselves have to be sorted in order to be used
 *   o see end of file for more notes on lookup table usage
 */

struct offsets {					
	const char * name ;				 
	int value ; // the offset
	int type ;
};
struct offsets oLookupTable[] =
{
	{"objNum", -1, 'i'},
	{"objPath", -1, 'c'},
	{"objIsTarget", -1, 'i'},
	{"objPanel", -1, 'i'},
	{"objMinSpeedH", -1, 'i'},
	{"objMaxSpeedH", -1, 'i'},
	{"objMinSpeedV", -1, 'i'},
	{"objMaxSpeedV", -1, 'i'}
};
struct offsets tLookupTable[] = 
{
	{"trialNum", -1, 'i'},
	{"condNum", -1, 'i'},
    {"condName", -1, 'c'},
	{"isFixMarker", -1, 'i'},
	{"duration", -1, 'i'},
	{"gridDisplay", -1, 'i'},
	{"bgColorA", -1, 'i'},
	{"bgColorR", -1, 'i'},
	{"bgColorG", -1, 'i'},
	{"bgColorB", -1, 'i'},
	{"fgColorA", -1, 'i'},
    {"fgColorR", -1, 'i'},
	{"fgColorG", -1, 'i'},
	{"fgColorB", -1, 'i'},
	{"isBounce", -1, 'i'},
	{"minBounceDist", -1, 'i'},
	{"minStartDist", -1, 'i'},
	{"numObjects", -1, 'i'},
	{"numTargets", -1, 'i'},
	{"objInfo", -1, 'p'}
};

char typeI = 'i';					// for comparing to lookup_table types
char typeC = 'c';					// for comparing to lookup_table types
//char typeP = 'p';					// (don't need to compare 'p' values yet)

/* convenience vars for lookup tables 
 *  o avoid multiple re-calculations throughout code
 *  o calculated in initTablesValues()
 */
int sizeOffsets;  		// size of "offsets" struct
int sizeOparams;		// size of "oparams" struct
int sizeTtable;			// size of tLookupTable
int sizeOtable;			// size of oLookupTable
int numElemTtable;		// number of elements in tLookupTable
int numElemOtable;		// number of elements in oLookupTable


/******************/
/* initialization */
/******************/
int initInterp() {
	int returnCode = 0;
	initTablesValues();
	initTLookupTable();
	initOLookupTable();
	if (DEBUG) printTLookupTable();
	if (DEBUG) printOLookupTable();
	returnCode=getSessionFile();	if (returnCode == ERRDOGUI)  { return returnCode; }	// GE should exit 
	returnCode=getXmlTree();	if (returnCode != 0) { return returnCode; }
	returnCode=getXmlRootNode();	if (returnCode != 0) { return returnCode; }
	returnCode=getXmlSessionNode();	if (returnCode != 0) { return returnCode; }
	returnCode=getSessionParams();	if (returnCode != 0) { return returnCode; }
	return 0;
}

/*****************************************/
/* initialization - supporting functions */
/*****************************************/

/***************************
 * initialize lookup tables: 
 *    initTablesValues();
 *    initTLookupTable();
 *    initOLookupTable();
 *    findTableEntry();
 *    param_cmp();
 ***************************/

int initTablesValues(void){
	// calculate the size/count values of the lookup table (needed for findTableEntry function)

	sizeOffsets   = sizeof (struct offsets);				// size of "offsets" struct
	sizeTtable    = sizeof (tLookupTable);
	sizeOtable    = sizeof (oLookupTable);
	numElemTtable = sizeTtable / sizeOffsets;	// number of elements in tLookupTable
	numElemOtable = sizeOtable / sizeOffsets;	// number of elements in oLookupTable
	sizeOparams   = sizeof (struct oparams);				// size of "oparams" struct
	return 0;
}

void initTLookupTable(void) {
	// load the trial lookup table:
	//   o get "count" of structs (needed for sort) 
	//   o sorts the table (needed for bsearch)
	//   o for each parameter: 
	//     -finds offset
	//     -finds parameter/element in lookup table (extra test that lookup table works)
	//     -enters offset into lookup table
	
	struct offsets * element;	// ptr to found struct in array
	
	qsort (tLookupTable, numElemTtable, sizeOffsets, param_cmp);	// sort
	
	// (step-by-step code)
	// int offset;	// offset of param inside struct ("offset" is the value to load)
	// offset = offsetof(struct tparams, trialNum);
	// element = findTableEntry("trialNum",tLookupTable,count);
	// element->value = offset ;
	
	element = findTableEntry("trialNum",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, trialNum);
	element = findTableEntry("condNum",tLookupTable,numElemTtable);		element->value = offsetof(struct tparams, condNum);
	element = findTableEntry("condName",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, condName);
	element = findTableEntry("isFixMarker",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, isFixMarker);
	element = findTableEntry("duration",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, duration);
	element = findTableEntry("gridDisplay",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, gridDisplay);
    element = findTableEntry("bgColorA",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, bgColorA);
	element = findTableEntry("bgColorR",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, bgColorR);
	element = findTableEntry("bgColorG",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, bgColorG);
	element = findTableEntry("bgColorB",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, bgColorB);
    element = findTableEntry("fgColorA",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, fgColorA);
	element = findTableEntry("fgColorR",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, fgColorR);
    element = findTableEntry("fgColorG",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, fgColorG);
    element = findTableEntry("fgColorB",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, fgColorB);
	element = findTableEntry("isBounce",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, isBounce);
	element = findTableEntry("minBounceDist",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, minBounceDist);
	element = findTableEntry("minStartDist",tLookupTable,numElemTtable);	element->value = offsetof(struct tparams, minStartDist);
	element = findTableEntry("numObjects",tLookupTable,numElemTtable);		element->value = offsetof(struct tparams, numObjects);
	element = findTableEntry("numTargets",tLookupTable,numElemTtable);		element->value = offsetof(struct tparams, numTargets);
	element = findTableEntry("objInfo",tLookupTable,numElemTtable);			element->value = offsetof(struct tparams, objInfo);
}

void initOLookupTable(void) {
	// load the object lookup table
	
	struct offsets * element;	// ptr to found struct in array

	qsort (oLookupTable, numElemOtable, sizeOffsets, param_cmp);	// sort

	element = findTableEntry("objNum",oLookupTable,numElemOtable);		element->value = offsetof(struct oparams, objNum);
    element = findTableEntry("objPath",oLookupTable,numElemOtable);		element->value = offsetof(struct oparams, objPath);
	element = findTableEntry("objIsTarget",oLookupTable,numElemOtable);	element->value = offsetof(struct oparams, objIsTarget);
	element = findTableEntry("objPanel",oLookupTable,numElemOtable);	element->value = offsetof(struct oparams, objPanel);
	element = findTableEntry("objMinSpeedH",oLookupTable,numElemOtable);	element->value = offsetof(struct oparams, objMinSpeedH);
	element = findTableEntry("objMaxSpeedH",oLookupTable,numElemOtable);	element->value = offsetof(struct oparams, objMaxSpeedH);
    element = findTableEntry("objMinSpeedV",oLookupTable,numElemOtable);	element->value = offsetof(struct oparams, objMinSpeedV);
	element = findTableEntry("objMaxSpeedV",oLookupTable,numElemOtable);	element->value = offsetof(struct oparams, objMaxSpeedV);
}


void * findTableEntry (const char *targetName, const void *arrayName, int numElements) {
	/* o perform a lookup into a sorted array
	 * o return ptr to found element
	 * o ref:  http://libiya.upf.es/www-storage/glibc/libc_9.html
	 */

	struct offsets target;		// have to give bsearch something to compare to
	struct offsets * result;	// ptr to struct in lookup table (where "offset" will be stored)

	target.name = targetName;
	result = bsearch (&target, arrayName, numElements, sizeOffsets, param_cmp);

	// needs to be error-checking here
	//   o bsearch returns either pointer to an object or null
	//   o can check for null result with something like
	//     if (result) printf ("Result:  %s %d\n", result->name, result->value);
	//     else printf ("ERROR: Couldn't find %s.\n", targetName);

	return result ;
}


int param_cmp (const struct offsets *str1, const struct offsets *str2) {
	/* comparison function used for sorting and searching. 
	 * (http://libiya.upf.es/www-storage/glibc/libc_9.html)
	 */

	return strcmp (str1->name, str2->name);
}

/*******************************************
 * get session file (part of initialization)
 *******************************************/

int getSessionFile() {
	// has to return an int in order to let GE know that there was an error and that it should exit

	int leng, result;

	if (DEBUG) printf("  getSessionFile - begin\n");
	
	leng = doGUI();
	if (DEBUG) printf("    session file length = %d\n",leng);
	if (leng == ERRDOGUI) {
		return leng;
	}
	
	sessionFile=(char *)malloc(leng);
	result = getFileName(sessionFile);

	if (DEBUG) printf("    session file = %s\n",sessionFile);
	if (DEBUG) printf("    return code = %d\n",result);

	if (DEBUG) printf("  getSessionFile - end\n");

	return leng;
}

/**********************************
 * xml functions for initialization
 **********************************/

int getXmlTree(void){
	if (DEBUG) printf("  getXmlTree - begin\n");

	/* get tree */
	xDoc = xmlParseFile(sessionFile) ;
	
	if (xDoc == NULL ) {
		fprintf(stderr,"ERROR: Document not parsed successfully. \n");
		return 1;
	}

	if (DEBUG) printf("  getXmlTree - end\n");
	return 0;
}


int getXmlRootNode(void){
	if (DEBUG) printf("  getXmlRootNode - begin\n");
	
	/* get root node */
	rootNode = xmlDocGetRootElement(xDoc);
	if (rootNode == NULL) {
		fprintf(stderr,"ERROR: empty document\n");
		xmlFreeDoc(xDoc);
		return 1;
	}
	if (DEBUG) printf("    root element: \"%s\"\n",rootNode->name);

	if (xmlStrcmp(rootNode->name, (const xmlChar *) xmlRootName)) {
		fprintf(stderr,"ERROR: document of the wrong type, root node != %s\n",xmlRootName);
		xmlFreeDoc(xDoc);
		return 1;
	}
	if (DEBUG) printf("  getXmlRootNode - end\n");
	return 0;
}


int getXmlSessionNode(void){
	if (DEBUG) printf("  getXmlSessionNode - begin\n");
	
	/* get session node */
	currNode = rootNode->xmlChildrenNode; // child node of root only holds "text" (whitespace) and "session", may hold "comment"

	/* loop thru child nodes until "session" */
	while (currNode != NULL) {
		if ((!xmlStrcmp(currNode->name, (const xmlChar *)xmlSessName))) {	// looking for "session"
			sessNode = currNode;
			break;
		}
		currNode = currNode->next;
	}

	/* check that we found something */
	if (sessNode == NULL) {
		fprintf(stderr,"ERROR: didn't find session node\n");
		return 1;
	} 
	else {
		if (DEBUG) printf("    session element: \"%s\"\n",currNode->name);	
	}

	if (DEBUG) printf("  getXmlSessionNode - end\n");
	return 0;
}

/*****************************************
 * get session parameters (initialization)
 *****************************************/

int getSessionParams() {
	/* two actions: 
	 *   o writes the values of child nodes displayX, displayY, numTrials into struct
	 *     (there are also "text" nodes, which are effectively ignored - nothing is done with them)
	 *   o writes location of "trial" node in struct            
	 */

	xmlChar *content;
	xmlNodePtr p = sessNode->children; // get ptr to first child node 

	if (DEBUG) printf("  getSessionInfo - begin\n");	
	
	while (p != NULL) {		
		// o loop thru the children of the session node
		// o we are not using the lookup table procedure for session parameters
		//   -node names are hard-coded in this function
		//   -each loop does six "if" checks

		if ((!xmlStrcmp(p->name, (const xmlChar *)"expName"))) {
			sessionParams.expName = xmlNodeGetContent(p) ;
				//o expName is now a pointer to the xml string
				//o don't free the content - we want to keep it for the whole session
				//o string is freed in endInterp function
		}

		if ((!xmlStrcmp(p->name, (const xmlChar *)"sessNum"))) {
			content = xmlNodeGetContent(p) ;
			sessionParams.sessNum = atoi(content) ;
			xmlFree(content);
		}

		if ((!xmlStrcmp(p->name, (const xmlChar *)"displayX"))) {
			content = xmlNodeGetContent(p) ;
			sessionParams.displayX = atoi(content) ;
			xmlFree(content);
		}
		if ((!xmlStrcmp(p->name, (const xmlChar *)"displayY"))) {
			content = xmlNodeGetContent(p) ;
			sessionParams.displayY = atoi(content) ;
			xmlFree(content);
		}
		if ((!xmlStrcmp(p->name, (const xmlChar *)"numTrials"))) {
			content = xmlNodeGetContent(p) ;
			sessionParams.numTrials = atoi(content) ;
			xmlFree(content);
		}
		if ((!xmlStrcmp(p->name, (const xmlChar *)xmlTrialsName))) {	// found "trials" node
			trialsNode = p ;					// set locations of trials node
			currTrNode = trialsNode->children;	// set for trial#1 in getTrialParams()
		}
		
		p = p->next;
	}

	/* check that we have a trialsNode */
	if (trialsNode == NULL) {			// initialized as null in interpreter.h
		fprintf(stderr,"ERROR: no \"trials\" node\n");
		return 1;
	}

	if (DEBUG) printf("  getSessionInfo - end\n");

	return 0;
}

/********************************/
/* end initialization functions */
/********************************/


/*********************************/
/* parameter retrieval functions */
/*       getTrialParams();       */
/*       getObjsParams();        */
/*********************************/

int getTrialParams(int tNum){
	// o parameter:  tNum - requested trial number
	// o two actions:  
	//   -fills in trialParams struct
	//   -sets objsNode var to "objects" node
	// o ref:  http://www.yolinux.com/TUTORIALS/GnomeLibXml2.html 
	//        (how to get attribute values)

	xmlNodePtr p;				// for looping/parsing
	int value;					// hold a trial node's assigned number (retrieved from trial node's attribute)
	struct offsets * element;	// ptr to an element in lookup table (returned by findTableEntry)
	xmlChar *content;			// node content is parsed as strings
	int contentAsInt;			// for node content that is an integer (holds int value after atoi() conversion)
	struct tparams * tpptr = &trialParams;		// ptr to trialParams
	int returnCode=0;			// for getObjsParams

	if (DEBUG) printf("  getTrialParams - begin - trial #%d\n",tNum);

	/*********************/
	/* reset trialParams */		
	/*********************/     
	if (DEBUG) printf("    checking if struct needs to be reset ...");
	if (currTrNum == 0) {
		// using value of "currTrNum" to determine whether we need to reset struct
		// (it's initialized to 0 and there is no trial 0)
		if (DEBUG) printf(" no (first run)\n");
	}
	else {
		if (DEBUG) printf(" yes (resetting ...");
		resetTparams();
		if (DEBUG) printf(" done)\n");
	}

    /*************************************/
	/* check sequence of requested trial */
	/*   and find startnode for parsing  */
	/*************************************/
	// "p" determines where parsing starts
	if (tNum == currTrNum+1) {
		// expected behavior is that GE asks for trials in sequence
		currTrNum = tNum;		// set for next iteration 
		p = currTrNode;			// currTrNode holds previous trialnode (start searching from previous trialnode)
	}
	else {
		fprintf(stderr,"Warning: requested out-of-sequence trial number. Last#=%d. Requested#=%d. Continuing ...\n",currTrNum,tNum);
		p = trialsNode->children;	// start search from beginning (at first child_node)
	}

	/****************************/
	/* look for requested trial */
	/****************************/
	// loop thru nodes (including Text & Comment nodes) looking for tNum
	while (p != NULL) {

		if ((!xmlStrcmp(p->name, (const xmlChar *)xmlTrialName))) {		// found a "trial" node
			value = atoi(xmlGetProp(p,xmlNumberName));			// read attribute (i.e., trial "number")

			if (value == tNum) {	// check if this is the requested trial_number 
				currTrNode = p;		// set current_trial_node ptr
				break;				// stop looping (we found what we're looking for)
			}
			// else this is not the node we're looking for - move on to next node
		}
		p = p->next;
	}

	/* after loop, p either points to requested_trial_node or to null */
	if (p == NULL) {
		fprintf(stderr,"ERROR: didn't find trial node #%d\n",tNum);
		return 1;
	}

    /********************/
	/* get trial params */
	/********************/
	trialParams.trialNum = tNum ;	// WRITE TO STRUCT: trial number
	p=currTrNode->children;			// get the child_nodes of the trial node

	/* loop until we find "condition" param (must be the first param) */
	while ((xmlStrcmp(p->name, (const xmlChar *)xmlConditionName))) {
		p=p->next;
	}  

	/* at this point, we're at condition_node */
	trialParams.condNum  = atoi(xmlGetProp(p,xmlNumberName));			// WRITE TO STRUCT: condition number
	trialParams.condName = xmlNodeGetContent(p);						// WRITE TO STRUCT: condition name (really giving ptr to string)

	/* now loop thru the rest of params */
	do {				// at start of do_loop, still at condition_node
		p=p->next;		// go to next node
		
		element = findTableEntry(p->name,tLookupTable,numElemTtable);	// returns either ptr or null
		if (element) {
			// o element is in lookup table, which means it is a value we want
			// o all fields from this point on are ints 
			//   (if params are later added that are not ints, then must check "type" field of struct offsets)

			content = xmlNodeGetContent(p);		// get string
			contentAsInt = atoi(content);		// convert to int
			*(int *)((char *)tpptr + element->value) = contentAsInt;		
					// o WRITE TO STRUCT:  use offset from lookup_table to write into struct
					// o ref:  http://docs.linux.cz/programming/c/www.eskimo.com/~scs/C-faq/q2.15.html
			xmlFree(content);
		}
	} while ((xmlStrcmp(p->name, (const xmlChar *)xmlObjectsName)));
		// o loop until the "objects" node (there must be an objects_node, else we'll loop forever)
		// o loop over (i.e., ignore) text/comments nodes
		// o using lookup table to write into struct, so xml_node_names must be the same as lookup_table_names

	/* at this point, p points to "objects" node */

	/*************************/
	/* get object parameters */
	/*************************/

	/* create struct for object params */
	trialParams.objInfo = malloc(sizeOparams * trialParams.numObjects);

	/* get info */
	if (DEBUG) printf("  getTrialParams - calling getObjsParams\n");
	
	returnCode = getObjsParams(p);
	if (returnCode != 0) {
		fprintf(stderr,"ERROR in getObjsParams\n");
		return 1;
	}
	if (DEBUG) printf("  getTrialParams - returned from getObjsParams\n");

	if (DEBUG) printf("  getTrialParams - end\n");
	return 0;
}


int getObjsParams (xmlNodePtr objsNodePtr) {
	// o fills in objInfo (an array of struct oparams)
	// o parameter:  objsNodePtr - pointer to "objects" node

	xmlChar * ocontent;						// for node content
	struct offsets * oelement;				// ptr to an element in oLookupTable (returned by findTableEntry)
	struct oparams ** opptr;				// ptr to obj params (for writing into array of structs)

	xmlNodePtr op = objsNodePtr->children;	// working ptr (get list of "object" nodes)
	xmlNodePtr opch;						// will traverse child_nodes (parameters) of each "object"
	int i = 0;								// for looping through "object" nodes

	if (DEBUG) printf("  getObjsParams - begin\n");

	/* loop thru "object" nodes */
	while (op != NULL) {

		/* only want "object" nodes (i.e., ignore "Text" nodes */
		if ((!xmlStrcmp(op->name, (const xmlChar *)xmlObjectName))) {	// found "object" node

			/* get obj num */
			trialParams.objInfo[i].objNum = atoi(xmlGetProp(op,xmlNumberName));		// WRITE TO STRUCT: objNum

			/* get addr of obj's "oparams" struct */
			opptr  = &trialParams.objInfo[i];
			
			/* set up parameter list */
			opch = op->children;		// get list of child nodes (parameters) for "object"

			/* get rest of info */
			while (opch != NULL) {		// loop through child nodes
				
				oelement = findTableEntry(opch->name,oLookupTable,numElemOtable);	// returns either ptr or null
				if (oelement) {
					ocontent = xmlNodeGetContent(opch);								// get string
					
					/* find out param type */
					if (oelement->type == typeI){		// param is an int
						*(int *)((char *)opptr + oelement->value) = atoi(ocontent);	// WRITE TO STRUCT (int param)
						xmlFree(ocontent);
					}
					else if (oelement->type == typeC){	// param is a string
						*(int *)((char *)opptr + oelement->value) = ocontent;		// WRITE TO STRUCT (string param)
						// o we're really writing the address of the string that xmlNodeGetContent returned
						// o don't free "ocontent" - we'll keep a ptr to this string for duration of trial
						// o string will be freed in resetTparams function
					}
				}

				opch=opch->next;	// move to next child node (parameter)
			}

			i++;	// this was a valid node - increment array element
		}
		op=op->next;	// get next child node
	}

	if (DEBUG) printf("  getObjsParams - end\n");
	return 0;
}
/*************************************/
/* end parameter retrieval functions */
/*************************************/


/**********************/
/* clean-up functions */
/*   resetTparams();  */
/*   endInterp();     */
/**********************/

int resetTparams(void){
	// o trialParams struct is reset to original state (all values are -1) before each trial 
	//   (except the first trial, since it's already in the original state by definition)
	// o resetting params makes erroneous debug output easier to see
	// o resetting params includes freeing data structures, so resetting doubles as a tidying procedure

	int i;
	struct tparams * tpptr = &trialParams;

	// free strings (they were created via the xmlNodeGetContent function in getObjsParams)
	for (i=0; i<trialParams.numObjects; i++){
		xmlFree(trialParams.objInfo[i].objPath);	// frees the "objPath" parameter
	}

	// free array
	if (trialParams.objInfo != NULL) {
		free(trialParams.objInfo);
		trialParams.objInfo=NULL;
	}

	// free string (created via xmlNodeGetContent)
	if (trialParams.condName != NULL) {
		xmlFree(trialParams.condName);
	}

	// reset values (only trialParams; objInfo array has been freed)
	for (i=0; i<numElemTtable; i++) {
		if (tLookupTable[i].type == typeI) {
			*(int *)((char *)tpptr + tLookupTable[i].value) = -1;
		}
		else if (tLookupTable[i].type == typeC) {
			*(int *)((char *)tpptr + tLookupTable[i].value) = NULL;
		}
	}
	return 0;
}

void endInterp(void){
	resetTparams();						//frees structures/strings 
	xmlFree(sessionParams.expName);		//free the string (from getSessionsParams function)
	xmlFreeDoc(xDoc);					//free dom
}
/**************************/
/* end clean-up functions */
/**************************/


/*****************************/
/* print functions for debug */
/*****************************/

/**************************
 * functions: 
 *
 *    printTLookupTable();  
 *    printOLookupTable();  
 *    printSessionInfo();
 *    printTrialInfo();
 *    printNodes();		// not used in code
 *
 **************************/

void printTLookupTable(void) {	
	// prints name and value of tLookupTable (lookup table for trial parameters struct) elements
	
	int i ;

	printf("  printTLookupTable - begin\n");
	for (i = 0; i < numElemTtable; i++) {
		printf ("    param name=%s\toffset in lookup table=%d\n", tLookupTable[i].name, tLookupTable[i].value);
    }
	printf("  printTLookupTable - end\n");

	return ;
}


void printOLookupTable(void){	
	// prints name and value of oLookupTable (lookup table for object parameters struct) elements
	
	int i;

	printf("  printOLookupTable - begin\n");
	for (i = 0; i < numElemOtable; i++) {
		printf ("    param name=%s\toffset in lookup table=%d\n", oLookupTable[i].name, oLookupTable[i].value);
    }
	printf ("  printTLookupTable - end\n");
}


void printSessionInfo(void) {
	// prints session parameters

	printf("  printSessionInfo - begin\n");
	printf("    sessionParams.expName = %s\n", sessionParams.expName);
	printf("    sessionParams.sessNum = %d\n", sessionParams.sessNum);
	printf("    sessionParams.displayX = %d\n", sessionParams.displayX);
	printf("    sessionParams.displayY = %d\n", sessionParams.displayY);
	printf("    sessionParams.numTrials = %d\n", sessionParams.numTrials);
	printf("  printSessionInfo - end\n");
}



void printTrialInfo(void) {
	// prints out info in trialParams struct (struct is re-populated with each call to getTrialParams())
	
	int i ;			// for looping over obj info
	int n ;			// convenience var for printout

	printf("  printTrialInfo - begin\n");
	printf("    trialParams.trialNum=%d\n",trialParams.trialNum);
    printf("    trialParams.condNum=%d\n",trialParams.condNum);
    printf("    trialParams.condName=%s\n",trialParams.condName);
	printf("    trialParams.isFixMarker=%d\n",trialParams.isFixMarker);
	printf("    trialParams.duration=%d\n",trialParams.duration);
	printf("    trialParams.gridDisplay=%d\n",trialParams.gridDisplay);
    printf("    trialParams.bgColorA=%d\n",trialParams.bgColorA);
	printf("    trialParams.bgColorR=%d\n",trialParams.bgColorR);
	printf("    trialParams.bgColorG=%d\n",trialParams.bgColorG);
	printf("    trialParams.bgColorB=%d\n",trialParams.bgColorB);
    printf("    trialParams.fgColorA=%d\n",trialParams.fgColorA);
    printf("    trialParams.fgColorR=%d\n",trialParams.fgColorR);
	printf("    trialParams.fgColorG=%d\n",trialParams.fgColorG);
	printf("    trialParams.fgColorB=%d\n",trialParams.fgColorB);
	printf("    trialParams.isBounce=%d\n",trialParams.isBounce);
	printf("    trialParams.minBounceDist=%d\n",trialParams.minBounceDist);
	printf("    trialParams.minStartDist=%d\n",trialParams.minStartDist);
	printf("    trialParams.numObjects=%d\n",trialParams.numObjects);
	printf("    trialParams.numTargets=%d\n",trialParams.numTargets);
	
	for (i=0;i<trialParams.numObjects;i++) {
		n=i+1;
		printf("    trialParams.objInfo[%d].objnum=%d\n",n,trialParams.objInfo[i].objNum);
        printf("    trialParams.objInfo[%d].objPath=%s\n",n,trialParams.objInfo[i].objPath);
		printf("    trialParams.objInfo[%d].objIsTarget=%d\n",n,trialParams.objInfo[i].objIsTarget);
		printf("    trialParams.objInfo[%d].objPanel=%d\n",n,trialParams.objInfo[i].objPanel);
		printf("    trialParams.objInfo[%d].objMinSpeedH=%d\n",n,trialParams.objInfo[i].objMinSpeedH);
		printf("    trialParams.objInfo[%d].objMaxSpeedH=%d\n",n,trialParams.objInfo[i].objMaxSpeedH);
		printf("    trialParams.objInfo[%d].objMinSpeedV=%d\n",n,trialParams.objInfo[i].objMinSpeedV);
		printf("    trialParams.objInfo[%d].objMaxSpeedV=%d\n",n,trialParams.objInfo[i].objMaxSpeedV);
	}

	printf("  printTrialInfo - end\n");
}


void printNodes(xmlNodePtr xnptr, int boolPrintTextNodes) {
	// o not used in code
	// o boolPrintTextNodes:  true = 1
	//                        false = 0
	// o the libxml2 parser keeps track of whitespace as "Text" nodes.
	//   This behavior is (1) primarily due to the lack of DTD in the xml .session file
	//   and (2) may or may not be avoided - when I used a DTD in a small xml file, the
	//   "Text" nodes still appeared.
	// o we're not interested in the whitespace, so we're not interested in the "Text" nodes.
	//   But for debugging, seeing the "Text" nodes may be helpful.
	
	xmlNodePtr p ;
	char * TextName = "text" ;

	// check that parameter is either 0 or 1
	if (boolPrintTextNodes != 0 && boolPrintTextNodes != 1) {
		printf("ERROR: illegal parameter sent to printNodes().  Will print Text nodes ...\n");
		boolPrintTextNodes = 1;
	}

	p = xnptr->children ; // get list of child nodes
	while (p != NULL) {
		if (boolPrintTextNodes) {
			printf("%s\n",p->name);
		}
		else {
			if ((xmlStrcmp(p->name, (const xmlChar *)TextName))) { // evaluates to match for non-Text node
				printf("%s\n",p->name);
			}
		}
		p = p->next ;
	}
} 


/**************/
/* references */
/**************/
/*
 * references for code:
 * 
 * o http://xmlsoft.org/tutorial/apc.html
 *   (traversing dom tree)
 *
 * o http://www.yolinux.com/TUTORIALS/GnomeLibXml2.html
 *   (example dom parse)
 *
 * o http://libiya.upf.es/www-storage/glibc/libc_9.html 
 *   (sorting example)
 *
 */

/*************************/
/* notes on lookup table */
/*************************/
/* lookup table - used to find element in struct by location/byte_offset rather than by name
 *
 *  o example situation:  
 *    -the interpreter is parsing the trial info in the .session file
 *    -it finds the "duration" node, which has a value of 10000 (10 sec)
 *     The value "10000" needs to be written to the "trialParams" struct
 *    -one way to save the information is with a procedure like in the "getSessionParams" function.
 *     The function uses six "if" statements to find out which node it is at.
 *     The equivalent functionality for the "trialParams" struct would have 20 "if" statements
 *     (19 values and a pointer to an object_info array)
 *    -in order to avoid the multiple "if" statements, a lookup table is used instead to determine
 *     the location in "trialParams" where "10000" will be written
 *    -for example, the lookup table "tLookupTable" says that "duration" has an offset of 16.
 *     That means that "duration" is stored 16 bytes from the beginning of "tLookupTable".
 *     To find the address of this location, 16 is added to the address of "tLookupTable".
 *     The value "10000" is written to the address &tLookupTable + 16.
 *
 *  o goals of using lookup tables:
 *    -reduce number of "if" statements
 *    -produce faster code (although it's unclear if there is any significant performance benefit)
 *
 *  o supporting a new parameter in the .session file:
 *    -add param to appropriate struct in .h
 *    -add entry to appropriate lookup table 
 *    -add initialization to either initTLookupTable() or initOLookupTable()
 *    -check if any code needs to be added to getTrialParams() or getObjsParams()
 *     --both functions do a couple of "manual" copies before using the "automatic" lookup table method
 *     --if an int parameter is added to "trialParams" after the "condName" element, then no change to getTrialParams() is necessary
 *     --if a char parameter is added to "trialParams", then the function will need to be edited
 *     --if an int or string parameter is added to "struct oparams" after the "objNum" element, 
 *       then no change to getObjsParams() is necessary
 *    -add line to printTrialInfo() (for debug/console output)
 *    -if the new param is a string, it needs to be freed in resetTparams()
 *
 *  o the session parameters are not handled via a lookup table
 *
 *  o in order to use the only C-provided search function (bsearch), 
 *    the lookup tables have to be sorted (in ascendeding order) before they can be bsearched
 *
 *  o the sorting (via the only C-provided sort function qsort) must happen in an initialization routine
 *    (i.e., before a lookup table is used for searching)
 *
 *  o for future revisions:  an alternative to using a lookup table is to use a hashtable 
 *    (C does not natively support hashtables, so a third-party library must be used)
 *
 *  o Refs:
 *    -example (http://libiya.upf.es/www-storage/glibc/libc_9.html)
 *    -the C FAQ (http://docs.linux.cz/programming/c/www.eskimo.com/~scs/C-faq/q2.15.html)
 *    -mailing list (http://www.thescripts.com/forum/thread217699.html)
 *    -K&R (p. 253)
 *
 */
