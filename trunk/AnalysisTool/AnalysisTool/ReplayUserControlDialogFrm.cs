using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnalysisTool
{
    //Provides following user control options during the Replay of a Trial
    //•	Mark/Unmark Target Objects
    //•	Resume Replay
    //•	Stop Replay
    // Author: Smitha Madhavamurthy
    // Date: 04-25-2007

    


    public delegate void SetMarkTargetOptionDelegate(int value);
    public delegate void SetResumeReplayDelegate(int value);
    public delegate void SetPauseReplayDelegate(int value);
    public delegate void SetStopReplayDelegate(int value);


   public partial class ReplayUserControlDialogFrm : Form
    {
        // Declare delagete callback function, the owner of communication
        public SetMarkTargetOptionDelegate SetMarkTargetOptionCallback;
        public SetResumeReplayDelegate SetResumeReplayCallback;
        public SetStopReplayDelegate SetStopReplayCallBack;



        public ReplayUserControlDialogFrm()
        {
            InitializeComponent();
        }

        private void MarkTargetObjects_Changed(object sender, EventArgs e)
        {
            //Notifiy subscribers
            SetMarkTargetOptionCallback(1);

        }

        private void Stop_Clicked(object sender, EventArgs e)
        {
            //Notifiy subscribers
            SetStopReplayCallBack(1);
        }

        
        private void Resume_Clicked(object sender, EventArgs e)
        {
           //Notifiy subscribers
            SetResumeReplayCallback(1);
        }

        private void MarkOption_CheckedChanged(object sender, EventArgs e)
        {
            //Notifiy subscribers
            SetMarkTargetOptionCallback(1);
        }

        private void UnmarkSelected_CheckedChanged(object sender, EventArgs e)
        {
            //Notifiy subscribers
            SetMarkTargetOptionCallback(0);
        }
    }
}