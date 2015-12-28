namespace AnalysisTool
{
    partial class AnalysisToolGUI
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
            this.selectSessionBoxForPRF = new System.Windows.Forms.TextBox();
            this.prfFileButton = new System.Windows.Forms.Button();
            this.browseSessionButtonForPRF = new System.Windows.Forms.Button();
            this.selectSessionLabel1 = new System.Windows.Forms.Label();
            this.openSessionFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.openTrialFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.selectSessionLabel = new System.Windows.Forms.Label();
            this.selectSessionBox = new System.Windows.Forms.TextBox();
            this.browseSessionButton = new System.Windows.Forms.Button();
            this.prfFileGenButton = new System.Windows.Forms.Button();
            this.fileGenerationBox = new System.Windows.Forms.GroupBox();
            this.selectTrialLabel = new System.Windows.Forms.Label();
            this.selectTrialBox = new System.Windows.Forms.TextBox();
            this.browseTrialButton = new System.Windows.Forms.Button();
            this.replayButton = new System.Windows.Forms.Button();
            this.replayBox = new System.Windows.Forms.GroupBox();
            this.ReplaySpeed = new System.Windows.Forms.GroupBox();
            this.VerySlowSpeed = new System.Windows.Forms.RadioButton();
            this.SlowSpeed = new System.Windows.Forms.RadioButton();
            this.FastSpeed = new System.Windows.Forms.RadioButton();
            this.NormalSpeed = new System.Windows.Forms.RadioButton();
            this.GenerateXLS_Click = new System.Windows.Forms.Button();
            this.fileGenerationBox.SuspendLayout();
            this.replayBox.SuspendLayout();
            this.ReplaySpeed.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectSessionBoxForPRF
            // 
            this.selectSessionBoxForPRF.Location = new System.Drawing.Point(100, 48);
            this.selectSessionBoxForPRF.Name = "selectSessionBoxForPRF";
            this.selectSessionBoxForPRF.Size = new System.Drawing.Size(191, 20);
            this.selectSessionBoxForPRF.TabIndex = 4;
            // 
            // prfFileButton
            // 
            this.prfFileButton.Location = new System.Drawing.Point(100, 80);
            this.prfFileButton.Name = "prfFileButton";
            this.prfFileButton.Size = new System.Drawing.Size(117, 32);
            this.prfFileButton.TabIndex = 3;
            this.prfFileButton.Text = "Generate .prf File";
            this.prfFileButton.UseVisualStyleBackColor = true;
            // 
            // browseSessionButtonForPRF
            // 
            this.browseSessionButtonForPRF.Location = new System.Drawing.Point(287, 36);
            this.browseSessionButtonForPRF.Name = "browseSessionButtonForPRF";
            this.browseSessionButtonForPRF.Size = new System.Drawing.Size(86, 32);
            this.browseSessionButtonForPRF.TabIndex = 2;
            this.browseSessionButtonForPRF.Text = "Browse";
            this.browseSessionButtonForPRF.UseVisualStyleBackColor = true;
            // 
            // selectSessionLabel1
            // 
            this.selectSessionLabel1.AutoSize = true;
            this.selectSessionLabel1.Location = new System.Drawing.Point(29, 51);
            this.selectSessionLabel1.Name = "selectSessionLabel1";
            this.selectSessionLabel1.Size = new System.Drawing.Size(80, 13);
            this.selectSessionLabel1.TabIndex = 0;
            this.selectSessionLabel1.Text = "Select Session:";
            // 
            // openSessionFileDialog
            // 
            this.openSessionFileDialog.FileName = "openFileDialog1";
            // 
            // selectSessionLabel
            // 
            this.selectSessionLabel.AutoSize = true;
            this.selectSessionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.selectSessionLabel.Location = new System.Drawing.Point(59, 32);
            this.selectSessionLabel.Name = "selectSessionLabel";
            this.selectSessionLabel.Size = new System.Drawing.Size(147, 13);
            this.selectSessionLabel.TabIndex = 0;
            this.selectSessionLabel.Text = "Select Experiment or Session:";
            // 
            // selectSessionBox
            // 
            this.selectSessionBox.Location = new System.Drawing.Point(62, 52);
            this.selectSessionBox.Name = "selectSessionBox";
            this.selectSessionBox.Size = new System.Drawing.Size(285, 23);
            this.selectSessionBox.TabIndex = 1;
            // 
            // browseSessionButton
            // 
            this.browseSessionButton.Location = new System.Drawing.Point(396, 47);
            this.browseSessionButton.Name = "browseSessionButton";
            this.browseSessionButton.Size = new System.Drawing.Size(86, 32);
            this.browseSessionButton.TabIndex = 2;
            this.browseSessionButton.Text = "Browse";
            this.browseSessionButton.UseVisualStyleBackColor = true;
            this.browseSessionButton.Click += new System.EventHandler(this.sessionBrowse_Click);
            // 
            // prfFileGenButton
            // 
            this.prfFileGenButton.Location = new System.Drawing.Point(62, 89);
            this.prfFileGenButton.Name = "prfFileGenButton";
            this.prfFileGenButton.Size = new System.Drawing.Size(147, 35);
            this.prfFileGenButton.TabIndex = 3;
            this.prfFileGenButton.Text = "Generate .perf File";
            this.prfFileGenButton.UseVisualStyleBackColor = true;
            this.prfFileGenButton.Click += new System.EventHandler(this.generatePerformancefile_Click);
            // 
            // fileGenerationBox
            // 
            this.fileGenerationBox.Controls.Add(this.prfFileGenButton);
            this.fileGenerationBox.Controls.Add(this.browseSessionButton);
            this.fileGenerationBox.Controls.Add(this.selectSessionBox);
            this.fileGenerationBox.Controls.Add(this.selectSessionLabel);
            this.fileGenerationBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.fileGenerationBox.Location = new System.Drawing.Point(12, 12);
            this.fileGenerationBox.Name = "fileGenerationBox";
            this.fileGenerationBox.Size = new System.Drawing.Size(537, 156);
            this.fileGenerationBox.TabIndex = 0;
            this.fileGenerationBox.TabStop = false;
            this.fileGenerationBox.Text = "Generate Performance File";
            // 
            // selectTrialLabel
            // 
            this.selectTrialLabel.AutoSize = true;
            this.selectTrialLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.selectTrialLabel.Location = new System.Drawing.Point(71, 37);
            this.selectTrialLabel.Name = "selectTrialLabel";
            this.selectTrialLabel.Size = new System.Drawing.Size(63, 13);
            this.selectTrialLabel.TabIndex = 0;
            this.selectTrialLabel.Text = "Select Trial:";
            // 
            // selectTrialBox
            // 
            this.selectTrialBox.Location = new System.Drawing.Point(56, 53);
            this.selectTrialBox.Name = "selectTrialBox";
            this.selectTrialBox.Size = new System.Drawing.Size(285, 23);
            this.selectTrialBox.TabIndex = 1;
            // 
            // browseTrialButton
            // 
            this.browseTrialButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.browseTrialButton.Location = new System.Drawing.Point(396, 48);
            this.browseTrialButton.Name = "browseTrialButton";
            this.browseTrialButton.Size = new System.Drawing.Size(86, 32);
            this.browseTrialButton.TabIndex = 2;
            this.browseTrialButton.Text = "Browse";
            this.browseTrialButton.UseVisualStyleBackColor = true;
            this.browseTrialButton.Click += new System.EventHandler(this.selectTrial_Click);
            // 
            // replayButton
            // 
            this.replayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.replayButton.Location = new System.Drawing.Point(56, 171);
            this.replayButton.Name = "replayButton";
            this.replayButton.Size = new System.Drawing.Size(159, 38);
            this.replayButton.TabIndex = 3;
            this.replayButton.Text = "Replay";
            this.replayButton.UseVisualStyleBackColor = true;
            this.replayButton.Click += new System.EventHandler(this.replay_click);
            // 
            // replayBox
            // 
            this.replayBox.Controls.Add(this.ReplaySpeed);
            this.replayBox.Controls.Add(this.GenerateXLS_Click);
            this.replayBox.Controls.Add(this.browseTrialButton);
            this.replayBox.Controls.Add(this.selectTrialBox);
            this.replayBox.Controls.Add(this.replayButton);
            this.replayBox.Controls.Add(this.selectTrialLabel);
            this.replayBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.replayBox.Location = new System.Drawing.Point(12, 159);
            this.replayBox.Name = "replayBox";
            this.replayBox.Size = new System.Drawing.Size(537, 239);
            this.replayBox.TabIndex = 1;
            this.replayBox.TabStop = false;
            this.replayBox.Text = "Replay Trial";
            // 
            // ReplaySpeed
            // 
            this.ReplaySpeed.Controls.Add(this.VerySlowSpeed);
            this.ReplaySpeed.Controls.Add(this.SlowSpeed);
            this.ReplaySpeed.Controls.Add(this.FastSpeed);
            this.ReplaySpeed.Controls.Add(this.NormalSpeed);
            this.ReplaySpeed.Location = new System.Drawing.Point(56, 82);
            this.ReplaySpeed.Name = "ReplaySpeed";
            this.ReplaySpeed.Size = new System.Drawing.Size(285, 74);
            this.ReplaySpeed.TabIndex = 5;
            this.ReplaySpeed.TabStop = false;
            this.ReplaySpeed.Text = "Select Speed of Replay";
            // 
            // VerySlowSpeed
            // 
            this.VerySlowSpeed.AutoSize = true;
            this.VerySlowSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.VerySlowSpeed.Location = new System.Drawing.Point(119, 44);
            this.VerySlowSpeed.Name = "VerySlowSpeed";
            this.VerySlowSpeed.Size = new System.Drawing.Size(84, 21);
            this.VerySlowSpeed.TabIndex = 3;
            this.VerySlowSpeed.TabStop = true;
            this.VerySlowSpeed.Text = "VerySlow";
            this.VerySlowSpeed.UseVisualStyleBackColor = true;
            this.VerySlowSpeed.CheckedChanged += new System.EventHandler(this.VerySlow_CheckedChanged);
            // 
            // SlowSpeed
            // 
            this.SlowSpeed.AutoSize = true;
            this.SlowSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.SlowSpeed.Location = new System.Drawing.Point(119, 19);
            this.SlowSpeed.Name = "SlowSpeed";
            this.SlowSpeed.Size = new System.Drawing.Size(55, 21);
            this.SlowSpeed.TabIndex = 2;
            this.SlowSpeed.TabStop = true;
            this.SlowSpeed.Text = "Slow";
            this.SlowSpeed.UseVisualStyleBackColor = true;
            this.SlowSpeed.CheckedChanged += new System.EventHandler(this.Slow_CheckedChanged);
            // 
            // FastSpeed
            // 
            this.FastSpeed.AutoSize = true;
            this.FastSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.FastSpeed.Location = new System.Drawing.Point(7, 44);
            this.FastSpeed.Name = "FastSpeed";
            this.FastSpeed.Size = new System.Drawing.Size(53, 21);
            this.FastSpeed.TabIndex = 1;
            this.FastSpeed.TabStop = true;
            this.FastSpeed.Text = "Fast";
            this.FastSpeed.UseVisualStyleBackColor = true;
            this.FastSpeed.CheckedChanged += new System.EventHandler(this.FastChecked_Changed);
            // 
            // NormalSpeed
            // 
            this.NormalSpeed.AutoSize = true;
            this.NormalSpeed.Checked = true;
            this.NormalSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.NormalSpeed.Location = new System.Drawing.Point(7, 20);
            this.NormalSpeed.Name = "NormalSpeed";
            this.NormalSpeed.Size = new System.Drawing.Size(71, 21);
            this.NormalSpeed.TabIndex = 0;
            this.NormalSpeed.TabStop = true;
            this.NormalSpeed.Text = "Normal";
            this.NormalSpeed.UseVisualStyleBackColor = true;
            this.NormalSpeed.CheckedChanged += new System.EventHandler(this.Normal_CheckedChanged);
            // 
            // GenerateXLS_Click
            // 
            this.GenerateXLS_Click.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.GenerateXLS_Click.Location = new System.Drawing.Point(342, 171);
            this.GenerateXLS_Click.Name = "GenerateXLS_Click";
            this.GenerateXLS_Click.Size = new System.Drawing.Size(140, 38);
            this.GenerateXLS_Click.TabIndex = 4;
            this.GenerateXLS_Click.Text = "Generate .xls File";
            this.GenerateXLS_Click.UseVisualStyleBackColor = true;
            this.GenerateXLS_Click.Click += new System.EventHandler(this.GenerateXLS_Click_Click);
            // 
            // AnalysisToolGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 410);
            this.Controls.Add(this.replayBox);
            this.Controls.Add(this.fileGenerationBox);
            this.Name = "AnalysisToolGUI";
            this.Text = "Analysis Tool";
            this.fileGenerationBox.ResumeLayout(false);
            this.fileGenerationBox.PerformLayout();
            this.replayBox.ResumeLayout(false);
            this.replayBox.PerformLayout();
            this.ReplaySpeed.ResumeLayout(false);
            this.ReplaySpeed.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox selectSessionBoxForPRF;
        private System.Windows.Forms.Button prfFileButton;
        private System.Windows.Forms.Button browseSessionButtonForPRF;
        private System.Windows.Forms.Label selectSessionLabel1;
        private System.Windows.Forms.OpenFileDialog openSessionFileDialog;
        private System.Windows.Forms.OpenFileDialog openTrialFileDialog;
        private System.Windows.Forms.Label selectSessionLabel;
        private System.Windows.Forms.TextBox selectSessionBox;
        private System.Windows.Forms.Button browseSessionButton;
        private System.Windows.Forms.Button prfFileGenButton;
        private System.Windows.Forms.GroupBox fileGenerationBox;
        private System.Windows.Forms.Label selectTrialLabel;
        private System.Windows.Forms.TextBox selectTrialBox;
        private System.Windows.Forms.Button browseTrialButton;
        private System.Windows.Forms.Button replayButton;
        private System.Windows.Forms.GroupBox replayBox;
        private System.Windows.Forms.Button GenerateXLS_Click;
        private System.Windows.Forms.GroupBox ReplaySpeed;
        private System.Windows.Forms.RadioButton VerySlowSpeed;
        private System.Windows.Forms.RadioButton SlowSpeed;
        private System.Windows.Forms.RadioButton FastSpeed;
        private System.Windows.Forms.RadioButton NormalSpeed;
    }
}