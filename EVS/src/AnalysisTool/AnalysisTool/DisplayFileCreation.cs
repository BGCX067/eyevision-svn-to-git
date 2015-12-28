using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Data;
using System.Text.RegularExpressions; // for streamreader
using System.Diagnostics; // for Process
using System.Threading;
using AnalysisTool;




/**
 Provides functionality to Create Display File for Analysis Tool.
  •	Given a Gelog File and a Gaze file for the Trial, builds an XML file .
    •	Extracts Gaze positions and Object positions into a Queue independently. 
    •	Then  deques each queue and Builds the Data required for Replay. 
    .   Gaze file has timestamps logged in every 2 milli seconds. While 
 
 * GElog File has Timestamps logged at an interval equal to the Refresh rate of the Monitor
   on which it was  played. 
 * Hence currently the Refresh Rate is Hard coded to 85 HZ under the Variable : REFRESH_RATE 
 * While dequeuing the 2 queues created  above,we  skip a total of  : 1000/(REFRESH_RATE) timestamps 
 * to ensure a smooth  replay.
 * The data in this Display File is used to Create Excel file which could be used for analysing purposes.
   	Usage of Concurrent Programming technique
    
    @author Smitha Madhavamurthy
    @date 03-23-2007
*/

namespace AnalysisTool
{
    class DisplayFileCreation
    {
        private static TextWriter log = null; //new StreamWriter("C:\\EVS\\experiments\\Exp1\\Sess1\\Tr1\\display.log");


        static System.Collections.Hashtable table = new System.Collections.Hashtable();
        public static System.Collections.Queue geLogTrialDataQueue = new System.Collections.Queue();
        public static System.Collections.Queue edfTrialDataQueue = new System.Collections.Queue();


        // Regular Expressions used to extract data from EDF and GE Log Files
        private static string geLogStartRegex = @"GELogStart.*\r\nTrialName\s(.*)\r\nDisplayX\s(\d+)\sDisplayY\s(\d+)";
        private static string conditionRegex = @"Condition\s.*(\d+).*\r\nGridDisplay\s.*(\d).*\r\nBGA\s*(\d{1,3})\s*BGR\s*(\d{1,3})\s*BGG\s*(\d{1,3})\s*BGB\s(\d{1,3}).*\r\nFGA\s*(\d{1,3})\s*FGR\s.*(\d{1,3})\s*FGG\s*(\d{1,3})\s*FGB\s*(\d{1,3}).*\r\nNumObjects\s*(\d+).*\r\nNumTargets\s*(\d+)";
        private static string subjTargetStartRegex = @"SubjectTargetStart(.*\r\n)*SubjectTargetEnd";

        private static int REFRESH_RATE = 85;
        private static int speedControl = 1;

        /**
         * Initialize the Object with required Node names to be written to the Display File .
         */
        public static void initialize()
        {

            table.Add("DisplayFileStart", null);
            table.Add("Trial", "Trial");
            table.Add("Display", "Display");

            table.Add("Condition", "Condition");
            table.Add("Num", "Num");
            table.Add("GridDisplay", "GridDisplay");
            table.Add("BGA", "BGA");
            table.Add("BGR", "BGR");
            table.Add("BGG", "BGG");
            table.Add("BGB", "BGB");
            table.Add("FGA", "FGA");
            table.Add("FGR", "FGR");
            table.Add("FGG", "FGG");
            table.Add("FGB", "FGB");
            table.Add("NumObjects", "NumObjects");
            table.Add("NumTargets", "NumTargets");
            table.Add("ObjStart", "ObjStart");
            table.Add("ObjectFileName", "ObjectFileName");
            table.Add("ObjectFilePath", "ObjectFilePath");
            table.Add("IsTargetObject", "IsTargetObject");
            table.Add("TrialStart", "TrialStart");
            table.Add("TimeStart", "TimeStart");
            table.Add("Obj", "Obj");
            table.Add("SubjectTargetStart", "SubjectTargetStart");
            table.Add("Name", "Name");
            table.Add("X", "X");
            table.Add("Y", "Y");
            table.Add("obj", "obj");
            table.Add("Time", "Time");
        }


