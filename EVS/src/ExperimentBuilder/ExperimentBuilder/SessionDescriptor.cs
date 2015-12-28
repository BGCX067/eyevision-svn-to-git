using System;
using System.Collections.Generic;
using System.Text;

namespace ExperimentBuilder
{
    /// <summary>
    /// Descripes the session for an experiment, and contains all the trial blocks for this session. 
    /// </summary>
    [Serializable]
    public class SessionDescriptor
    {
        private List<TrialDescriptor> trialList = new List<TrialDescriptor>();

        private int numberTrials = 0;

        /// <summary>
        /// Number of trial blocks for this session
        /// </summary>
        public int NumberTrials
        {
            get { return numberTrials; }
            set { numberTrials = value; }
        }

        /// <summary>
        /// A list of all trial blocks of this session
        /// </summary>
        public List<TrialDescriptor> TrialList
        {
            get
            {
                return trialList;
            }
            set
            {
                trialList = value;
            }
        }
    }
}
