using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Collections;

namespace AnalysisTool
{
    // This Object holds all data useful for replay Functionality in data structures 
    // that provide efficient and fast data retrieval.
    // Author: Smitha Madhavamurthy
    // Date: 04-15-2007
    
    class DisplayFormPreferences
    {
         private  Color  m_FrmBackColor  = Color.White; // default
         private  Color m_FrmTextColor  = Color.Black; // default;
        private int gridDisplay;
        private int displayX;
        private int displayY;
        int bga = 255;
        int bgr = 0;
        int bgg = 0;
        int bgb = 0;

        int fga = 255;
        int fgr = 0;
        int fgg = 0;
        int fgb = 0;
        int numTargets = 0;
        List<string> targetObjects = new List<string>();

        // Adds  Target Object info
        public void addTargetObject(string objName)
        {
            targetObjects.Add(objName);
        }

        //return Target Objects info
        public List<string> getTargetObjects()
        {
            return targetObjects;
        }

        /**
       * Properties to set and get obj Name
       * */
        public int DisplayX
        {
            get  //get accessor method
            {
                return displayX;
            }
            set   //set accessor method
            {
                displayX = value;
            }
        }

        /**
       * Properties to set and get obj Name
       * */
        public int DisplayY
        {
            get  //get accessor method
            {
                return displayY;
            }
            set   //set accessor method
            {
                displayY = value;
            }
        }

        /**
       * Properties to set and get obj Name
       * */
        public int GridDisplay
        {
            get  //get accessor method
            {
                return gridDisplay;
            }
            set   //set accessor method
            {
                gridDisplay = value;
            }
        }
        /**
         * Properties to set and get obj Name
         * */
        public int BGA
        {
            get  //get accessor method
            {
                return bga;
            }
            set   //set accessor method
            {
                if (value > 0)
                {
                    bga = value;
                }
            }
        }

        /**
        * Properties to set and get obj Name
        * */
        public int BGR
        {
            get  //get accessor method
            {
                return bgr;
            }
            set   //set accessor method
            {
                bgr = value;
            }
        }

        /**
        * Properties to set and get obj Name
        * */
        public int BGG
        {
            get  //get accessor method
            {
                return bgg;
            }
            set   //set accessor method
            {
                bgg = value;
            }
        }


        /**
        * Properties to set and get obj Name
        * */
        public int BGB
        {
            get  //get accessor method
            {
                return bgb;
            }
            set   //set accessor method
            {
                bgb = value;
            }
        }

        /**
        * Properties to set and get obj Name
        * */
        public int FGA
        {
            get  //get accessor method
            {
                return fga;
            }
            set   //set accessor method
            {
                if (value > 0)
                {
                    fga = value;
                }
            }
        }
        /**
       * Properties to set and get obj Name
       * */
        public int FGR
        {
            get  //get accessor method
            {
                return fgr;
            }
            set   //set accessor method
            {
                fgr = value;
            }
        }

        /**
        * Properties to set and get obj Name
        * */
        public int FGG
        {
            get  //get accessor method
            {
                return fgg;
            }
            set   //set accessor method
            {
                fgg = value;
            }
        }


        /**
        * Properties to set and get obj Name
        * */
        public int FGB
        {
            get  //get accessor method
            {
                return fgb;
            }
            set   //set accessor method
            {
                fgb = value;
            }
        }

        public bool isTargetObject(String name)
        {
            if (name != null && targetObjects.Contains(name))
                return true;
            return false;


        }

        // Disposes all components and resouces  held by this object
        public void clear()
        {
            this.bga = 0;
            this.bgb = 0;
            this.bgg = 0;
            this.bgr = 0;
            this.displayX = 0;
            this.displayY = 0;
            this.fga = 0;
            this.fgb = 0;
            this.fgg = 0;
            this.fgr = 0;
            this.gridDisplay = 0;
           this.numTargets = 0;
           this.targetObjects.Clear();
        }

       
    }
}