        /**
         * Entry Point To Create Display File Functionality
         */
        public static void create(String geLogFileName, String edfFileName, String displayFileName)
        {

            String tmpGeFileName = geLogFileName;
            string displayLogFileName = Regex.Replace(tmpGeFileName, ".gelog$", ".executionLog");

            if (File.Exists(displayLogFileName))
            {
                File.Delete(displayLogFileName);
            }
            log = new StreamWriter(displayLogFileName);
            log.WriteLine("--------------------------  Display File creation -----------------------");
            log.WriteLine("gelogFileName : " + geLogFileName + " :  edfFileName : " + edfFileName);
            log.Flush();

            try
            {
                //validate Input 
                if (geLogFileName == null || !File.Exists(geLogFileName) || edfFileName == null || !File.Exists(edfFileName))
                {
                    throw new ATLogicException(" Invalid Input Parameters : GeLogFile :" + geLogFileName + " ,Edf File : " + edfFileName);
                }
            }
            catch (FieldAccessException)
            {
                throw new ATLogicException(" File access Exception: GeLogFile :" + geLogFileName + " ,Edf File : " + edfFileName);
            }
            try
            {
                if (table.Count <= 0)
                {
                    initialize();
                }

                if (File.Exists(displayFileName))
                {
                    log.WriteLine(" Display file : " + displayFileName + "  Exists.Returning to the Caller ");
                    log.Flush();
                }

                log.WriteLine(" Converting file to Strings ");
                log.Flush();

                String geLogFileStr = GetDataFromFile(geLogFileName);
                String edfFileStr = GetDataFromFile(edfFileName);
                try
                {
                    parseData(geLogFileStr, geLogFileName, edfFileStr, edfFileName, displayFileName);
                }
                catch (Exception e)
                {
                    WriteError(e);

                    if (File.Exists(displayFileName))
                    {
                        File.Delete(displayFileName);
                    }
                    throw new ATLogicException(e.Message + " : Parsing Data Error :: Creating Display File : " + displayFileName);
                }
            }
            catch (Exception e)
            {
                WriteError(e, " Error Creating Display File");

                if (File.Exists(displayFileName))
                {
                    File.Delete(displayFileName);
                }
                throw new ATLogicException(e.StackTrace + " : : Error Creating Display File");
            }

            log.Flush();
            log.Close();

        }// End  of creating Display Fil


        /**
         * 
         */
        public static void parseData(String fileStr, String geLogFileName, String edfFileStr, String edfFileName, String displayFileName)
        {
            log.WriteLine(" parse data method");
            log.Flush();
            log.WriteLine("validating input");
            //validate input
            if (fileStr == null || geLogFileName == null || edfFileStr == null || edfFileName == null || displayFileName == null)
            {
                log.WriteLine("ABORTING RUN :Invalid Input parameters to Parse Data ");
                log.Flush();
                throw new ATLogicException("ABORTING RUN :Invalid Input parameters to Parse Data ");
            }
            log.WriteLine(" input data validated successfully");
            log.Flush();

            XmlDocument xmlDoc = new XmlDocument();
            XmlTextWriter xmlWriter = null;
            try
            {
                // Define Regular Expressions to parse GeLog File and Edf files
                // Create Display File in XML Format 


                if (File.Exists(displayFileName))
                {
                    log.WriteLine("Processing Complete : Display File  Exists : " + displayFileName);
                    log.Flush();
                    // For Testing Purposes  : Delete the exisiting File and Create a new One
                    File.Delete(displayFileName);
                }
                log.WriteLine("Writing Data into XMl file");
                log.Flush();

                //if file  not found, create a new xml file
                log.WriteLine(" Creating xml file ");
                xmlWriter = new XmlTextWriter(displayFileName, System.Text.Encoding.UTF8);
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                xmlWriter.WriteStartElement("DisplayFileStart");
                xmlWriter.Close();
                xmlDoc.Load(displayFileName);



                // Create Document Root Element
                XmlNode DisplayFileStart = xmlDoc.DocumentElement; // root node
                processTest(fileStr, conditionRegex);
                //   Console.WriteLine("Processing Global TrialParameters ..........");
                processGlobalParameters(geLogFileName, fileStr, geLogStartRegex, DisplayFileStart, xmlDoc);

                //   Console.WriteLine("Processing Condition..........");
                processCondition(geLogFileName, fileStr, conditionRegex, DisplayFileStart, xmlDoc);

                //   Console.WriteLine("Processing Trial Parameters..........");
                processTrialStart(geLogFileName, DisplayFileStart, xmlDoc, edfFileName);

                //   Console.WriteLine("Processing Subject Targets..........");
                processSubjTargets(geLogFileName, fileStr, subjTargetStartRegex, DisplayFileStart, xmlDoc);
                //   Console.WriteLine("Processing Complete");

                xmlDoc.Save(displayFileName);
                //   Console.WriteLine(" NOTE : Display File saved to  " + displayFileName);
            }
            catch (Exception e1)
            {
                //Since there is an exception, Undo everything written to the Display file.
                File.Delete(displayFileName);
                xmlWriter = null;
                xmlDoc = null;

                throw new ATLogicException(e1 + " : Error while Parsing Data ");
            }
        }

