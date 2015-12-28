using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace AnalysisTool
{
    // Worker Thread that  retrieves Object position data from the specified gelog file.
    // Enqueues  the object positions along with the timestamp in user defined objects 
    // into an accessible  Queue Object
    // Usage of Concurrent Programming technique
    // Author: Smitha Madhavamurthy
    // Date: 04-15-2007
    class ObjectDataWorker
    {
    
     private String geLogFileName;
        
       /**
        * Constructor
        */
        public ObjectDataWorker(String name)
        {
            this.geLogFileName = name;
        }

        /**
         * Routine to enqueue Time stamp with Object Position Data from GELog File
         * Gets called from Thread
         */
        public void doWork()
        {

            try
            {
                
                // retrieve all Object Positions data and enqueue them
                Regex timeStartRegex = new Regex("^TimeStart");
                Regex timeEndRegex = new Regex("^TimeEnd");
                if (File.Exists(geLogFileName))
                {

                    using (StreamReader sr = new StreamReader(geLogFileName))
                    {
                        // DisplayFileCreation.WriteError("in Object Data worker  thread : doWork method : stream reader opened found");

                        String newLine = null;

                        do
                        {
                            do
                            {
                                newLine = sr.ReadLine();
                            } while (newLine != null && !(timeStartRegex.IsMatch(newLine)));

                            if (newLine == null) break;

                            // Split the new line to obtain the timeStamp data
                            String[] timeStampData = Regex.Split(newLine.Trim(), "\\s+");

                            ObjPositionData objPosData = new ObjPositionData();
                            objPosData.timeStamp = Convert.ToInt32(timeStampData[1].Trim());

                            String objDataLine;
                            // here we have a line that has Time Stamp Data 
                            // continue to read new Line and Process the line's contents until TimeEnd node is seen
                            objDataLine = sr.ReadLine();
                            while (objDataLine != null && !timeEndRegex.IsMatch(objDataLine))
                            {
                                string[] objData = Regex.Split(objDataLine, "\\s+");

                                ObjectData data = new ObjectData();
                                String objname = objData[0].Trim().Substring(0, objData[0].Trim().Length - 1);
                                String xPos = objData[1].Trim();
                                String yPos = objData[3].Trim();

                                data.objName = objname;
                                data.xPos = Convert.ToInt32(xPos);
                                data.yPos = Convert.ToInt32(yPos);

                                objPosData.addObjData(data);
                                //  DisplayFileCreation.WriteError("in Object data  worker  thread : doWork method : " + Convert.ToInt32(xPos));
                                // move on to next line      
                                objDataLine = sr.ReadLine();
                            }// end of while loop  -- reading data from a unique TimeStart node
                            if (newLine == null) break;
                            DisplayFileCreation.geLogTrialDataQueue.Enqueue(objPosData); // enqueue positions of all objects for the current TimeStamp


                        } while (newLine != null); // move on to next line  until EOF

                    }// end of reading from Stream Reader
                }
                else
                {
                    DisplayFileCreation.WriteError("Aborting processing Trial info .Invalid Input File : " + geLogFileName);

                }

            }
               
            catch (StackOverflowException e)
            {
                DisplayFileCreation.WriteError(" StackOverflowException : " + e.StackTrace);
            }
            catch (SystemException e)
            {
                DisplayFileCreation.WriteError(" SystemException : " + e.StackTrace);
            }
           
            catch (ApplicationException e)
            {
                DisplayFileCreation.WriteError(" ApplicationException : " + e.StackTrace);
            }
            
           catch( Exception ){
                
                throw;
            }
                 
        }
    }
}
