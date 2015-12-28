using System;
using System.Collections.Generic;
using System.Text;



namespace AnalysisTool
{
    //User Defined Object to hold  timeStamp data  Objects used in a Trial. 
    // Each timestamp contains both Gaze and Objects  Position data.
    // Author: Smitha Madhavamurthy
    // Date: 04-15-2007

    class ObjPositionData
    {
        // private local variables      
        public long timeStamp ;
        public LinkedList<ObjectData> list = new LinkedList<ObjectData>();


        /**
        * Properties to set and get timeStamp of an Object Position
        */
        public long TimeStamp
        {
            get  //get accessor method
            {
                return timeStamp;
            }
            set   //set accessor method
            {
                timeStamp = value;
            }
        }

        
        /**
         * add  an object to the List
         */
        public  void addObjData(ObjectData data){

            if (data != null)
            {
                list.AddLast(data);
            }

        }

        /**
         * 
         */
        public ObjectData getObjData(String name)
        {
            foreach (ObjectData data in list)
            {
                if (data.objName == name)
                {
                    return data;
                }
            }
            return null;

        }


        public LinkedList<ObjectData> getobjList()
        {
            return list;
        }


        public void setObjList(LinkedList<ObjectData> currentList)
        {
           list =  currentList;
        }
       }
}