        /**
         * Utility to test any given regular expression on an input String
         */
        public static void processTest(String inputString, String regex)
        {

            if (inputString == null)
            {
                WriteError("Input String to Test is null ");
            }


            //  Console.WriteLine(" Processing test:");
            //from Regex using the specified pattern on the input data

            //private static string conditionRegex = @"Condition\s(\d)\r\nGridDisplay(\s)*(\d)\r\nBGA(\s)*(\d{1,3})(\s)*BGR\s(\d{1,3})(\s)*BGG(\s)*(\d{1,3})(\s)*BGB\s(\d{1,3})\r\nFGA\s(\d{1,3})(\s)*FGR\s(\d{1,3})(\s)*FGG\s(\d{1,3})(\s)*FGB(\s)*(\d{1,3})\r\nNumObjects\s(\d+)\r\nNumTargets(\s)*(\d+)";
            regex = @"Condition\s(\d+).*\r\nGridDisplay\s(\d).*\r\nBGA(\s)*(\d{1,3})(\s)*BGR\s(\d{1,3})(\s)*BGG(\s)*(\d{1,3})(\s)*BGB\s(\d{1,3}).*\r\nFGA\s(\d{1,3})(\s)*FGR\s(\d{1,3})(\s)*FGG\s(\d{1,3})(\s)*FGB(\s)*(\d{1,3}).*\r\nNumObjects\s(\d+).*\r\nNumTargets\s(\d+)";
            MatchCollection matches = Regex.Matches(inputString, regex);

            //Iterate through the matches

            foreach (Match SingleMatch in matches)
            {
                //  Console.WriteLine(SingleMatch.Value);// every single match is eye Position Captured every 2 msecs
                string eyePositionStr = SingleMatch.Value;

                GroupCollection gc = SingleMatch.Groups;
                for (int i = 1; i < gc.Count; i++)
                {
                    //  Console.WriteLine( "Printing captures for this group...");

                    CaptureCollection cc = gc[i].Captures;
                    // Console.WriteLine();
                    for (int k = 0; k < cc.Count; k++)
                    {
                        //        Console.WriteLine("i= " + i + " :: k = " + k + "  : " + cc[k].Value);

                    }
                }
            }

        }

        /**
         * 
         */
        public static void processGlobalParameters(String geLogFileName, String fileStr, String geLogStartRegex, XmlNode DisplayFileStart, XmlDocument xmlDoc)
        {
            log.WriteLine("Processing Global Parameters");
            log.Flush();
            //from Regex using the specified pattern on the input data
            MatchCollection matches = Regex.Matches(fileStr, geLogStartRegex, RegexOptions.Multiline);

            //Iterate through the matches, adding them to the list box

            foreach (Match SingleMatch in matches)
            {
                GroupCollection gc = SingleMatch.Groups;

                if (gc.Count != 4)
                {
                    WriteError("Parse Error while Obtaining Global Parameters from File : " + geLogFileName);
                    continue;

                }

                String trialName = gc[1].Captures[0].Value.Trim();
                string displayX = gc[2].Captures[0].Value;
                String displayY = gc[3].Captures[0].Value;
                
                processSingleNode("Trial", "name", trialName, DisplayFileStart, xmlDoc);
                XmlElement displayElement = addElement("Display", DisplayFileStart, xmlDoc);
                addAttribute("X", displayX, displayElement);
                addAttribute("Y", displayY, displayElement);
                log.WriteLine("DisplayX : "+displayX+" , DIsplayY : " +displayY);
                log.Flush();

            }

        }


