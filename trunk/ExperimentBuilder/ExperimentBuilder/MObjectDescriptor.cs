using System;
using System.Collections.Generic;
using System.Text;

namespace ExperimentBuilder
{
    /// <summary>
    /// This class represents an object which is displayed in a trial. It contains information about its type (whether if it a 
    /// rectangle, an ellipse or a BMP-File), it movement and start position etc.
    /// </summary>
    [Serializable]
    public class MObjectDescriptor
    {
        private bool targetObject = false;

        /// <summary>
        /// Indicates whether the object is a target object or not. (E.g., if it blinks at the start of a trial)
        /// </summary>
        public bool TargetObject
        {
            get { return targetObject; }
            set { targetObject = value; }
        }

        private string type = "";

        /// <summary>
        /// Defines the type of the object. Can be either be "Rectangle/Square", "Ellipse/Circle" or "Bitmap File".
        /// </summary>
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        private string filename = "";

        /// <summary>
        /// If the type is "Bitmap File", the path to the File can be defined here. 
        /// </summary>
        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        private int width = 0;

        /// <summary>
        /// If the object is of type "Rectangle/Square" or "Ellipse/Circle", the width/x-radius can be set here.
        /// </summary>
        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        private int height = 0;

        /// <summary>
        /// If the object is of type "Rectangle/Square" or "Ellipse/Circle", the height/y-radius can be set here.
        /// </summary>
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        private int color = System.Drawing.Color.Red.ToArgb();

        /// <summary>
        /// If the object is of type "Rectangle/Square" or "Ellipse/Circle", the color of the object can be set here.
        /// </summary>
        public int Color
        {
            get { return color; }
            set { color = value; }
        }

        private int minSpeedVertical = 0;

        /// <summary>
        /// Defines the minimum speed for vertical movements of the object.
        /// </summary>
        public int MinSpeedVertical
        {
            get { return minSpeedVertical; }
            set { minSpeedVertical = value; }
        }
        
        private int maxSpeedVertical = 0;

        /// <summary>
        /// Defines the maximum speed for vertical movements of the object.
        /// </summary>
        public int MaxSpeedVertical
        {
            get { return maxSpeedVertical; }
            set { maxSpeedVertical = value; }
        }

        private int minSpeedHorizontal = 0;

        /// <summary>
        /// Defines the minimum speed for horizontal movements of the object. 
        /// </summary>
        public int MinSpeedHorizontal
        {
            get { return minSpeedHorizontal; }
            set { minSpeedHorizontal = value; }
        }

        private int maxSpeedHorizontal = 0;

        /// <summary>
        /// Defines the maximum speed for horizontal movement of the object. 
        /// </summary>
        public int MaxSpeedHorizontal
        {
            get { return maxSpeedHorizontal; }
            set { maxSpeedHorizontal = value; }
        }

        private int startPanel = 0;

        /// <summary>
        /// Defines, in which area of the screen (e.g, upper right/left, bottom right/left corner). Possible values are either 1, 2, 3 or 4.
        /// </summary>
        public int StartPanel
        {
            get { return startPanel; }
            set { startPanel = value; }
        }

        public MObjectDescriptor(MObjectDescriptor mObject)
        {
            this.maxSpeedHorizontal = mObject.maxSpeedHorizontal;
            this.minSpeedHorizontal = mObject.minSpeedHorizontal;
            this.maxSpeedVertical = mObject.maxSpeedVertical;
            this.minSpeedVertical = mObject.minSpeedVertical;
            this.color = mObject.color;
            this.filename = mObject.filename;
            this.height = mObject.height;
            this.startPanel = mObject.startPanel;
            this.targetObject = mObject.targetObject;
            this.type = mObject.type;
            this.width = mObject.width;
        }

        public MObjectDescriptor() : base()
        {
        }



    }
}
