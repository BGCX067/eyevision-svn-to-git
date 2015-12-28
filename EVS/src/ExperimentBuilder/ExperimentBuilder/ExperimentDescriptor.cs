using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace ExperimentBuilder
{
    /// <summary>
    /// Contains information about the experiment and the a list of all session. To make life easier, there is only one Session 
    /// per Experiment. 
    /// </summary>
    [Serializable]
    public class ExperimentDescriptor
    {
        /*
         * This is how it is supposed to look like following RAD
         *  
            private List<SessionDescriptor> sessionList = new List<SessionDescriptor>();
            public List<SessionDescriptor> SessionList
            {
                get
                {
                    return sessionList;
                }
                set
                {
                    sessionList = value;
                }
            }
         * 
         * A 1to1 connection between Experiment and Session has the same effect
         */

        private SessionDescriptor sessionList = new SessionDescriptor();
        /// <summary>
        /// The SessionList property contains the session belonging to the experiment. It can contain only one session. (But could 
        /// be easily extended to a list later.)
        /// </summary>
        public SessionDescriptor SessionList
        {
            get
            {
                return sessionList;
            }
            set
            {
                sessionList = value;
            }
        }

        private int displayX;

        /// <summary>
        /// The width in pixel of the resolution for the experiment.
        /// </summary>
        public int DisplayX
        {
            get { return displayX; }
            set { displayX = value; }
        }

        private int displayY;

        /// <summary>
        /// The height in pixel of the resolution for the experiment.
        /// </summary>
        public int DisplayY
        {
            get { return displayY; }
            set { displayY = value; }
        }

        private int numberSessionFiles = 0;

        /// <summary>
        /// How many session files the Experiment Builder is supposed to generate
        /// </summary>
        public int NumberSessionFiles
        {
            get { return numberSessionFiles; }
            set { numberSessionFiles = value; }
        }

        private string experimentFolder= "";

        /// <summary>
        /// The folder where the experiments files are generated. 
        /// </summary>
        public string ExperimentFolder
        {
            get { return experimentFolder; }
            set { experimentFolder= value; }
        }

        private string experimentFileName = "";

        /// <summary>
        /// The file name of the experiment. 
        /// </summary>
        public string ExperimentFileName
        {
            get { return experimentFileName; }
            set { experimentFileName = value; }
        }

        private string experimentName = "";

        /// <summary>
        /// The name of the experiment. 
        /// </summary>
        public string ExperimentName
        {
            get { return experimentName; }
            set { experimentName = value; }
        }

        private int numberTrials = 0;

        /// <summary>
        /// How many trials all sessions are containingl
        /// </summary>
        public int NumberTrials
        {
            get { return numberTrials; }
            set { numberTrials = value; }
        }




    }
}
