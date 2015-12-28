using System;
using System.Collections.Generic;
using System.Text;

namespace ExperimentBuilder
{
    /// <summary>
    /// This class is a representant for a block of trials. It stores informations like duration, number of Trials, Trial name, Background Color etc. 
    /// for each block of trials. Also it contains a list of all Objects belonging to the trial. 
    /// </summary>
    [Serializable]
    public class TrialDescriptor
    {
        private int duration = 0;
        /// <summary>
        /// The duration for each trial in this trial block. The value will store seconds. 
        /// </summary>
        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        private int numberTrials = 1;

        /// <summary>
        /// The number of trials this block contains.
        /// </summary>
        public int NumberTrials
        {
            get { return numberTrials; }
            set { numberTrials = value; }
        }

        private string trialName = "";

        /// <summary>
        /// The (hopefully) unique name to identify this trial block later in the EyeTracker generated data files. 
        /// </summary>
        public string TrialName
        {
            get { return trialName; }
            set { trialName = value; }
        }

        private int backgroundColor = 0;

        /// <summary>
        /// The background color for this block of trials. 
        /// </summary>
        public int BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        private int foregroundColor = 0;

        /// <summary>
        /// The foreground color - in this color components of the experiments like the fixation marker or the grid are painted. 
        /// </summary>
        public int ForegroundColor
        {
            get { return foregroundColor; }
            set { foregroundColor = value; }
        }

        
        private bool fixationMarker = false;

        /// <summary>
        /// Indicates whether a fixation marker should be displayed or not. 
        /// </summary>
        public bool FixationMarker
        {
            get { return fixationMarker; }
            set { fixationMarker = value; }
        }
        private bool gridDisplayHorizontal = false;

        /// <summary>
        /// Indicates whether a horizontal grid should be displayed or not. 
        /// </summary>
        public bool GridDisplayHorizontal
        {
            get { return gridDisplayHorizontal; }
            set { gridDisplayHorizontal = value; }
        }
        private bool gridDisplayVertical = false;

        /// <summary>
        /// Indicates whether a vertical grid should be displayed or not. 
        /// </summary>
        public bool GridDisplayVertical
        {
            get { return gridDisplayVertical; }
            set { gridDisplayVertical = value; }
        }
        private bool bouncing = false;

        /// <summary>
        /// Indicates if the objects should bounce off from each other. 
        /// </summary>
        public bool Bouncing
        {
            get { return bouncing; }
            set { bouncing = value; }
        }
        private int bouncingDistance = 0;

        /// <summary>
        /// Distance in pixels the objects should bounce off from each other (if the bouncing property is set.)
        /// </summary>
        public int BouncingDistance
        {
            get { return bouncingDistance; }
            set { bouncingDistance = value; }
        }
        private int startingDistance = 0;

        /// <summary>
        /// The minimum distance in pixels objects should be away from each other at the start of an experiment
        /// </summary>
        public int StartingDistance
        {
            get { return startingDistance; }
            set { startingDistance = value; }
        }

        /// <summary>
        /// The number of objects this trial block contains. 
        /// </summary>
        public int NumberObjects
        {
            get { return mObjectList.Count; }
        }

        private List<MObjectDescriptor> mObjectList = new List<MObjectDescriptor>(); 

        /// <summary>
        /// The List which contains all objects for this trial block. 
        /// </summary>
        public List<MObjectDescriptor> MObjectList
        {
            get
            {
                return mObjectList;
            }
            set
            {
                mObjectList = value;
            }
        }
    }
}
