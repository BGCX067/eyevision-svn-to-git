using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Text.RegularExpressions; 

namespace AnalysisTool
{
    /**
     * Hepler routine to enqueue Object Position Data from EDF ascii file 
     * Worker Thread that  retrieves Eye Gaze position data from the specified gaze file. 
     * Enqueues  the gaze positions alogn with the timestamp at which the gaze 
     * was recorded into an accessible  Queue Object.
     * Usage of Concurrent Programming technique
     * Author: Smitha Madhavamurthy
     * Date: 04-15-2007
     */
    public class GazeDataWorker 
    {

        private String edfFileName;
        private Regex gazePostionRegex = null;
       // Regex gazePostionRegex = new Regex(@"^(\d+)\s*(\d.*)\s*(\d.*)\s");
           

        public GazeDataWorker(String name,  Regex regex)
        {
            this.edfFileName = name;
           this.gazePostionRegex = regex;
        }
        /** 
         * Routine to enqueue Gaze position data into a Queue
         */
        public void doWork()
        {

            //local  variable
            ObjPositionData gazePrevData = null;
            float prevXpos = 0;
            float prevYPos = 0;
            DisplayFileCreation.logData("Gaze  worker  thread : Processing doWork method ");
            if(File.Exists(edfFileName))
            {
                DisplayFileCreation.logData("in gaze  worker  thread : doWork method  : edf  file exists");
            

                // retrieve all Gaze Position data and enqueue them
                using (StreamReader sr = new StreamReader(edfFileName))
                {
                   // DisplayFileCreation.WriteError("in gaze  worker  thread : doWork method : stream reader  opened");
            
                    String line;

                    do
                    {
                        line = sr.ReadLine();
                        if (line != null && gazePostionRegex.IsMatch(line))
                        {
                            // here have a line that has gaze position data
                            ObjPositionData gazeData = new ObjPositionData();
                            ObjectData data = new ObjectData();

                            String[] split = Regex.Split(line.Trim(), "\\s+");
                           
                            if (split.Length < 3)
                            {
                                continue;
                            }
                           
                          String str1 = split[0];
                          String str2 = split[1];
                          String str3 = split[2];
                          if (str2 == "." || str3 == ".")
                          {
                              
                              String timestampNew = str1;
                              ////if (gazePrevData != null)
                             // {
                              gazePrevData = new ObjPositionData();

                                  gazePrevData.TimeStamp = Convert.ToInt32(timestampNew);
                                  ObjectData thisData = new ObjectData();
                                  thisData.IsTargetObject = 0;
                                  thisData.ObjName = "gaze";
                                  thisData.XPos = prevXpos;
                                  thisData.YPos = prevYPos;
                                  gazePrevData.addObjData(thisData);

                                  DisplayFileCreation.edfTrialDataQueue.Enqueue(gazePrevData);
                             // }
                             //  * */
                           //   continue;
                              
                          }
                          else
                          {
                              String timestamp = split[0].Trim();
                              String xPos = split[1].Trim();
                              String yPos = split[2].Trim();

                              data.objName = "gaze";
                              float xpos = -1;
                              float ypos = -1;
                              try
                              {
                                  xpos = float.Parse(xPos);
                                  ypos = float.Parse(yPos);
                              }
                              catch (Exception)
                              {
                                  continue;
                              }
                              if (xpos == -1 || ypos == -1)
                              {
                                  continue;
                              }
                              else
                              {
                                  data.xPos = xpos;
                                  data.yPos = ypos;
                                  gazeData.addObjData(data);
                                  gazeData.timeStamp = Convert.ToInt32(timestamp);
                                  DisplayFileCreation.edfTrialDataQueue.Enqueue(gazeData);
                                  prevXpos = xpos;
                                  prevYPos = ypos;
                                  gazePrevData = null;
                                  gazePrevData = new ObjPositionData();
                                  gazePrevData.TimeStamp = gazeData.timeStamp;
                                  LinkedList<ObjectData> list = gazeData.getobjList();
                                  gazePrevData.setObjList(list);
                              }
                              //DisplayFileCreation.WriteError("in gaze  worker  thread : doWork method : " + Convert.ToInt32(timestamp));
                             }// else loop ends
                          }// end of if loop

                    } while (line != null);


                }// end of reading from Stream Reader
            }
            else
            {
                DisplayFileCreation.WriteError("ABORTING processing Object Position Data: Invalid input File :"+edfFileName);
            }
        } // end of enqueing Gaze data from EDF File
    }
}