        /**
         * 
         */
        public static XmlNode processSingleNode(String nodeName, String nodeValue, XmlNode nodeToAppend, XmlDocument xmlDoc)
        {
            XmlElement newNode = null;
            if (table.ContainsKey(nodeName))
            {
                newNode = xmlDoc.CreateElement((String)table[nodeName]);
                XmlText textNode = xmlDoc.CreateTextNode(nodeValue);
                nodeToAppend.AppendChild(newNode);
                newNode.AppendChild(textNode);
            }
            else
            {
                WriteError("Error Parsing Parameter : " + nodeName);
            }
            return newNode;

        }

        /**
         * 
         */
        public static XmlNode processSingleNode(String nodeName, String attributeName, String attributeValue, XmlNode nodeToAppend, XmlDocument xmlDoc)
        {
            XmlElement newNode = null;
            if (table.ContainsKey(nodeName))
            {
                newNode = xmlDoc.CreateElement((String)table[nodeName]);
                newNode.SetAttribute(attributeName, attributeValue);
                nodeToAppend.AppendChild(newNode);
            }
            else
            {
                WriteError("Error Parsing Parameter : " + nodeName);
            }
            return newNode;
        }


        /**
         * 
         */
        public static XmlElement addElement(String elementName, String value, XmlNode nodeToAppend, XmlDocument xmlDoc)
        {
            XmlElement newElement = null;
            if (table.ContainsKey(elementName))
            {
                newElement = xmlDoc.CreateElement((String)table[elementName]);
                newElement.SetAttribute("name", value);
                nodeToAppend.AppendChild(newElement);
            }
            else
            {
                WriteError("Error Parsing Parameter : " + elementName);
            }
            return newElement;
        }


        /**
       * 
       */
        public static XmlElement addElement(String elementName, XmlNode nodeToAppend, XmlDocument xmlDoc)
        {
            XmlElement newElement = null;
            // Console.WriteLine(" Node name : " + nodeName);
            if (table.ContainsKey(elementName))
            {
                newElement = xmlDoc.CreateElement((String)table[elementName]);
                nodeToAppend.AppendChild(newElement);
            }
            else
            {
                WriteError("Error Parsing Parameter : " + elementName);
            }

            return newElement;

        }



        /**
        * 
        */
        public static XmlNode processSingleNode(String nodeName, XmlNode nodeToAppend, XmlDocument xmlDoc)
        {
            XmlElement newNode = null;
            //Console.WriteLine(" Node name : " + nodeName);
            if (table.ContainsKey(nodeName))
            {
                newNode = xmlDoc.CreateElement((String)table[nodeName]);
                nodeToAppend.AppendChild(newNode);
            }
            else
            {
                WriteError("Error Parsing Parameter : " + nodeName);

            }
            return newNode;

        }


        /**
        * 
        */
        public static XmlElement addAttribute(String attributeVal, XmlElement elementToAppend)
        {
            // default attribute identifier = 'name'
            elementToAppend.SetAttribute("name", attributeVal);
            WriteError("Error Parsing Parameter : " + attributeVal);
            return elementToAppend;
        }

        /**
        * 
        */
        public static XmlElement addAttribute(String attributeName, String attributeVal, XmlElement nodeToAppend)
        {
            nodeToAppend.SetAttribute(attributeName, attributeVal);
            return nodeToAppend;
        }


