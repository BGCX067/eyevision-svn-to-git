using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ExperimentBuilder
{
    public partial class NameForm : Form
    {
        ExperimentDescriptor experiment = null;

        /// <summary>
        /// Display a pop-up window which waits for a valid name of the experiment and starts the creation process.
        /// </summary>
        /// <param name="experiment">The experiment dataa</param>
        public NameForm(ExperimentDescriptor experiment)
        {
            InitializeComponent();
            this.experiment = experiment;
            FileGenerator.getInstance().createDefaultDir();
            FileGenerator.getInstance().createSubDirs();
        }

        /// <summary>
        /// When the user enters a name for the experiment, this name is verified if it valid (That means, it is checked
        /// if the folder already exists.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!fileNameTextBox.Text.Equals("") && !Directory.Exists(FileGenerator.getInstance().ExperimentDir + @"\"+ fileNameTextBox.Text))
            {
                fileExistsLabel.Text = "";
                if (!okButton.Enabled) okButton.Enabled = true;
                if (deleteButton.Enabled) deleteButton.Enabled = false;
            }
            else
            {
                if (okButton.Enabled) okButton.Enabled = false;
                if (!deleteButton.Enabled) deleteButton.Enabled = true;
                fileExistsLabel.Text = "Experiment name already exists.";
            }
        }

        /// <summary>
        /// After clicking on OK, the creation of folders and files for the experiment can be started. (In a parallel thread).
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void okButton_Click(object sender, EventArgs e)
        {
            if (!fileNameTextBox.Text.Equals("") && !Directory.Exists(FileGenerator.getInstance().DefaultDir + fileNameTextBox.Text))
            {
                okButton.Enabled = false;
                cancelButton.Enabled = false;
                fileExistsLabel.ForeColor = Color.Black;
                fileExistsLabel.Text = "Please wait while files are being created.";
                fileExistsLabel.Update();
                experiment.ExperimentName = fileNameTextBox.Text;
                experiment.ExperimentFolder = FileGenerator.getInstance().ExperimentDir+@"\"+fileNameTextBox.Text;
                backgroundWorker.RunWorkerAsync(experiment);
            }
            

        }

        /// <summary>
        /// When the experiment name already exists, the Delete Button can be used to delete that folder. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            cancelButton.Enabled = false;
            deleteButton.Enabled = false;
            deleteBackgroundWorker.RunWorkerAsync(fileNameTextBox.Text);
        }

        /// <summary>
        /// The generation of the files in a parallel Thread. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //BackgroundWorker worker = sender as BackgroundWorker;
            FileGenerator.getInstance().generateSessionFiles((ExperimentDescriptor)(e.Argument));
        }

        /// <summary>
        /// Delegate which is called after the generation of the files is completed. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Starts the deletion of an existing experiment folder. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void deleteBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //BackgroundWorker deleteWorker = sender as BackgroundWorker;
            try
            {
                Directory.Delete(FileGenerator.getInstance().ExperimentDir + @"\" + (string)e.Argument, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't delete Folder - Are still files in use?", "Error deleting folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Delegate which is called after the deletion of the folder is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cancelButton.Enabled = true;
            fileNameTextBox_TextChanged(sender, e);
        }
    }
}