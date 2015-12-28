
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace AnalysisTool
{
    //Parses the input  Display File  to extract data into data structures 
    //which provide easy retrieval by the Graphics  conrtoller .
    //Defines the Speed Factor . Uses the Speed Factor to skip certain number 
    //timestamps data to achieve a pre determined speed.  
    // Author: Smitha Madhavamurthy
    // Date: 04-15-2007

    


    class ReplayTrial
    {

        public static DisplayFileParser parser = null;
        public static Queue<ObjPositionData> timeStampDataQueue = new Queue<ObjPositionData>();
        public static String displayFileName = null;
        private static TextWriter log = null;
        private static ObjPositionData posData = null;
       
        public static String SPEED_FACTOR = "NORMAL";
        // If executing under Debug  mode : change this to   
            //private static String GAZE_FILE_PATH =  "..\\..\\gaze.bmp"  ;
        private static String GAZE_FILE_PATH = "gaze.bmp";
        private static int REFRESH_RATE = 12;
       
        private static int FAST = 9;
        private static int NORMAL = 7;
        private static int SLOW = 3;
        private static int VERYSLOW = 1;



        public static ObjPositionData ObjPosDataPrev
        {
            get
            {
                return posData;
            }
            set
            {
                posData = value;
            }
        }


        public static DisplayFormPreferences formSetting = new DisplayFormPreferences();

        public static Hashtable objects = new Hashtable();

        /**
         * Constructor  : accepts Display file name of the Trial to be replayed
         */
        public static String DisplayFileName
        {
            get
            {
                return displayFileName;
            }
            set
            {
                displayFileName = value;
            }
        }


        /**
         * Entry routine to display graphics and replay the trial
         * 
         */
        public static void display(String speedFactor)
        {

            if (!File.Exists(displayFileName))
            {
                log.WriteLine("display file : " + displayFileName + " does not exist : Aborting Replay");
                log.Flush();
                throw new ATLogicException("display file : " + displayFileName + " does not exist : Aborting Replay");
            }

            SPEED_FACTOR = speedFactor;

            try
            {
                try
                {
                    if (log != null)
                        log.Close();
                }
                catch (Exception ignore) { }

                String logFileName = "C:\\replay.log";


                if (File.Exists(logFileName))
                {

                    File.Delete(logFileName);
                }
                log = new StreamWriter(logFileName);


                // display file exists and Log file has been created

                //Now read the Display File and Display the Trial
                populateDisplayFormData();

                // if Validation Throws an Exception, then Replay is Aborted.
                try
                {
                    validateObjectFilePaths();
                }
                catch (ATLogicException logic)
                {
                    throw; // Fatal Erroe. Cannot replay trial
                }


                if (timeStampDataQueue.Count == 0)
                {
                    throw new ATLogicException("No time Stamp data Obtained  for Display File: " + displayFileName + "  : Aborting Replay");

                }

                int totalTimeStamps = timeStampDataQueue.Count;

                setupGraphicsWindow();
                // timeStampDataQueue.Clear();
                //   finally{
                if (parser != null)
                {
                    parser = null;
                }
                log.WriteLine(" End of Replay Trial : " + DisplayFileName);
                log.Flush();
                log.Close();
                log = null;
                //  }
            }
            catch (Exception e)
            {
                throw;

            }
            /*
         finally
         {
            // timeStampDataQueue.Clear();
             parser.close();
             parser = null;
             log.WriteLine(" End of Replay Trial : " + DisplayFileName);
             log.Flush();
             log.Close();
        
         }
              * */



        }

        private static void populateDisplayFormData()
        {
            try
            {

                parser = new DisplayFileParser(displayFileName);

                if (parser.validate())
                {
                    Hashtable displayData = parser.retrieveGlobalDisplayData();
                    if (displayData == null)
                    {
                        log.WriteLine(" display data null");
                        return;
                    }
                    if (displayData.ContainsKey("GridDisplay"))
                    {
                        formSetting.GridDisplay = Convert.ToInt32(displayData["GridDisplay"]);
                    }
                    if (displayData.ContainsKey("X"))
                    {
                        formSetting.DisplayX = Convert.ToInt32(displayData["X"]);
                    }

                    if (displayData.ContainsKey("Y"))
                    {
                        formSetting.DisplayY = Convert.ToInt32(displayData["Y"]);
                    }
                    if (displayData.ContainsKey("BGA"))
                    {
                        formSetting.BGA = Convert.ToInt32(displayData["BGA"]);
                    }
                    if (displayData.ContainsKey("BGR"))
                    {
                        formSetting.BGR = Convert.ToInt32(displayData["BGR"]);
                    }
                    if (displayData.ContainsKey("BGG"))
                    {
                        formSetting.BGG = Convert.ToInt32(displayData["BGG"]);
                    }
                    if (displayData.ContainsKey("BGB"))
                    {
                        formSetting.BGB = Convert.ToInt32(displayData["BGB"]);
                    }
                    if (displayData.ContainsKey("FGA"))
                    {
                        formSetting.FGA = Convert.ToInt32(displayData["FGA"]);
                    }
                    if (displayData.ContainsKey("FGR"))
                    {
                        formSetting.FGR = Convert.ToInt32(displayData["FGR"]);
                    }
                    if (displayData.ContainsKey("FGG"))
                    {
                        formSetting.FGG = Convert.ToInt32(displayData["FGG"]);
                    }
                    if (displayData.ContainsKey("FGB"))
                    {
                        formSetting.FGB = Convert.ToInt32(displayData["FGB"]);
                    }

                    objects.Add("gaze", "gaze.bmp");

                    System.Collections.IDictionaryEnumerator itr = displayData.GetEnumerator();
                    while (itr.MoveNext())
                    {
                        Object value = itr.Value;
                        if (value is List<ObjectData>)
                        {
                            List<ObjectData> list = (List<ObjectData>)value;
                            foreach (ObjectData thisData in list)
                            {

                                objects.Add(thisData.objName, thisData.objFilePath);
                                if (thisData.isTargetObject == 1)
                                {
                                    formSetting.addTargetObject(thisData.objName);
                                }

                                //     log.WriteLine(thisData.objName + " : " + thisData.objFilePath + " : " + thisData.isTargetObject);
                            }
                        }

                    }
                    enQueueTimeStampData();
                }
            }
            catch (Exception e)
            {
                // do nothing
                throw;
            }
            finally
            {
                parser.close();
            }
        }


        private static void enQueueTimeStampData()
        {
            try
            {
                while (!parser.isEOF())
                {
                    int speed = NORMAL; // default Speed
                    
                    if (SPEED_FACTOR.Equals("NORMAL"))
                        speed = NORMAL;
                    else if (SPEED_FACTOR.Equals("SLOW"))
                        speed = SLOW;
                    else if (SPEED_FACTOR.Equals("VERYSLOW"))
                        speed = VERYSLOW;
                       
                    else if (SPEED_FACTOR.Equals("FAST"))
                        speed = FAST;

                    ObjPositionData currentObjPos = null;
                    for (int index = 1; index <= speed && !parser.isEOF(); index++)
                    {
                        currentObjPos = parser.getNextTimeStampData();
                    }
                    if (currentObjPos != null)
                    {
                        timeStampDataQueue.Enqueue(currentObjPos);
                    }


                }
                
            }
            catch (Exception)
            {
                // do nothing
                throw;
            }

            int tmpCount = timeStampDataQueue.Count;
        }



        //test  routine
        private static void testXmlParser(string displayFileName)
        {
            DisplayFileParser parser = new DisplayFileParser(displayFileName);
            if (parser.validate())
            {
                Console.WriteLine("Success");
                Hashtable data = parser.getDisplayData();
                if (data == null)
                {
                    Console.WriteLine(" No Data found ");
                    return;
                }

                System.Collections.IDictionaryEnumerator itr = data.GetEnumerator();
                while (itr.MoveNext())
                {
                    Console.WriteLine(itr.Entry.Key + "  : " + itr.Entry.Value);
                }
            }
        }


        /**
         * 
        */
        private static void setupGraphicsWindow()
        {
            ReplayGraphics replayForm = new ReplayGraphics();
            replayForm.Size = new Size(formSetting.DisplayX, formSetting.DisplayY);
            replayForm.Location = new System.Drawing.Point(0, 0);
            replayForm.TopMost = true;

            // 
            // doubleBufferedControl
            // 
            // ReplayControlForTest doubleBufferedControl = new ReplayControlForTest();
            DoubleBufferReplayControl doubleBufferedControl = new AnalysisTool.DoubleBufferReplayControl();
            doubleBufferedControl.Location = new System.Drawing.Point(0, 0);
            doubleBufferedControl.Name = "doubleBufferedControl";

            doubleBufferedControl.Size = new System.Drawing.Size(formSetting.DisplayX, formSetting.DisplayY);

            doubleBufferedControl.BackColor = Color.FromArgb(formSetting.BGA, formSetting.BGR, formSetting.BGG, formSetting.BGB);
            doubleBufferedControl.ForeColor = Color.FromArgb(formSetting.FGA, formSetting.FGR, formSetting.FGG, formSetting.FGB);
            doubleBufferedControl.TabIndex = 0;

            replayForm.Controls.Add(doubleBufferedControl);

            //   replayForm.Activate();
            if (parser == null)
            {
                Console.WriteLine("ERROR : NullReferenceException  while displaying graphics");
                return;
            }

            try
            {
                replayForm.Show();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("ERROR : NullReferenceException  while displaying graphics" + e);
                throw;
            }


        }

        public static void cleanUp()
        {
            ReplayTrial.formSetting.clear();
            if (log != null)
                ReplayTrial.log.Close();

            ReplayTrial.objects.Clear();
            if (parser != null)
                ReplayTrial.parser.close();
            if (posData != null)
                ReplayTrial.posData = null;
            ReplayTrial.timeStampDataQueue.Clear();

        }

        /**
         * Validates if all Objects File Paths are Valid
         */
        private static void validateObjectFilePaths()
        {
            if (objects == null || objects.Count == 0)
                throw new ATLogicException("No Objects found in the Trial");

            System.Collections.IDictionaryEnumerator itr = objects.GetEnumerator();
            while (itr.MoveNext())
            {
                String objName = itr.Key.ToString();
                String objFilePath = itr.Value.ToString();
                try
                {
                    if (objFilePath == null || !File.Exists(objFilePath))
                    {
                        throw new ATLogicException("Invalid Object File path " + objFilePath + "  for Object : " + objName);
                    }
                }
                catch (Exception)
                {
                    throw new ATLogicException("Invalid Object File path +" + objFilePath + "  for Object : " + objName);
                }
            }
        }

    }



}
