namespace ExperimentBuilder
{
    partial class SessionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SessionForm));
            this.trialListLabel = new System.Windows.Forms.Label();
            this.trialListView = new System.Windows.Forms.ListView();
            this.trialNumbers = new System.Windows.Forms.ColumnHeader();
            this.trialName = new System.Windows.Forms.ColumnHeader();
            this.trialObjects = new System.Windows.Forms.ColumnHeader();
            this.trialListContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.upToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parameterLabel = new System.Windows.Forms.Label();
            this.displayX = new System.Windows.Forms.TextBox();
            this.displayY = new System.Windows.Forms.TextBox();
            this.displayXLabel = new System.Windows.Forms.Label();
            this.displayYLabel = new System.Windows.Forms.Label();
            this.generateButton = new System.Windows.Forms.Button();
            this.checkButtonsTimer = new System.Windows.Forms.Timer(this.components);
            this.numberTrialsTextLabel = new System.Windows.Forms.Label();
            this.numberTrialsLabel = new System.Windows.Forms.Label();
            this.experimentMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.numberSessionFilesLabel = new System.Windows.Forms.Label();
            this.numberSessionFilesTextBox = new System.Windows.Forms.TextBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.downButton = new System.Windows.Forms.Button();
            this.upButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateSessionFilesLabel = new System.Windows.Forms.Label();
            this.trialListContextMenu.SuspendLayout();
            this.experimentMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // trialListLabel
            // 
            this.trialListLabel.AutoSize = true;
            this.trialListLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trialListLabel.Location = new System.Drawing.Point(18, 33);
            this.trialListLabel.Name = "trialListLabel";
            this.trialListLabel.Size = new System.Drawing.Size(245, 16);
            this.trialListLabel.TabIndex = 1;
            this.trialListLabel.Text = "List of all Trials for this Experiment";
            // 
            // trialListView
            // 
            this.trialListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.trialListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.trialNumbers,
            this.trialName,
            this.trialObjects});
            this.trialListView.ContextMenuStrip = this.trialListContextMenu;
            this.trialListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trialListView.FullRowSelect = true;
            this.trialListView.Location = new System.Drawing.Point(15, 52);
            this.trialListView.MultiSelect = false;
            this.trialListView.Name = "trialListView";
            this.trialListView.Size = new System.Drawing.Size(484, 277);
            this.trialListView.TabIndex = 2;
            this.trialListView.UseCompatibleStateImageBehavior = false;
            this.trialListView.View = System.Windows.Forms.View.Details;
            this.trialListView.DoubleClick += new System.EventHandler(this.trialListView_DoubleClick);
            this.trialListView.SelectedIndexChanged += new System.EventHandler(this.trialListView_SelectedIndexChanged);
            // 
            // trialNumbers
            // 
            this.trialNumbers.Text = "# of Trials";
            // 
            // trialName
            // 
            this.trialName.Text = "Name of Trial group";
            this.trialName.Width = 305;
            // 
            // trialObjects
            // 
            this.trialObjects.Text = "# of Objects";
            this.trialObjects.Width = 87;
            // 
            // trialListContextMenu
            // 
            this.trialListContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.upToolStripMenuItem,
            this.downToolStripMenuItem});
            this.trialListContextMenu.Name = "trialListContextMenu";
            this.trialListContextMenu.Size = new System.Drawing.Size(117, 70);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.remove_delete;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // upToolStripMenuItem
            // 
            this.upToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.arrow_up_16;
            this.upToolStripMenuItem.Name = "upToolStripMenuItem";
            this.upToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.upToolStripMenuItem.Text = "Up";
            this.upToolStripMenuItem.Click += new System.EventHandler(this.upToolStripMenuItem_Click);
            // 
            // downToolStripMenuItem
            // 
            this.downToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.arrow_down_16;
            this.downToolStripMenuItem.Name = "downToolStripMenuItem";
            this.downToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.downToolStripMenuItem.Text = "Down";
            this.downToolStripMenuItem.Click += new System.EventHandler(this.downToolStripMenuItem_Click);
            // 
            // parameterLabel
            // 
            this.parameterLabel.AutoSize = true;
            this.parameterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.parameterLabel.Location = new System.Drawing.Point(16, 363);
            this.parameterLabel.Name = "parameterLabel";
            this.parameterLabel.Size = new System.Drawing.Size(134, 16);
            this.parameterLabel.TabIndex = 4;
            this.parameterLabel.Text = "Overall Parameter";
            // 
            // displayX
            // 
            this.displayX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.displayX.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.displayX.Location = new System.Drawing.Point(134, 386);
            this.displayX.Name = "displayX";
            this.displayX.Size = new System.Drawing.Size(100, 22);
            this.displayX.TabIndex = 5;
            // 
            // displayY
            // 
            this.displayY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.displayY.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.displayY.Location = new System.Drawing.Point(134, 414);
            this.displayY.Name = "displayY";
            this.displayY.Size = new System.Drawing.Size(100, 22);
            this.displayY.TabIndex = 6;
            // 
            // displayXLabel
            // 
            this.displayXLabel.AutoSize = true;
            this.displayXLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.displayXLabel.Location = new System.Drawing.Point(18, 389);
            this.displayXLabel.Name = "displayXLabel";
            this.displayXLabel.Size = new System.Drawing.Size(92, 16);
            this.displayXLabel.TabIndex = 7;
            this.displayXLabel.Text = "Display size X";
            // 
            // displayYLabel
            // 
            this.displayYLabel.AutoSize = true;
            this.displayYLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.displayYLabel.Location = new System.Drawing.Point(18, 417);
            this.displayYLabel.Name = "displayYLabel";
            this.displayYLabel.Size = new System.Drawing.Size(93, 16);
            this.displayYLabel.TabIndex = 8;
            this.displayYLabel.Text = "Display size Y";
            // 
            // generateButton
            // 
            this.generateButton.Enabled = false;
            this.generateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.generateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generateButton.ForeColor = System.Drawing.SystemColors.Control;
            this.generateButton.Image = global::ExperimentBuilder.Properties.Resources.copy48;
            this.generateButton.Location = new System.Drawing.Point(384, 363);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(67, 58);
            this.generateButton.TabIndex = 9;
            this.generateButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.generateButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // checkButtonsTimer
            // 
            this.checkButtonsTimer.Enabled = true;
            this.checkButtonsTimer.Tick += new System.EventHandler(this.checkButtonsTimer_Tick);
            // 
            // numberTrialsTextLabel
            // 
            this.numberTrialsTextLabel.AutoSize = true;
            this.numberTrialsTextLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numberTrialsTextLabel.Location = new System.Drawing.Point(16, 332);
            this.numberTrialsTextLabel.Name = "numberTrialsTextLabel";
            this.numberTrialsTextLabel.Size = new System.Drawing.Size(127, 16);
            this.numberTrialsTextLabel.TabIndex = 10;
            this.numberTrialsTextLabel.Text = "Number of Trials:";
            // 
            // numberTrialsLabel
            // 
            this.numberTrialsLabel.AutoSize = true;
            this.numberTrialsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numberTrialsLabel.Location = new System.Drawing.Point(145, 332);
            this.numberTrialsLabel.Name = "numberTrialsLabel";
            this.numberTrialsLabel.Size = new System.Drawing.Size(15, 16);
            this.numberTrialsLabel.TabIndex = 11;
            this.numberTrialsLabel.Text = "0";
            // 
            // experimentMenuStrip
            // 
            this.experimentMenuStrip.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.experimentMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolStripMenuItem1});
            this.experimentMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.experimentMenuStrip.Name = "experimentMenuStrip";
            this.experimentMenuStrip.Size = new System.Drawing.Size(563, 24);
            this.experimentMenuStrip.TabIndex = 12;
            this.experimentMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.open;
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.ToolTipText = "Loads a previously generated set of trials";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.save;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.ToolTipText = "Saves the set of trials";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.save_as;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.ToolTipText = "Saves the set of trials in a file with specified file name";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.exit;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.ToolTipText = "Exit Experiment Builder";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.infoToolStripMenuItem});
            this.toolStripMenuItem1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(26, 20);
            this.toolStripMenuItem1.Text = "?";
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Image = global::ExperimentBuilder.Properties.Resources.help;
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.infoToolStripMenuItem.Text = "Info";
            this.infoToolStripMenuItem.Click += new System.EventHandler(this.infoToolStripMenuItem_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Experiment Builder File|*.eb|All files|*.*";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Experiment Builder File|*.eb|All files |*.*";
            // 
            // numberSessionFilesLabel
            // 
            this.numberSessionFilesLabel.AutoSize = true;
            this.numberSessionFilesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numberSessionFilesLabel.Location = new System.Drawing.Point(18, 445);
            this.numberSessionFilesLabel.Name = "numberSessionFilesLabel";
            this.numberSessionFilesLabel.Size = new System.Drawing.Size(162, 16);
            this.numberSessionFilesLabel.TabIndex = 13;
            this.numberSessionFilesLabel.Text = "How many Session Files?";
            // 
            // numberSessionFilesTextBox
            // 
            this.numberSessionFilesTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numberSessionFilesTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numberSessionFilesTextBox.Location = new System.Drawing.Point(186, 442);
            this.numberSessionFilesTextBox.Name = "numberSessionFilesTextBox";
            this.numberSessionFilesTextBox.Size = new System.Drawing.Size(48, 22);
            this.numberSessionFilesTextBox.TabIndex = 14;
            // 
            // deleteButton
            // 
            this.deleteButton.Enabled = false;
            this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.deleteButton.ForeColor = System.Drawing.SystemColors.Control;
            this.deleteButton.Image = global::ExperimentBuilder.Properties.Resources.delete48;
            this.deleteButton.Location = new System.Drawing.Point(505, 215);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(48, 48);
            this.deleteButton.TabIndex = 18;
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // downButton
            // 
            this.downButton.Enabled = false;
            this.downButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.downButton.ForeColor = System.Drawing.SystemColors.Control;
            this.downButton.Image = global::ExperimentBuilder.Properties.Resources.ardown48;
            this.downButton.Location = new System.Drawing.Point(505, 133);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(48, 48);
            this.downButton.TabIndex = 17;
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.Click += new System.EventHandler(this.downButton_Click);
            // 
            // upButton
            // 
            this.upButton.Enabled = false;
            this.upButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.upButton.ForeColor = System.Drawing.SystemColors.Control;
            this.upButton.Image = global::ExperimentBuilder.Properties.Resources.arup48;
            this.upButton.Location = new System.Drawing.Point(505, 76);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(48, 48);
            this.upButton.TabIndex = 16;
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.Click += new System.EventHandler(this.upButton_Click);
            // 
            // addButton
            // 
            this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addButton.ForeColor = System.Drawing.SystemColors.Control;
            this.addButton.Image = global::ExperimentBuilder.Properties.Resources.add48;
            this.addButton.Location = new System.Drawing.Point(505, 269);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(48, 48);
            this.addButton.TabIndex = 3;
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.testToolStripMenuItem.Text = "test";
            // 
            // generateSessionFilesLabel
            // 
            this.generateSessionFilesLabel.AutoSize = true;
            this.generateSessionFilesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generateSessionFilesLabel.Location = new System.Drawing.Point(351, 424);
            this.generateSessionFilesLabel.Name = "generateSessionFilesLabel";
            this.generateSessionFilesLabel.Size = new System.Drawing.Size(148, 16);
            this.generateSessionFilesLabel.TabIndex = 19;
            this.generateSessionFilesLabel.Text = "Generate Session Files";
            // 
            // SessionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(563, 469);
            this.Controls.Add(this.generateSessionFilesLabel);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.numberTrialsLabel);
            this.Controls.Add(this.downButton);
            this.Controls.Add(this.upButton);
            this.Controls.Add(this.numberSessionFilesLabel);
            this.Controls.Add(this.numberSessionFilesTextBox);
            this.Controls.Add(this.numberTrialsTextLabel);
            this.Controls.Add(this.trialListView);
            this.Controls.Add(this.displayYLabel);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.experimentMenuStrip);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.displayXLabel);
            this.Controls.Add(this.parameterLabel);
            this.Controls.Add(this.displayY);
            this.Controls.Add(this.trialListLabel);
            this.Controls.Add(this.displayX);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.experimentMenuStrip;
            this.Name = "SessionForm";
            this.Text = "Experiment Builder";
            this.trialListContextMenu.ResumeLayout(false);
            this.experimentMenuStrip.ResumeLayout(false);
            this.experimentMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label trialListLabel;
        private System.Windows.Forms.ListView trialListView;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label parameterLabel;
        private System.Windows.Forms.TextBox displayX;
        private System.Windows.Forms.TextBox displayY;
        private System.Windows.Forms.Label displayXLabel;
        private System.Windows.Forms.Label displayYLabel;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.ColumnHeader trialNumbers;
        private System.Windows.Forms.ColumnHeader trialName;
        private System.Windows.Forms.ColumnHeader trialObjects;
        private System.Windows.Forms.Timer checkButtonsTimer;
        private System.Windows.Forms.Label numberTrialsTextLabel;
        private System.Windows.Forms.Label numberTrialsLabel;
        private System.Windows.Forms.MenuStrip experimentMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label numberSessionFilesLabel;
        private System.Windows.Forms.TextBox numberSessionFilesTextBox;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip trialListContextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.ToolStripMenuItem upToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.Label generateSessionFilesLabel;
    }
}