        /**
         * 
         */
        public static XmlNode processCondition(String geLogFileName, String fileStr, String conditionRegex, XmlNode nodeToAppend, XmlDocument xmlDoc)
        {
            log.WriteLine("Processing Condition");
            log.Flush();
            XmlElement conditionNode = null;
            //  string conditionRegex = @"Condition\s(\d)\r\nGridDisplay\s(\d)\r\nBGA\s(\d{1,3})\sBGR\s(\d{1,3})\sBGG\s(\d{1,3})\sBGB\s(\d{1,3})\r\nFGA\s(\d{1,3})\sFGR\s(\d{1,3})\sFGG\s(\d{1,3})\sFGB\s(\d{1,3})\r\nNumObjects\s(\d+)\r\nNumTargets\s(\d+)";

            //from Regex using the specified pattern on the input data
            MatchCollection matches = Regex.Matches(fileStr, conditionRegex, RegexOptions.Multiline);
            conditionNode = addElement("Condition", nodeToAppend, xmlDoc);
            foreach (Match SingleMatch in matches)
            {
                //Iterate through the matches, adding them to the list box
                GroupCollection gc = SingleMatch.Groups;
                if (gc.Count != 13)
                {
                    WriteError("Parse Error for Condition Parameters from File : " + geLogFileName);
                    // throw ATException("Parse Error while Obtaining Global Parameters from File : " + geLogFileName);
                }

                String ConditionNum = gc[1].Captures[0].Value.Trim();
                string GridDisplay = gc[2].Captures[0].Value;
                String BGA = gc[3].Captures[0].Value;
                String BGR = gc[4].Captures[0].Value;
                String BGG = gc[5].Captures[0].Value;
                String BGB = gc[6].Captures[0].Value;
                String FGA = gc[7].Captures[0].Value;
                String FGR = gc[8].Captures[0].Value;
                String FGG = gc[9].Captures[0].Value;
                String FGB = gc[10].Captures[0].Value;
                String NumObjects = gc[11].Captures[0].Value;
                String NumTargets = gc[12].Captures[0].Value;

                //  String objectInfoStr = gc[13].Captures[0].Value;
                addAttribute("Num", ConditionNum, conditionNode);
                addAttribute("GridDisplay", GridDisplay, conditionNode);
                addAttribute("BGA", BGA, conditionNode);
                addAttribute("BGR", BGR, conditionNode);
                addAttribute("BGG", BGG, conditionNode);
                addAttribute("BGB", BGB, conditionNode);
                addAttribute("FGA", FGA, conditionNode);
                addAttribute("FGR", FGR, conditionNode);
                addAttribute("FGG", FGG, conditionNode);
                addAttribute("FGB", FGB, conditionNode);
                addAttribute("NumObjects", NumObjects, conditionNode);
                addAttribute("NumTargets", NumTargets, conditionNode);

                string objectInfoRegex = @"ObjStart.*\r\nObjName\s(.*)\r\nObjectFilePath\s(.*)\r\nIsTargetObject\s(\d)";
                processConditionObjectInfo(geLogFileName, fileStr, objectInfoRegex, conditionNode, xmlDoc);
            }

            return conditionNode;
        }

        /**
         * 
         */
        public static XmlNode processConditionObjectInfo(String geLogFileName, String fileStr, string objectInfoRegex, XmlNode conditionNode, XmlDocument xmlDoc)
        {
            log.WriteLine("Processing Condition Object Info");
            log.Flush();
            // string objectInfoRegex = @"ObjStart.*\r\nObjName\s(.*)\r\nObjectFilePath\s(.*)\r\nIsTargetObject\s(\d)";

            //from Regex using the specified pattern on the input data
            MatchCollection matches = Regex.Matches(fileStr, objectInfoRegex, RegexOptions.Multiline);

            foreach (Match SingleMatch in matches)
            {
                //Iterate through the matches, adding them to the list box
                GroupCollection gc = SingleMatch.Groups;
                if (gc.Count != 4)
                {
                    WriteError("Parse Error for Condition Object info Parameters from File : " + geLogFileName);
                }

                XmlElement objStartNode = addElement("Obj", conditionNode, xmlDoc);
                String ObjName = gc[1].Captures[0].Value.Trim();
                string ObjectFilePath = gc[2].Captures[0].Value.Trim();
                String IsTargetObject = gc[3].Captures[0].Value.Trim();
                addAttribute("name", ObjName, objStartNode);
                addAttribute("ObjectFilePath", ObjectFilePath, objStartNode);
                addAttribute("IsTargetObject", IsTargetObject, objStartNode);
            }
            return conditionNode;


        }

