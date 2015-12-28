using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace AnalysisTool
{
    // Main form which acts as  User Interface for the Application. 
    // Provides options for Performance File creation, Excel file creation and Replay of a Trial. 
    // Event Handlers have been defined for each event occuring in the Form.
    // Author : Smitha Madhavamurthy 
    // Author : Bahar Akbal
    // Date : 05-25-2007
    
    public partial class AnalysisToolGUI : Form
    {
        private Stream sessionFile = null;
        private Stream trialGELogFile = null;
        public int DISPLAY_COMPLETE = 0;
        private bool normal_speed = true; // by default , Replay is done in Normal Speed
        private bool slow_speed = false; // user needs to chose this  radio button to activate slow replay
        private bool very_slow_speed = false; // user needs to chose this  radio button to activate slow replay
        private bool fast_speed = false;
        
        public AnalysisToolGUI()
        {
            InitializeComponent();
        }


        /**
         * Event to handle "Generate .perf File" button click.
         * Gets the folder path given by the user and calls iterateOverTrials() 
         * method to iterate over the subfolders and files under that path to
         * create .perf files for each session in the selected set.
         */
        private void generatePerformancefile_Click(Object sender, System.EventArgs e)
        {
            if (this.selectSessionBox.Text == "")
            {
                MessageBox.Show("To create a performance file for each " +
                    "session under an experiment: select an experiment folder. \n\n" +
                    "To create a performance file for a specific session:" +
                    " select a session folder under an experiment.",
                "Experiment/Session Folder Selection Warning", MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(this.selectSessionBox.Text);
                ATUtil.IsSessionDirectory(dir);
                PerformanceFileCreation.iterateOverTrials(dir, null);
                if (PerformanceFileCreation.isNewPerfFileOpen())
                {
                    MessageBox.Show("Performance File : " + PerformanceFileCreation.PerformanceFilename + " created.");
                }
                PerformanceFileCreation.setPerfFileStatus(false);
                this.selectSessionBox.Clear();
                sessionFile = null;
            }

        }

        /**
         * Event Handler to handle functionality to open file Dialog to chose a Trial File
         * Assumption : The trial files are foudn at a specific pre determined Location
         */
        private void selectTrial_Click(object sender, System.EventArgs e)
        {

            openTrialFileDialog.InitialDirectory =
                "C:\\EVS\\experiments\\";
            openTrialFileDialog.Filter = "trial files (*.gelog)|*.gelog|All files (*.*)|*.*";
                    // Look for .gelog type files in teh Trial Directory
            openTrialFileDialog.FilterIndex = 1;
            openTrialFileDialog.RestoreDirectory = true;

            if (openTrialFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((trialGELogFile = openTrialFileDialog.OpenFile()) != null)
                {
                    try
                    {
                        // Insert the selected Trial's Ge Log file name into the text box
                        this.selectTrialBox.Text = openTrialFileDialog.FileName.ToString();
                        String fileName = openTrialFileDialog.FileName.ToString();
                        this.replayButton.Enabled = true;
                    }
                    catch (Exception e1)
                    {
                        System.Windows.Forms.MessageBox.Show(e1.Message);
                    }
                }
            }
        }


        /**
         * Event to handle replay button click
         * Creates a Display File and Human readable file for the input Trial if one does not exist and replays the trial 
         * Display File  - user_selected__trial_name.display  ( XML file format)
         * Display Readable File - user_selected_trial_name.xls ( Microsoft Excel file )
         * 
         * @Author : Smitha Madhavamurthy
         * @date : 04-05-2007
         * 
         */
        private void replay_click(object sender, EventArgs e)
        {
            try
            {
                ReplayTrial.cleanUp();
                DisplayFileCreation.cleanUp();
               
                // validate the input File name chosen by the User
                String geLogFileName = this.selectTrialBox.Text;
                if (this.selectTrialBox == null || this.selectTrialBox.Text == null || geLogFileName == null || geLogFileName == "")
                {
                    MessageBox.Show("Select a valid Trial file to replay");
                    this.selectTrialBox.Clear();
                    return;
                }
                String displayFileName = setUp(geLogFileName);
               // Check to see user's option of the Speed of Replay
                try
                {
                    String speedFactor = "NORMAL";
                    ReplayTrial.DisplayFileName = displayFileName;
                    if (slow_speed)
                        speedFactor = "SLOW";
                    else if (very_slow_speed)
                        speedFactor = "VERYSLOW";
                    else if (fast_speed)
                        speedFactor = "FAST";
                    else
                        speedFactor = "NORMAL";

                    // Now invoke the  routine to Start Replaying the Trial
                   ReplayTrial.display(speedFactor);

                }
                catch (SystemException e1)
                {
                    MessageBox.Show(" System Exception : Error replaying Trial : " + e1.Message);
                    MessageBox.Show(" System Exception : Error replaying Trial : " + e1.StackTrace);
                    this.selectTrialBox.Clear();
                    return;
                }

                catch (ATLogicException e2)
                {
                    MessageBox.Show("Analysis Tool Exception : Error replaying Trial : " + e2.getATLogicErrorMessage());
                    MessageBox.Show("Analysis Tool Exception : Error replaying Trial : " + e2.StackTrace);
                    this.selectTrialBox.Clear();
                    return;
                }
            }
            catch (ATLogicException e3)
            {
                MessageBox.Show("Analysis Tool Exception : Error replaying Trial : " + e3.getATLogicErrorMessage());
                MessageBox.Show("Analysis Tool Exception : Error replaying Trial : " + e3.StackTrace);
                this.selectTrialBox.Clear();
                return;
            }
            catch (SystemException e4)
            {
                MessageBox.Show(" AT Exception : Error replaying Trial : " + e4.Message);
                MessageBox.Show(" AT Exception : Error replaying Trial : " + e4.StackTrace);
                this.selectTrialBox.Clear();
                return;

            }
            catch (Exception e5) // any generic Exception
            {
                MessageBox.Show(" Generic Exception : Error replaying Trial : " + e5.Message);
                MessageBox.Show(" Generic Exception : Error replaying Trial : " + e5.StackTrace);
                this.selectTrialBox.Clear();
                return;

            }

            this.selectTrialBox.Clear(); // clears the field that holds the user selected Trial name

        }

        /**
        * Helper Routine : Retrieve EDF filename for this Session from the given GE log Filename
        * Pre condition: EDF file extension is of type .EDF and 
        * the .EDF file is stored directly under the session Directory.
        * @Author : Smitha Madhavamurthy
         * @date : 04-05-2007
        */
        private String getEDFFileName(String geLogFileName)
        {
            String edfFileName = null;

            // validate input
            if (geLogFileName == null) return null;

            DirectoryInfo info = Directory.GetParent(geLogFileName);
            DirectoryInfo trialDirectoryInfo = Directory.GetParent(geLogFileName);
            String trialDir = trialDirectoryInfo.FullName;
            DirectoryInfo sessionDirectoryInfo = Directory.GetParent(trialDir);
            String sessionDir = sessionDirectoryInfo.FullName;

            string[] files = Directory.GetFiles(sessionDir);
            foreach (String fileName in files)
            {
                if (fileName.EndsWith(".EDF") || fileName.EndsWith(".edf"))
                {
                    edfFileName = fileName;
                    break;
                }
            }
            return edfFileName;
        }


        /**
         * Helper Routine: 
         * 1. Invokes Actual worker routine to Create Gaze files.
         * 2. on successful creation of gaze files, retrieves the gaze file for the current trial.
         * 
         * @Author : Smitha Madhavamurthy
         * @date : 04-05-2007
         */
        private String createAndRetrieveGazeFile(String edfFileName, String geLogFileName)
        {
            String edfGazeFileName = null;
            try
            {
                edfGazeFileName = retrieveGazeFileName(edfFileName, geLogFileName);
                if (edfGazeFileName != null)
                {
                    return edfGazeFileName;
                }

                // invoke actual worker  routine to create gaze files
                CreateGazeFiles createGazeFiles = new CreateGazeFiles(null, null);
                int result = createGazeFiles.generateGazeFiles(edfFileName);

                // Error handling protocol between the 2 routines. 
                // Any value returned other than zero indicates an error during  Gaze files creation
                if (result != 0)
                {
                    return null;
                }

                // Now retrieve the gaze file for the current trial to be replayed.
                // Precondition : Gaze file  alogn with Ge log File is found directly under Trial Directory.

                edfGazeFileName = retrieveGazeFileName(edfFileName, geLogFileName);
                return edfGazeFileName;

            }
            catch (Exception)
            {
                return null;
            }

        }



        /**
         * Helper routine to retrieve Gaze file name given an input Edf or Ge Log file name
         * 
         */

        private string retrieveGazeFileName(String edfFilename, String geLogFileName)
        {
            String edfGazeFileName = null;
            DirectoryInfo trialDir = Directory.GetParent(geLogFileName);
            String trialDirName = trialDir.FullName;
            string[] files = Directory.GetFiles(trialDirName);
            foreach (String fileName in files)
            {
                if (fileName.EndsWith(".GAZE") || fileName.EndsWith(".gaze"))
                {
                    // get the gaze File name for this trial;
                    edfGazeFileName = fileName;
                    break;
                }
            }
            return edfGazeFileName;


        }


        /**
         * Session Browse  Button click Event Handler
         * Displays Folder Dialog Box to allow user to choose a Session File
         * 
         */
        private void sessionBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                FolderSelect folders = new FolderSelect();

                if (folders.ShowDialog() == DialogResult.OK)
                {
                    this.selectSessionBox.Text = folders.fullPath;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }



        // Routine to handle Generation of XLS  file Creation
        private void GenerateXLS_Click_Click(object sender, EventArgs e)
        {
            try
            {
                ReplayTrial.cleanUp();
                DisplayFileCreation.cleanUp();

                // validate the input File name chosen by the User
                String geLogFileName = this.selectTrialBox.Text;
                if (this.selectTrialBox == null || this.selectTrialBox.Text == null || geLogFileName == null || geLogFileName == "")
                {
                    MessageBox.Show("Select a valid Trial file to replay");
                    this.selectTrialBox.Clear();
                    return;
                }

                String xlsFileName = ATUtil.retrieveXLSFileName(geLogFileName, "GELOG");
                if(ATUtil.checkIfXLSFileExists(xlsFileName))
                {
                    MessageBox.Show("XLS File Exists");
                    this.selectTrialBox.Clear();
                    return;

                }
                String displayFileName = setUp(geLogFileName);
                // Create Human Readable File 
                WaitMessageForm waitMessageFrm = new WaitMessageForm();

                //waitMessageFrm.Location = new System.Drawing.Point(0, 0);
                waitMessageFrm.TopMost = true;
                waitMessageFrm.BringToFront();
                waitMessageFrm.Show();
                waitMessageFrm.Refresh();

                XML2XLS converter = new XML2XLS(displayFileName);
                try
                {
                    converter.convert();

                    waitMessageFrm.Hide();
                    waitMessageFrm.Close();

                    MessageBox.Show(" XML to XLS file converted Successfully");

                }
                catch (SystemException e3)
                {
                    // do nothing
                    MessageBox.Show(e3.Message);
                }
                catch (Exception e2)
                {
                    // do nothing
                    MessageBox.Show(e2.Message);
                }
                finally
                {
                    converter = null;
                }
            }
            catch (ATLogicException e3)
            {
                MessageBox.Show("Analysis Tool Exception : Error Creating XLS file : " + e3.getATLogicErrorMessage());
                MessageBox.Show("Analysis Tool Exception : Error Creating XLS file : " + e3.StackTrace);
                this.selectTrialBox.Clear();
                return;
            }
            catch (SystemException e4)
            {
                MessageBox.Show(" AT Exception :Error Creating XLS file : " + e4.Message);
                MessageBox.Show(" AT Exception : Error Creating XLS file : " + e4.StackTrace);
                this.selectTrialBox.Clear();
                return;

            }
            catch (Exception e5)
            {
                MessageBox.Show(" Generic Exception : Error Creating XLS file  : " + e5.Message);
                MessageBox.Show(" Generic Exception : Error Creating XLS file  : " + e5.StackTrace);
                this.selectTrialBox.Clear();
                return;

            }


        }



        // Routine  to do the following functions :
        // 1> Validates the file names at each step
        // 2> Checks and Creates Gaze file if not found
        // 3> checks and  creates Display File if not found
        // on ERROR : throws ATLogic Exception with a User message  Wrapped around the System message.
        private String setUp(String gelogFileName)
        {
            ReplayTrial.cleanUp();
            DisplayFileCreation.cleanUp();

            // validate the input File name chosen by the User
            String geLogFileName = this.selectTrialBox.Text;
            if (this.selectTrialBox == null || this.selectTrialBox.Text == null || geLogFileName == null || geLogFileName == "")
            {
                MessageBox.Show("Select a valid Trial file to replay");
                this.selectTrialBox.Clear();
                return null;
            }

            //  Validation Success at this point
            // retrieve EDF file( created  by Eyelink ) that is found in the session directory.
            String edfFileName = getEDFFileName(geLogFileName);
            if (edfFileName == null)
            {
                MessageBox.Show("EDF File not found  : given gelogFile " + geLogFileName);
                this.selectTrialBox.Clear();
                return null;
            }

            //1. From the EDF file obtained above,convert the file from .EDF  format to .ASC file format and 
            //2. Create Gaze Files (split the single .ASC file into as many gaze files as the number of trials foudn in the session).
            //3. Retrieve the gaze file for the Given Trial

            string edfGazeFileName = null;
            try
            {
                edfGazeFileName = createAndRetrieveGazeFile(edfFileName, geLogFileName);
            }
            catch (Exception)
            {
                MessageBox.Show("EDF Gaze File not found  : given edfFile " + edfFileName);
                this.selectTrialBox.Clear();
                return null;
            }

            if (edfGazeFileName == null)
            {
                MessageBox.Show("Error retrieving Edf Gaze File : given gelogFile " + geLogFileName);
                this.selectTrialBox.Clear();
                return null;
            }

            // At this point, we have both gaze file and gelog file required to create a display file.
            // Retrieve a  name for the Display file to be created from the given gelog file and gaze file names.
            String displayFileName = null;
            try
            {
                displayFileName = DisplayFileCreation.GetDisplayFileName(geLogFileName);
            }
            catch (Exception)
            {
                MessageBox.Show("Error Retrieving Display File Name ");
                this.selectTrialBox.Clear();
                return null;
            }

            if (displayFileName == null)
            {
                MessageBox.Show("Error Retrieving Display File Name ");
                this.selectTrialBox.Clear();
                return null;
            }

            if (!File.Exists(displayFileName))
            {
                DisplayFileCreation.create(geLogFileName, edfGazeFileName, displayFileName);
            }
            if (!File.Exists(displayFileName))
            {
                MessageBox.Show("Error Creating Display File Name :" + displayFileName);
                this.selectTrialBox.Clear();
                return displayFileName;
            }
            return displayFileName;
        }


        // Event Handler to handle Normal Speed selection for Replaying a Trial by the User
        private void Normal_CheckedChanged(object sender, EventArgs e)
        {
            if (this.NormalSpeed.Checked)
                this.normal_speed = true;
            else ResetValues();

        }

        // Event Handler to handle Slow Speed selection for Replaying a Trial by the User
        private void Slow_CheckedChanged(object sender, EventArgs e)
        {
            if (this.SlowSpeed.Checked)
                this.slow_speed = true;
            else 
                ResetValues();

        }

        // Event Handler to handle Very Slow Speed selection for Replaying a Trial by the User
        private void VerySlow_CheckedChanged(object sender, EventArgs e)
        {
            if (this.VerySlowSpeed.Checked)
                this.very_slow_speed = true;
            else
                ResetValues();
        }

        // Event Handler to handle Fast Speed selection for Replaying a Trial by the User
        private void FastChecked_Changed(object sender, EventArgs e)
        {
            if (this.FastSpeed.Checked)
                this.fast_speed = true;
            else
                ResetValues();
        }

        // Helper Routine to reset values of All private Fields to Default Values
        private void ResetValues()
        {
            this.fast_speed = false;
            this.normal_speed = true;
            this.slow_speed = false;
            this.very_slow_speed = false;
          

        }

        

    }
}
