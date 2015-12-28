using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace ExperimentBuilder
{
    /// <summary>
    /// Displays window where experiment data can be entered. 
    /// </summary>
    public partial class SessionForm : Form
    {
        private ExperimentDescriptor experiment = new ExperimentDescriptor();

        /// <summary>
        /// The experiment data object. 
        /// </summary>
        public ExperimentDescriptor Experiment
        {
            get
            {
                return experiment;
            }
            set
            {
                experiment = value;
            }
        }

        private string actualFilename = ""; //Actual filename used for Save-Functionality

        public SessionForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens a trial form to add a new block of trial to the List. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            //Open TrialForm
            try
            {
                TrialForm trialForm = new TrialForm(this);
                trialForm.Show();
                trialForm.Focus();
            }
            catch (Exception ex) { ; }
        }

        /// <summary>
        /// Updates the list of trials with current experiment data. 
        /// </summary>
        public void updateTrialList()
        {
            trialListView.Items.Clear();
            int count = 0;
            for (int i = 0; i < Experiment.SessionList.TrialList.Count; i++)
            {
                ListViewItem trialItem = new ListViewItem(experiment.SessionList.TrialList[i].NumberTrials.ToString(), 0);
                count += experiment.SessionList.TrialList[i].NumberTrials;
                trialItem.SubItems.Add(experiment.SessionList.TrialList[i].TrialName.ToString());
                trialItem.SubItems.Add(experiment.SessionList.TrialList[i].NumberObjects.ToString());
                trialListView.Items.AddRange(new ListViewItem[] { trialItem }); 
            }
            numberTrialsLabel.Text = count.ToString();
            experiment.NumberTrials = count;
            checkUpDownDelete();

        }

        /// <summary>
        /// Opens a new trial form. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void trialListView_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                int itemNumber = trialListView.Items.IndexOf(trialListView.SelectedItems[0]);
                TrialForm trialForm = new TrialForm(this, itemNumber);
                trialForm.Show();
                trialForm.Focus();
            }
            catch (ArgumentOutOfRangeException ae) { ;}
            catch (Exception ex) { ;}
        }

        /// <summary>
        /// Starts the Generation process of the session files. Opens a new NameForm object. After succesful creation,
        /// a MessageBox will pop up and inform the user about this. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void generateButton_Click(object sender, EventArgs e)
        {
            try
            {
                experiment.DisplayX = Int32.Parse(displayX.Text);
                experiment.DisplayY = Int32.Parse(displayY.Text);
                experiment.NumberSessionFiles = Int32.Parse(numberSessionFilesTextBox.Text);
                int count = 0;
                for (int i = 0; i < Experiment.SessionList.TrialList.Count; i++)
                {
                    count += experiment.SessionList.TrialList[i].NumberTrials;
                }
                experiment.SessionList.NumberTrials = count;
                NameForm nameForm = new NameForm(experiment);;
                if (nameForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Experiment creation succesful.", "Experiment Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (FormatException fe)
            {
                ;
            }
        }

        /// <summary>
        /// Checks regulary if the generate Button can be enable (that means, that enough data is entered. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void checkButtonsTimer_Tick(object sender, EventArgs e)
        {
            bool generate= false;
            try
            {
                if (Int32.Parse(displayX.Text) > 0
                    && Int32.Parse(displayY.Text) > 0
                    && Int32.Parse(numberSessionFilesTextBox.Text) > 0
                    && experiment.SessionList.TrialList.Count > 0)
                    generate = true;
            }
            catch (FormatException fe) { ; }
            
            if (generateButton.Enabled != generate) generateButton.Enabled = generate;
            
         }

        /// <summary>
        /// Calls the save or save-as function, depending if a filename is defined. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!actualFilename.Equals(""))
            {
                FileGenerator.getInstance().saveExperiment(experiment, actualFilename);
            }
            else
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
        }

        /// <summary>
        /// Calls the save-as function, after displaying the save-as file dialog.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileGenerator.getInstance().saveExperiment(experiment, saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Displays the open-file dialog and load the experiment data in this file. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    actualFilename = openFileDialog.FileName;
                    experiment = FileGenerator.getInstance().loadExperiment(actualFilename);
                }
                updateTrialList();
            }
            catch (Exception ex)
            {
                actualFilename = "";
            }
        }

        /// <summary>
        /// Calls the delete-Function for the selected object.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteSelectedItem();
            
        }

        /// <summary>
        /// Deletes the selected item from the experiment-List.
        /// </summary>
        private void deleteSelectedItem()
        {
            try
            {
                int itemNumber = trialListView.Items.IndexOf(trialListView.SelectedItems[0]);
                experiment.SessionList.TrialList.RemoveAt(itemNumber);
                updateTrialList();
            }
            catch (ArgumentOutOfRangeException ae) { ;}
        }

        /// <summary>
        /// Swaps two trial blocks. 
        /// </summary>
        /// <param name="trialPosition1">Trial to be swapped</param>
        /// <param name="trialPosition2">Trial to be swapped</param>
        private void swapTrials(int trialPosition1, int trialPosition2)
        {
            TrialDescriptor temp_trial = experiment.SessionList.TrialList[trialPosition1];
            experiment.SessionList.TrialList[trialPosition1] = experiment.SessionList.TrialList[trialPosition2];
            experiment.SessionList.TrialList[trialPosition2] = temp_trial;
        }

        /// <summary>
        /// Calls the "Moves the selected object up"-function
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void upButton_Click(object sender, EventArgs e)
        {
            moveSelectedUp();
        }
        /// <summary>
        /// Calls the "Moves the selected object down"-function. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downButton_Click(object sender, EventArgs e)
        {
            moveSelectedDown();
        }

        /// <summary>
        /// Moves the selected object down
        /// </summary>
        private void moveSelectedDown()
        {
            try
            {
                int itemNumber = trialListView.Items.IndexOf(trialListView.SelectedItems[0]);
                swapTrials(itemNumber + 1, itemNumber);
                updateTrialList();
                trialListView.Select();
                trialListView.Items[itemNumber+1].Selected = true;

            }
            catch (ArgumentOutOfRangeException ae) {
                if (trialListView.Items.Count > 0)
                {
                    trialListView.Select();
                    trialListView.Items[trialListView.Items.Count - 1].Selected = true;
                }
            }
        }

        /// <summary>
        /// Calls delete-function for the selected item.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            deleteSelectedItem();
        }

        /// <summary>
        /// Calls moveup-function for selected trial block. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void upToolStripMenuItem_Click(object sender, EventArgs e)
        {
            moveSelectedUp();
        }

        /// <summary>
        /// Moves the selected trial block up (if possible.)
        /// </summary>
        private void moveSelectedUp()
        {
            try
            {
                int itemNumber = trialListView.Items.IndexOf(trialListView.SelectedItems[0]);
                swapTrials(itemNumber - 1, itemNumber);
                updateTrialList();
                trialListView.Select();
                trialListView.Items[itemNumber-1].Selected = true;

            }
            catch (ArgumentOutOfRangeException ae) {
                if (trialListView.Items.Count > 0)
                {
                    trialListView.Select();
                    trialListView.Items[0].Selected = true;
                }
            }
        }

        /// <summary>
        /// Calls movedown-function for selected trial block. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void downToolStripMenuItem_Click(object sender, EventArgs e)
        {
            moveSelectedDown();
        }

        /// <summary>
        /// Shows the about form when clicked in the toolstrip.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.Show();
            about.Focus();
        }

        /// <summary>
        /// Close the application when clicked in the toolstrip. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Calls the check-function when the selection changes. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void trialListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkUpDownDelete();
        }

        /// <summary>
        /// Check which buttons can be displayed
        /// </summary>
        private void checkUpDownDelete()
        {
            try
            {
                if (trialListView.SelectedItems[0] != null && trialListView.Items.Count > 0)
                {
                    deleteButton.Enabled = true;
                    upButton.Enabled = true;
                    downButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                deleteButton.Enabled = false;
                upButton.Enabled = false;
                downButton.Enabled = false;
            }
        }

    }
}