        /**
         * Process Trial Related Data.
         */
        public static XmlNode processTrialStart(String geLogFileName, XmlNode DisplayFileStartNode, XmlDocument xmlDoc, String edfFileName)
        {

            XmlNode trialStartNode = null;
            long prevGazeTimeStamp = 0;
            ObjPositionData gePrevPositionData = null;
            try
            {
                log.WriteLine("Processing Trial Start");
                log.Flush();

                Regex gazePostionRegex = new Regex(@"^(\d+)\s*(\d.*)\s*(\d.*)\s");
                trialStartNode = processSingleNode("TrialStart", DisplayFileStartNode, xmlDoc);

                GazeDataWorker gazeWorker = new GazeDataWorker(edfFileName, gazePostionRegex);
                ObjectDataWorker objectWorker = new ObjectDataWorker(geLogFileName);

                ReplayTrial.timeStampDataQueue.Clear();
                geLogTrialDataQueue.Clear();

                Thread gazeWorkerThread = new Thread(gazeWorker.doWork);
                Thread objectWorkerThread = new Thread(objectWorker.doWork);
                log.WriteLine("created threads");
                log.Flush();
                try
                {
                    gazeWorkerThread.Start();
                    objectWorkerThread.Start();
                    gazeWorkerThread.Join();
                    objectWorkerThread.Join();
                }
                catch (ThreadInterruptedException e)
                {
                    log.WriteLine(" Thread Interrupted Exception " + e.StackTrace);
                }
                catch (ThreadAbortException e)
                {
                    log.WriteLine(" ThreadAbortException : " + e.StackTrace);
                }
                catch (SystemException e)
                {
                    log.WriteLine("SystemException : " + e.StackTrace);
                }

                catch (Exception)
                {
                    log.WriteLine("Exception with Threads");
                }
                log.WriteLine(" threads completed  job");
                log.Flush();

                int edfCount = edfTrialDataQueue.Count;
                int geCount = geLogTrialDataQueue.Count;

                long relativeDifference = 0;

                long gelogFirstTimeStamp = 0;
                long gazeFirstTimeStamp = 0;
                if (edfCount > 0)
                {
                    ObjPositionData firstTimeStampData = (ObjPositionData)edfTrialDataQueue.Peek();
                    gazeFirstTimeStamp = firstTimeStampData.timeStamp;
                }


                if (geCount > 0)
                {
                    ObjPositionData geFirstData = (ObjPositionData)geLogTrialDataQueue.Peek();
                    gelogFirstTimeStamp = geFirstData.timeStamp;
                }
                if (gazeFirstTimeStamp != gelogFirstTimeStamp)
                {
                    if (gazeFirstTimeStamp > gelogFirstTimeStamp)
                    {
                        relativeDifference = gazeFirstTimeStamp - gelogFirstTimeStamp;
                    }

                }

                if (edfCount != geCount)
                {
                    log.WriteLine("WARNING : Time Stamps Obtained from EDFFile does not match the number of timeStamps in GELog ");
                    log.WriteLine("WARNING : TimeStamp Count :: edf :" + edfCount + "  , GELog : " + geCount);
                    log.Flush();
                }


                XmlNode timeStartNode = null;
                long gePrevTimeStamp = 0;
                int count = 0;
                while (edfTrialDataQueue.Count > 0) //&& geLogTrialDataQueue.Count > 0
                {
                    long edfTimeStamp = -1;
                    long gazeTimeStamp = 0;


                    if (edfTrialDataQueue.Count > 0)
                    {
                        ObjPositionData data = null;
                        do
                        {
                            data = (ObjPositionData)edfTrialDataQueue.Dequeue();
                            edfTimeStamp = data.timeStamp;
                            if (prevGazeTimeStamp == 0)
                                prevGazeTimeStamp = edfTimeStamp;

                        } while ((edfTimeStamp != prevGazeTimeStamp) && (edfTrialDataQueue.Count > 0));

                        // Obtain the Refresh rate and  calculate number of times data would have been recorded.
                        int tmpRefreshRate = 1;
                        int refreshRateFactor = 1000 / REFRESH_RATE;
                        if ((refreshRateFactor % 2) == 0)
                            tmpRefreshRate = refreshRateFactor;
                        else
                        {
                            tmpRefreshRate = refreshRateFactor - 1;
                        }

                        prevGazeTimeStamp = edfTimeStamp + (tmpRefreshRate);
                        timeStartNode = processSingleNode("Time", "stamp", edfTimeStamp.ToString(), trialStartNode, xmlDoc);
                        LinkedList<ObjectData> gazedataList = data.getobjList();
                        foreach (ObjectData objData in gazedataList)
                        {
                            String name = objData.objName;
                            String xCoord = objData.xPos.ToString();
                            String yCoord = objData.yPos.ToString();

                            XmlElement objNode = addElement("obj", timeStartNode, xmlDoc);
                            addAttribute("name", name, objNode);
                            addAttribute("x", xCoord, objNode);
                            addAttribute("y", yCoord, objNode);
                            log.WriteLine(" objname : "+name+" , x : "+xCoord+" , Y :"+yCoord);
                            log.Flush();
                        }
                        edfCount--;
                    }

                    log.WriteLine(" dequeing gelog  data ");
                    log.Flush();
                    long geTimeStamp = -1;
                    ObjPositionData geData = null;
                    //  for (int index = 0; index < speedControl * 5; index++)
                    //  {
                    if (geLogTrialDataQueue.Count > 0)
                    {
                        long geNewTimeStamp = 0;
                        geData = (ObjPositionData)geLogTrialDataQueue.Dequeue();
                        geTimeStamp = geData.timeStamp;
                        geCount--;

                    }
                    else if (gePrevPositionData != null)
                    {

                        geData = gePrevPositionData;
                        geTimeStamp = geData.timeStamp;

                    }
                   
                    if (geData != null)
                    {
                        gePrevPositionData = geData;
                        LinkedList<ObjectData> objdataList = geData.getobjList();
                        foreach (ObjectData objData in objdataList)
                        {
                            String name = objData.objName;
                            String xCoord = objData.xPos.ToString();
                            String yCoord = objData.yPos.ToString();

                            XmlElement objNode = addElement("obj", timeStartNode, xmlDoc);
                            addAttribute("name", name, objNode);
                            addAttribute("x", xCoord, objNode);
                            addAttribute("y", yCoord, objNode);
                            log.WriteLine(" objname : " + name + " , x : " + xCoord + " , Y :" + yCoord);
                            log.Flush();

                        }
                        count++;
                    }

                } //end of while loop

                geLogTrialDataQueue.Clear();
            }
            catch (Exception e)
            {
                throw new ATLogicException(" Error while processing TrialStart  parameters .   gecount : " + geLogTrialDataQueue.Count + "  , edf  count : " + edfTrialDataQueue.Count);
            }
            return trialStartNode;
        }

