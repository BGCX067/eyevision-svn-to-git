using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace ExperimentBuilder
{
    /// <summary>
    /// This class display the window for entering trial data for a block trial data. 
    /// </summary>
    public partial class TrialForm : Form
    {
        SessionForm sessionForm = null; //The session form which invoked this from

        private List<MObjectDescriptor> mObjectList = new List<MObjectDescriptor>(); //Stores all objects for this trial block.
        private TrialDescriptor trial = new TrialDescriptor(); //Stores the data for this block of trials.
        private int maxObjectsNumber = 1; //Maximum number of objects for this trial block.
        private int currentNumber = 1; //Which object is display and can be edited 
        private bool addTrial = true; //Indicates if the form adds a new block of trials or change an existing one.
        private int editNumber = 0; //If addTrial is false, then this number determines which trial block will be changed.

        private string whatsmissing = "";
        /// <summary>
        /// A new Trial Form
        /// </summary>
        public TrialForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// A new trial Form.
        /// </summary>
        /// <param name="sessionForm">The sessionForm which invoked this form.</param>
        public TrialForm(SessionForm sessionForm)
        {
            InitializeComponent();
            this.sessionForm = sessionForm;
        }

        /// <summary>
        /// A new trial Form. This constructor is used if an existing block of trial are subject to change.
        /// </summary>
        /// <param name="sessionForm">The sessionForm which invoked this form.</param>
        /// <param name="editNumber">The trial block number that has to be changed.</param>
        public TrialForm(SessionForm sessionForm,int editNumber)
        {
            addTrial = false;
            this.editNumber = editNumber;
            InitializeComponent();
            this.sessionForm = sessionForm;
            this.trial= sessionForm.Experiment.SessionList.TrialList[editNumber];
            trialNumberFromTextBox.Text = trial.NumberTrials.ToString();
            groupNameTextBox.Text = trial.TrialName;
            durationTextBox.Text = trial.Duration.ToString();
            fixationMarkerCheckBox.Checked = trial.FixationMarker;
            horizontalCheckBox.Checked = trial.GridDisplayHorizontal;
            verticalCheckBox.Checked = trial.GridDisplayVertical;
            bouncingCheckBox.Checked = trial.Bouncing;
           
            if (trial.Bouncing == true)
                bouncingTextBox.Text = trial.BouncingDistance.ToString();
            startingDistanceTextBox.Text = trial.StartingDistance.ToString();
            backgroundPictureBox.BackColor = Color.FromArgb(trial.BackgroundColor);
            foregroundColorPictureBox.BackColor = Color.FromArgb(trial.ForegroundColor);
            
            this.mObjectList = sessionForm.Experiment.SessionList.TrialList[editNumber].MObjectList;
            maxObjectsNumber = trial.NumberObjects;
            updateObjectList(maxObjectsNumber);
            checkButtonStatus();
        }

        /// <summary>
        /// Initialisation of the trial form. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void TrialForm_Load(object sender, EventArgs e)
        {
            OKButton.Enabled = false;
            if (addTrial)
                numberComboBox.Text = numberComboBox.Items[0].ToString();
            else
                numberComboBox.Text = maxObjectsNumber.ToString();
        }

        /// <summary>
        /// Updates the GUI when the number of objects for a trial block is changed. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void numberComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            maxObjectsNumber = Int32.Parse(numberComboBox.Text);
            updateObjectList(maxObjectsNumber);
            checkButtonStatus();
        }

        /// <summary>
        /// Updates the Object List. Is called when the number of objects for a trial block is changed.  
        /// </summary>
        /// <param name="max"></param>
        private void updateObjectList(int max)
        {
            if (mObjectList.Count < max)
                for (int i = mObjectList.Count; i < max; i++)
                    mObjectList.Add(new MObjectDescriptor());
            try
            {
                mObjectList.RemoveRange(max, mObjectList.Count - max);
            }
            catch (ArgumentOutOfRangeException ae) { ;}
            mObjectList.Capacity = max;
        }

        /// <summary>
        /// Adds the trial to the session Form list. (Or changes an existing one)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, EventArgs e)
        {
           bool noError=true;
           try 
           {
                trial.NumberTrials = Int32.Parse(trialNumberFromTextBox.Text);
                trial.TrialName = groupNameTextBox.Text.ToString();
                trial.Duration = Int32.Parse(durationTextBox.Text);
                trial.FixationMarker = fixationMarkerCheckBox.Checked;
                trial.GridDisplayHorizontal = horizontalCheckBox.Checked;
                trial.GridDisplayVertical = verticalCheckBox.Checked;
                trial.Bouncing = bouncingCheckBox.Checked;
                if (bouncingCheckBox.Checked) trial.BouncingDistance = Int32.Parse(bouncingTextBox.Text);
                trial.StartingDistance = Int32.Parse(startingDistanceTextBox.Text);
                trial.BackgroundColor = backgroundPictureBox.BackColor.ToArgb();
                trial.ForegroundColor = foregroundColorPictureBox.BackColor.ToArgb();
                trial.MObjectList = mObjectList;
            }
            catch (Exception ex) {
                noError = false;
            }
            if (noError)
            {
                if (addTrial)
                {
                    sessionForm.Experiment.SessionList.TrialList.Add(trial);
                }
                else
                {
                    sessionForm.Experiment.SessionList.TrialList[editNumber] = trial;
                }
                sessionForm.updateTrialList();
                this.Close();
            }           
        }

        /// <summary>
        /// Check if all necessary form fields are filled with correct values - if they are filled, enable the OK-Button.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OKButtonTimer_Tick(object sender, EventArgs e)
        {
            whatsmissing = "";
            bool chkFrm = true;
            try
            {
                /*if (
                    ( !(Int32.Parse(trialNumberFromTextBox.Text) > 0) )
                ||  ( !(Int32.Parse(durationTextBox.Text) > 0) ) 
                ||  (bouncingCheckBox.Checked && ( !(Int32.Parse(bouncingTextBox.Text)>0) ))
                ||  ( !(Int32.Parse(startingDistanceTextBox.Text)>=0) )
                ||  ( !(Int32.Parse(numberComboBox.Text)>0) )
                ||  ( (((groupNameTextBox.Text.Trim()).Length) == 0) )
                   ) 
                    chkFrm = false;*/
                try {
                    if (!(Int32.Parse(trialNumberFromTextBox.Text) > 0)) {
                        whatsmissing += "Number of trials must be bigger than 0!\n";
                        chkFrm = false; 
                    }
                } catch (FormatException ae) {
                    whatsmissing += "Number of trials must not be empty and a number!\n";
                    chkFrm = false;
                }

                try {
                    if (!(Int32.Parse(durationTextBox.Text) > 0)) {
                        whatsmissing += "Duration must be bigger than 0!\n";
                        chkFrm = false;
                    }
                }
                catch (FormatException ae)
                {
                    whatsmissing += "Duration must not be empty and a number!\n";
                    chkFrm = false;
                }

                try
                {
                    if (bouncingCheckBox.Checked && (!(Int32.Parse(bouncingTextBox.Text) > 0)))
                    {
                        whatsmissing += "Bouncing number must be bigger than 0!\n";
                        chkFrm = false;
                    }
                }
                catch (FormatException ae)
                {
                    whatsmissing += "Bouncing value must not be empty and a number!\n";
                    chkFrm = false;
                }

                try
                {
                    if (!(Int32.Parse(startingDistanceTextBox.Text) >= 0))
                    {
                        whatsmissing += "Starting distance must be bigger than 0!\n";
                        chkFrm = false;
                    }
                }
                catch (FormatException ae)
                {
                    whatsmissing += "Starting distance must not be empty and a number!\n";
                    chkFrm = false;
                }
                
                if ( ((groupNameTextBox.Text.Trim()).Length) == 0) {
                    {
                        whatsmissing += "No Trial Block name defined.\n";
                        chkFrm = false;
                    }
                }

                int count = 0;
                whatsmissing += "\n";
                foreach (MObjectDescriptor mObject in mObjectList)
                {
                    bool change = false;
                    count++;
                    /*if (
                         (mObject.MaxSpeedHorizontal == 0)
                    || (mObject.MaxSpeedVertical == 0)
                    || (mObject.MinSpeedHorizontal == 0)
                    || (mObject.MinSpeedVertical == 0)
                    || (mObject.StartPanel == 0)
                    || (mObject.Type.Equals(""))
                    || ( (mObject.Type.Contains("Rectangle") || mObject.Type.Contains("Circle")) && (mObject.Width <= 0 || mObject.Height <= 0)  )
                    || ( (mObject.Type.Contains("File") && (mObject.Filename == "") ) )
                        )
                        chkFrm = false;
                     * */
                    if ((mObject.MaxSpeedHorizontal == 0)
                    || (mObject.MaxSpeedVertical == 0)
                    || (mObject.MinSpeedHorizontal == 0)
                    || (mObject.MinSpeedVertical == 0))  
                    {
                        whatsmissing += "For object #"+count+":Invalid movement speed.\n";
                        chkFrm = false; change = true;
                    }
                    if (mObject.StartPanel == 0)
                    {
                        whatsmissing += "For object #" + count + ":No start panel defined.\n";
                        chkFrm = false; change = true;
                    }
                    if ((mObject.Type.Equals("")))
                    {
                        whatsmissing += "For object #" + count + ":No object type defined.\n";
                        chkFrm = false; change = true;
                    }
                    if (((mObject.Type.Contains("Rectangle") || mObject.Type.Contains("Circle")) && (mObject.Width <= 0 || mObject.Height <= 0)))
                    {
                        whatsmissing += "For object #" + count + ":No width or height defined.\n";
                        chkFrm = false; change = true;
                    }
                    if ( (mObject.Type.Contains("File") && (mObject.Filename == "") ))
                    {
                        whatsmissing += "For object #" + count + ":No filename defined.\n";
                        chkFrm = false; change = true;
                    }

                    if (change) whatsmissing += "\n";

                }
            }
            catch (Exception ex)
            {
                chkFrm = false;
            }
            if (chkFrm) { whatsmissing = "All necessary information entered."; }
            if (chkFrm != OKButton.Enabled) OKButton.Enabled = chkFrm;
            
        }

        /// <summary>
        /// Activates the text Box for Bouncing distance when bouncing is enabled. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void bouncingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (bouncingCheckBox.Checked) bouncingTextBox.Enabled = true;
            else bouncingTextBox.Enabled = false;
        }

        /// <summary>
        /// Changes GUI appearance corresponding to the object type selected. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void typeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (typeComboBox.Text.ToString().Contains("File")) {
                selectFileButton.Enabled = true;
                fileLabel.Enabled = true;
                fileDescriptionLabel.Enabled = true;
                widthLabel.Enabled = false;
                heightLabel.Enabled = false;
                widthTextBox.Enabled = false;
                heightTextBox.Enabled = false;
                colorPictureBox.Enabled = false;
                colorLabel.Enabled = false;
            }
            else {
                selectFileButton.Enabled = false;
                fileLabel.Enabled = false;
                fileDescriptionLabel.Enabled = false;
                widthLabel.Enabled = true;
                heightLabel.Enabled = true;
                widthTextBox.Enabled = true;
                heightTextBox.Enabled = true;
                colorPictureBox.Enabled = true;
                colorLabel.Enabled = true;
            }
            if (typeComboBox.Text.ToString().Contains("Rectangle"))
            {
                heightLabel.Text = "Height";
                widthLabel.Text = "Width";
            }
            else if (typeComboBox.Text.ToString().Contains("Circle"))
            {
                heightLabel.Text = "X-Radius";
                widthLabel.Text = "Y-Radius";
            }

            try
            {
                mObjectList[currentNumber - 1].Type = typeComboBox.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error in creating object array!", "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Open a file dialog for the BMP-File. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void selectFileButton_Click(object sender, EventArgs e)
        {
            selectTypeFileDialog.InitialDirectory = mObjectList[currentNumber - 1].Filename;

            if (selectTypeFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileLabel.Text = selectTypeFileDialog.FileName.Substring(selectTypeFileDialog.FileName.LastIndexOf("\\") + 1);
                mObjectList[currentNumber - 1].Filename = selectTypeFileDialog.FileName;
            }
        }

        /// <summary>
        /// Next object button clicked.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void nextButton_Click(object sender, EventArgs e)
        {
            currentNumber++;
            checkButtonStatus();
        }

        /// <summary>
        /// Previous object button clicked
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void prevButton_Click(object sender, EventArgs e)
        {
            currentNumber--;
            checkButtonStatus();

        }

        /// <summary>
        /// Changes which object is displayed in the GUI.
        /// </summary>
        private void checkButtonStatus()
        {
            if (currentNumber > maxObjectsNumber) currentNumber = maxObjectsNumber;
            if (maxObjectsNumber > currentNumber) nextButton.Enabled = true;
            else nextButton.Enabled = false;
            if (currentNumber > 1) prevButton.Enabled = true;
            else prevButton.Enabled = false;
            actualNumberLabel.Text = currentNumber.ToString();
            maxNumberLabel.Text = maxObjectsNumber.ToString();

            try
            {
                targetObjectCheckBox.Checked = mObjectList[currentNumber - 1].TargetObject;
                minSpeedHorizontalTextBox.Text = mObjectList[currentNumber - 1].MinSpeedHorizontal.ToString();
                minSpeedVerticalTextBox.Text = mObjectList[currentNumber - 1].MinSpeedVertical.ToString();
                maxSpeedHorizontalTextBox.Text = mObjectList[currentNumber - 1].MaxSpeedHorizontal.ToString();
                maxSpeedVerticalTextBox.Text = mObjectList[currentNumber - 1].MaxSpeedVertical.ToString();
                typeComboBox.Text = mObjectList[currentNumber - 1].Type;
                if (mObjectList[currentNumber - 1].Type.Equals("")) typeComboBox_SelectedIndexChanged(null, null);
                widthTextBox.Text = mObjectList[currentNumber - 1].Width.ToString();
                heightTextBox.Text = mObjectList[currentNumber - 1].Height.ToString();
                fileLabel.Text = mObjectList[currentNumber - 1].Filename.Substring(mObjectList[currentNumber - 1].Filename.LastIndexOf("\\") + 1);
                colorPictureBox.BackColor = Color.FromArgb(mObjectList[currentNumber - 1].Color);
                switch (mObjectList[currentNumber - 1].StartPanel)
                {
                    case 1: panel1RadioButton.Checked = true; break;
                    case 2: panel2RadioButton.Checked = true; break;
                    case 3: panel3RadioButton.Checked = true; break;
                    case 4: panel4RadioButton.Checked = true; break;
                    default: panel1RadioButton.Checked = false;
                        panel2RadioButton.Checked = false;
                        panel3RadioButton.Checked = false;
                        panel4RadioButton.Checked = false;
                        break;
                }

            }
            catch (Exception ex)
            {
                //;
            }
            
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void targetObjectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                mObjectList[currentNumber - 1].TargetObject = targetObjectCheckBox.Checked;
            }
            catch (IndexOutOfRangeException ie)
            {
                //
            }
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void minSpeedHorizontalTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mObjectList[currentNumber - 1].MinSpeedHorizontal = Int32.Parse(minSpeedHorizontalTextBox.Text);
            }
            catch (FormatException fe) {
                minSpeedHorizontalTextBox.Text = "0";
                mObjectList[currentNumber - 1].MinSpeedHorizontal = 0;
            }
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void minSpeedVerticalTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mObjectList[currentNumber - 1].MinSpeedVertical = Int32.Parse(minSpeedVerticalTextBox.Text);
            }
            catch (FormatException fe) {
                minSpeedVerticalTextBox.Text = "0";
                mObjectList[currentNumber - 1].MinSpeedVertical = 0;
            }
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void maxSpeedHorizontalTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mObjectList[currentNumber - 1].MaxSpeedHorizontal = Int32.Parse(maxSpeedHorizontalTextBox.Text);
            }
            catch (FormatException fe) {
                maxSpeedHorizontalTextBox.Text = "0";
                mObjectList[currentNumber - 1].MaxSpeedHorizontal = 0;
            }
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void maxSpeedVerticalTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mObjectList[currentNumber - 1].MaxSpeedVertical = Int32.Parse(maxSpeedVerticalTextBox.Text);
            }
            catch (FormatException fe) {
                maxSpeedVerticalTextBox.Text = "0";
                mObjectList[currentNumber - 1].MaxSpeedVertical = 0;
            }
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void panel1RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (panel1RadioButton.Checked)
                mObjectList[currentNumber - 1].StartPanel = 1;
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void panel2RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (panel2RadioButton.Checked)
                mObjectList[currentNumber - 1].StartPanel = 2;
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void panel3RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (panel3RadioButton.Checked)
                mObjectList[currentNumber - 1].StartPanel = 3;
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void panel4RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (panel4RadioButton.Checked)
                mObjectList[currentNumber - 1].StartPanel = 4;
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void widthTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mObjectList[currentNumber - 1].Width = Int32.Parse(widthTextBox.Text);
            }
            catch (FormatException fe)
            {
                widthTextBox.Text = "0";
                mObjectList[currentNumber - 1].Width = 0;
            }
        }

        /// <summary>
        /// Stores changes of objects in object list. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void heightTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mObjectList[currentNumber - 1].Height = Int32.Parse(heightTextBox.Text);
            }
            catch (FormatException fe)
            {
                heightTextBox.Text = "0";
                mObjectList[currentNumber - 1].Height = 0;
            }
        }

        /// <summary>
        /// Display color Chooser for background color. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void backgroundPictureBox_Click(object sender, EventArgs e)
        {
            // Sets the initial color select to the current text color.
            backgroundColorDialog.Color = backgroundPictureBox.BackColor;

            // Update the text box color if the user clicks OK 
            if (backgroundColorDialog.ShowDialog() == DialogResult.OK)
                backgroundPictureBox.BackColor = backgroundColorDialog.Color;
        }

        /// <summary>
        /// Display color chooser for object color. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void colorPictureBox_Click(object sender, EventArgs e)
        {
            // Sets the initial color select to the current text color.
            colorDialog.Color = colorPictureBox.BackColor;

            // Update the text box color if the user clicks OK 
            if (colorDialog.ShowDialog() == DialogResult.OK)
                colorPictureBox.BackColor = colorDialog.Color;

            mObjectList[currentNumber - 1].Color = colorPictureBox.BackColor.ToArgb();
        }

        /// <summary>
        /// Display color chooser for foreground color. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void foregroundColorPictureBox_Click(object sender, EventArgs e)
        {
            // Sets the initial color select to the current text color.
            foregroundColorDialog.Color = foregroundColorPictureBox.BackColor;

            // Update the text box color if the user clicks OK 
            if (foregroundColorDialog.ShowDialog() == DialogResult.OK)
                foregroundColorPictureBox.BackColor = foregroundColorDialog.Color;
        }

        /// <summary>
        /// Selects all data when text box is clicked. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void minSpeedHorizontalTextBox_Click(object sender, EventArgs e)
        {
            minSpeedHorizontalTextBox.SelectAll();
        }

        /// <summary>
        /// Selects all data when text box is clicked. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void minSpeedVerticalTextBox_Click(object sender, EventArgs e)
        {
            minSpeedVerticalTextBox.SelectAll();
        }

        /// <summary>
        /// Selects all data when text box is clicked. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void maxSpeedHorizontalTextBox_Click(object sender, EventArgs e)
        {
            maxSpeedHorizontalTextBox.SelectAll();
        }

        /// <summary>
        /// Selects all data when text box is clicked. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void maxSpeedVerticalTextBox_Click(object sender, EventArgs e)
        {
            maxSpeedVerticalTextBox.SelectAll();
        }

        /// <summary>
        /// Selects all data when text box is clicked. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void widthTextBox_Click(object sender, EventArgs e)
        {
            widthTextBox.SelectAll();

        }

        /// <summary>
        /// Selects all data when text box is clicked. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void heightTextBox_Click(object sender, EventArgs e)
        {
            heightTextBox.SelectAll();
        }
        /// <summary>
        /// Displays a message box with the missing information
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void missingButton_Click(object sender, EventArgs e)
        {
           MessageBox.Show(this, whatsmissing, "What's missing?", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void useForAllButton_Click(object sender, EventArgs e)
        {
            MObjectDescriptor mObject = new MObjectDescriptor(mObjectList[currentNumber-1]);
           
            for (int mcount = 0; mcount < mObjectList.Count; mcount++)
            {
                if (mcount != (currentNumber - 1))
                {
                    mObjectList[mcount] = new MObjectDescriptor(mObject);
                }

            }
        }


 
    }
}