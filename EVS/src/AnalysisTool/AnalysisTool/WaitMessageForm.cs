using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnalysisTool
{
    // Simple windows Form which  displays a Progress Bar associated with the creation of an Excel file .
    // Author: Smitha Madhavamurthy
    // Date: 05-15-2007

    
    public partial class WaitMessageForm : Form
    {
        // Constructor
        public WaitMessageForm()
        {
            InitializeComponent();
        }
        
        
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            g.DrawString(" Creating Display File in Excel Format ", new Font("Times New Roman", 16), new SolidBrush(Color.Black), 40, 40);
        }

     }
}