        /**
         * Process  The target Objects selected by the Subject. Retrieve this data and add  it to the the  Display file.
         */
        public static void processSubjTargets(String geLogFileName, string fileStr, string subjTargetStartRegex, XmlNode GELogStartNode, XmlDocument xmlDoc)
        {
            log.WriteLine("Processing Subject Target Objects");
            log.Flush();
            //string subjTargetStartRegex = @"SubjTargetStart(.*\r\n)*SubjTargetEnd";

            MatchCollection matches = Regex.Matches(fileStr, subjTargetStartRegex, RegexOptions.Multiline);

            //Iterate through the matches, adding them to the list box

            XmlNode SubjTargetStartNode = processSingleNode("SubjectTargetStart", GELogStartNode, xmlDoc);

            foreach (Match SingleMatch in matches)
            {
                //Iterate through the matches, adding them to the list box
                GroupCollection gc = SingleMatch.Groups;
                if (gc.Count <= 0)
                {
                    WriteError("Parse Error for Subject Target Object info Parameters from File : " + geLogFileName);
                }
                for (int i = 1; i < gc.Count; i++)
                {
                    CaptureCollection cc = gc[i].Captures;
                    //Console.WriteLine();
                    for (int k = 1; k < cc.Count; k++)
                    {

                        String ObjName = cc[k].Value.Trim();
                        addElement("Obj", ObjName, SubjTargetStartNode, xmlDoc);
                        log.WriteLine(" objname : " + ObjName);
                        log.Flush();

                    }
                }
            }

        }


