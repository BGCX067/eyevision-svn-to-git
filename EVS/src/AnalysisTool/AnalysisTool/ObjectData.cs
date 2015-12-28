using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTool
{
    // User Defined Object to hold data pertaining to Individual Objects used in a Trial
    // Author: Smitha Madhavamurthy
    // Date: 03-15-2007

   public  class ObjectData
    {
        // private local variables
       public float xPos;
       public float yPos;
        public String objName;
       public String objFilePath;
       public int isTargetObject;


        /**
        * Properties to set and get XPosition of  Gaze Position
        */
       public float XPos
        {
             get  //get accessor method
            {
                return xPos;
            }
             set   //set accessor method
            {
                xPos = value;
            }
        }

        /**
         * Properties to set and get YPosition of Gaze Position
         */
       public float YPos
        {
            get  //get accessor method
            {
                return yPos;
            }
             set   //set accessor method
            {
                 yPos = value;
            }
        }


        /**
         * Properties to set and get obj Name
         * */
        public String ObjName
        {
             get  //get accessor method
            {
                return objName;
            }
             set   //set accessor method
            {
                objName = value;
            }
        }

        /**
          * Properties to set and get obj Name
          * */
        public String ObjFilePath
        {
            get  //get accessor method
            {
                return objFilePath;
            }
            set   //set accessor method
            {
                objFilePath = value;
            }
        }

        /**
          * Properties to set and get obj Name
          * */
       public int IsTargetObject
        {
            get  //get accessor method
            {
                return isTargetObject;
            }
            set   //set accessor method
            {
                isTargetObject = value;
            }
        }


    }
}
