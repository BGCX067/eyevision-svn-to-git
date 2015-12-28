namespace AnalysisTool
{
    partial class ReplayUserControlDialogFrm
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
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.MarkOption = new System.Windows.Forms.RadioButton();
            this.UnmarkSelected = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(31, 111);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 41);
            this.button1.TabIndex = 0;
            this.button1.Text = "Resume";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Resume_Clicked);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(193, 111);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(120, 41);
            this.button3.TabIndex = 2;
            this.button3.Text = "Stop";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Stop_Clicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.CausesValidation = false;
            this.label1.Enabled = false;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(28, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "MarkSystemTargetObjects";
            // 
            // MarkOption
            // 
            this.MarkOption.AutoSize = true;
            this.MarkOption.Location = new System.Drawing.Point(31, 53);
            this.MarkOption.Name = "MarkOption";
            this.MarkOption.Size = new System.Drawing.Size(43, 17);
            this.MarkOption.TabIndex = 5;
            this.MarkOption.TabStop = true;
            this.MarkOption.Text = "Yes";
            this.MarkOption.UseVisualStyleBackColor = true;
            this.MarkOption.CheckedChanged += new System.EventHandler(this.MarkOption_CheckedChanged);
            // 
            // UnmarkSelected
            // 
            this.UnmarkSelected.AutoSize = true;
            this.UnmarkSelected.Location = new System.Drawing.Point(31, 76);
            this.UnmarkSelected.Name = "UnmarkSelected";
            this.UnmarkSelected.Size = new System.Drawing.Size(39, 17);
            this.UnmarkSelected.TabIndex = 6;
            this.UnmarkSelected.TabStop = true;
            this.UnmarkSelected.Text = "No";
            this.UnmarkSelected.UseVisualStyleBackColor = true;
            this.UnmarkSelected.CheckedChanged += new System.EventHandler(this.UnmarkSelected_CheckedChanged);
            // 
            // ReplayUserControlDialogFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 165);
            this.Controls.Add(this.UnmarkSelected);
            this.Controls.Add(this.MarkOption);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Name = "ReplayUserControlDialogFrm";
            this.Text = "UserOptions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton MarkOption;
        private System.Windows.Forms.RadioButton UnmarkSelected;
    }
}