        /**
         * Helper function to return a String value of a file.
         */
        public static string GetDataFromFile(String fileName)
        {

            string fileStr = null;
            //open a text file for reading
            try
            {
                if (File.Exists(fileName))
                {
                    StreamReader reader = File.OpenText(fileName);

                    //get all the text in the file

                    fileStr = reader.ReadToEnd();
                    //dismiss the file handle
                    reader.Close();
                    //return the text in the file
                    //  Console.WriteLine(fileStr);
                }
                else
                {
                    log.WriteLine("Error converting file :" + fileName + " to string ");
                    log.Flush();
                }
            }
            catch (Exception)
            {
                log.WriteLine("Error converting file :" + fileName + " to string ");
                log.Flush();
            }
            return fileStr;
        }

        /**
         * Routine to return a Display File name given EDF and GE Log Filenames
         */
        public static String GetDisplayFileName(String geLogFileName)
        {

            //validate Input parameters
            if (geLogFileName == null)
            {
                //  Console.WriteLine("Input parameters to getDisplayFileName is not valid : gelogFile :" + geLogFileName + " , edf Filename : " + edfFileName);
                return null;
            }
            String displayFileName = null;
            try
            {
                String tpDisplayFileName = geLogFileName;
                displayFileName = Regex.Replace(tpDisplayFileName, ".gelog$", ".display");
                return displayFileName;
            }
            catch (Exception)
            {
                throw new ATLogicException(" Error retrieving Display File Name");
            }// end of getDisplayFileName Routine


        }


        /**
         * Utility Routine to Parse an XML file and print its contents
         */
        public static void ReadFromXML(String displayFileName)
        {
            try
            {
                if (!File.Exists(displayFileName))
                {
                    WriteError("DisplayFile :" + displayFileName + " does not exist. Exit reading File ");
                    return;
                }
                XmlTextReader reader = new XmlTextReader(displayFileName);
                while (reader.Read())
                {
                    // Do some work here on the data.
                    Console.WriteLine(reader.Name);
                }
                //    Console.ReadLine();
                reader.Close();
            }
            catch (Exception e)
            {


                if (File.Exists(displayFileName))
                {
                    File.Delete(displayFileName);
                }
                throw (e);
            }

        }



        /**
        * Error Writing Routine
         * @str : Error message
         */

        public static void WriteError(string str)
        {
            // Console.WriteLine("ERROR : " + str);
            log.WriteLine("ERROR : " + str);
            log.Flush();
        }

        /**
        * Error Writing Routine
        * @e : Throwable Exception
        */
        public static void WriteError(Exception e)
        {
            // Console.WriteLine(e.StackTrace);
            // Console.WriteLine(e.Message);
            log.WriteLine(e.StackTrace);
            log.WriteLine(e.Message);
            log.Flush();

        }


        /**
         * Error Writing Routine
         * @ e: Throwable Exception
         * @str : Error Message
         */
        public static void WriteError(Exception e, String str)
        {
            // Console.WriteLine(e.StackTrace);
            // Console.WriteLine("ERROR : " + str + " : " + e.Message);
            log.WriteLine(e.StackTrace);
            log.WriteLine("ERROR : " + str + " : " + e.Message);
            log.Flush();

        }

        /**
        * Data logging Routine
        *@str : Log Message
        */
        public static void logData(String str)
        {
            log.WriteLine("MSG : " + str);
            log.Flush();
        }

        public static void cleanUp()
        {
            DisplayFileCreation.edfTrialDataQueue.Clear();
            DisplayFileCreation.geLogTrialDataQueue.Clear();
            if (log != null)
                DisplayFileCreation.log.Close();
            DisplayFileCreation.table.Clear();
        }





    }
}