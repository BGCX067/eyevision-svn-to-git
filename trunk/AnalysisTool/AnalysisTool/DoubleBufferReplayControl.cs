using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
//using System.Timers;

namespace AnalysisTool
{
    //•	Provides the core graphics handling functionality for the Replay.
    //•	Uses Timer’s Tick event to refresh the screen and repaint it.
    //•	 Uses DoubleBuffering to control Flickerign of image . 
    //•	Uses Bit map Transparency Techniques where required 
    //•	uses alpha Blending of Images
    //•	has child form “ ReplayUserControlDialogFrm.cs” 
    //•	Uses Delegates to provide Pause, resume, Mark/Unmark target objects During Replay.
    // Author: Smitha Madhavamurthy
    // Date: 04-15-2007
    public partial class DoubleBufferReplayControl : UserControl
    {
        const Bitmap NO_BACK_BUFFER = null;
        const Graphics NO_BUFFER_GRAPHICS = null;

        Bitmap BackBuffer;
        Graphics BufferGraphics;
        Timer blinkTimer;
        Timer paintTimer;
        ReplayUserControlDialogFrm dlg;

        // Fields to change Display state
        bool markTargetObjectsFlag = true; // Default option is to mark the Target Objects

        int blinkColorOn = 1;
        int maxCount = ReplayTrial.timeStampDataQueue.Count;
        int maxBlinkCount = 4;
        int blinkCount = 0;
        int count = 0;
        int singleCount = 1;


        /**
         * Constructor: Initializes the Form Component along with Setting Graphics 
         * and Timers required for the display of Graphics
         * @author Smitha Madhavamurthy
         */
        public DoubleBufferReplayControl()
        {
            InitializeComponent();

            Application.ApplicationExit +=
                new EventHandler(MemoryCleanup);

            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            initialSetup();

            //setup the blink timer object
            blinkTimer = new Timer();
            blinkTimer.Interval = 500;  //default timer interval

            blinkTimer.Enabled = true; //  enables the start of the timer

            blinkTimer.Tick += new EventHandler(Blink);
            
            //setup the timer object
            paintTimer = new Timer();
            paintTimer.Interval = 10;  //default timer interval
            paintTimer.Enabled = false; //  will ensure that this timer is not enabled
            paintTimer.Tick += new EventHandler(Tick);
            
        }

        private void initialSetup()
        {

            // Fields to change Display state
            markTargetObjectsFlag = true; // Default option is to mark the Target Objects
            blinkColorOn = 1;
            maxBlinkCount = 4;
            blinkCount = 0;
            count = 0;
            singleCount = 1;

        }


        //Controls the animation of the Blink graphics.
        private void Blink(object sender, EventArgs e)
        {
            this.blinkCount++;
            this.blinkColorOn = (this.blinkColorOn == 1) ? 0 : 1;
            if (this.blinkCount >= this.maxBlinkCount)
            {
                blinkTimer.Stop();// stop the timer
                this.blinkCount++; // reset the values
                ReplayTrial.ObjPosDataPrev = null;
                paintTimer.Enabled = true;// enable the start of replay graphics timer
                this.Invalidate();
                this.Update();
            }
            else
            { //repaint the control   
                this.Invalidate();
                this.Update();
            }
        }


        //Controls the animation of the graphics.
        private void Tick(object sender, EventArgs e)
        {
            this.count++;
            if (count > maxCount)
            {
                try
                {
                    paintTimer.Enabled = false; // will ensure that this timer is disabled
                    this.paintTimer.Stop();
                    this.paintTimer.Dispose();
                    this.blinkTimer.Stop();
                    this.blinkTimer.Dispose();

                    initialSetup();
                }
                catch (Exception) { }
                finally
                {
                    this.Dispose(true);
                    Application.Exit();
                }

            }
            else
            {  //repaint the control   
                this.Invalidate();
                this.Update();
            }
        }

        private void MemoryCleanup(object sender, EventArgs e)
        {
            // clean up the memory
            if (BackBuffer != NO_BACK_BUFFER)
                BackBuffer.Dispose();

            if (BufferGraphics != NO_BUFFER_GRAPHICS)
                BufferGraphics.Dispose();
        }


        protected override void OnPaint(PaintEventArgs pe)
        {
            // we draw the Bitmap into the image in 
            // the memory

            BackBuffer = new Bitmap(this.Width, this.Height);
            BufferGraphics = Graphics.FromImage(BackBuffer);

            DrawTemplate(BufferGraphics);
            if (this.blinkCount <= this.maxBlinkCount)
            {
                try
                {
                    if (singleCount == 1)
                    {
                        if (ReplayTrial.timeStampDataQueue.Count > 0)
                        {
                            ObjPositionData posData = ReplayTrial.timeStampDataQueue.Dequeue();
                            ReplayTrial.ObjPosDataPrev = posData;

                        }
                        singleCount++;
                    }

                    DrawBlinkingObjects(BufferGraphics);

                }
                catch (SystemException e)
                {
                    // do nothing
                    // continue to next iteration
                    throw;
                }
            }
            else
            {
                try
                {
                    DrawReplayGraphics(BufferGraphics);


                }
                catch (SystemException)
                {
                    // do nothing
                    // continue to next iteration
                    throw;
                }
            }

            // now we draw the image into the screen
            BufferGraphics.Dispose();
            pe.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }


        private void DrawTemplate(Graphics g)
        {
            //1.  Draw Grid Display as defined in the Trial
            try
            {
                Color formForeColor = this.ForeColor;
                
                int gridDisplay = ReplayTrial.formSetting.GridDisplay;
                if (gridDisplay == 0)
                {
                    // no  grid
                }
                else if (gridDisplay == 1)
                {
                    // horizontal
                    g.DrawLine(new Pen(formForeColor), new Point(0, BackBuffer.Height / 2), new Point(BackBuffer.Width, BackBuffer.Height / 2));

                }
                else if (gridDisplay == 2)
                {
                    // vertical
                    g.DrawLine(new Pen(formForeColor), new Point(BackBuffer.Width / 2, 0), new Point(BackBuffer.Width / 2, BackBuffer.Height));
                }
                else if (gridDisplay == 3)
                {
                    // both
                    g.DrawLine(new Pen(formForeColor), new Point(0, BackBuffer.Height / 2), new Point(BackBuffer.Width, BackBuffer.Height / 2));
                    g.DrawLine(new Pen(formForeColor), new Point(BackBuffer.Width / 2, 0), new Point(BackBuffer.Width / 2, BackBuffer.Height));

                }
            }
            catch (SystemException)
            {
                // do nothing
                // continue to next iteration
                //throw;
            }
            catch (Exception)
            {
                // do nothing. 
                // continue with the Display
                // throw;
            }
        }

        private void DoubleBufferedControl_Resize(object sender, EventArgs e)
        {
            if (BackBuffer != NO_BACK_BUFFER)
                BackBuffer.Dispose();

            BackBuffer = new Bitmap(this.Width, this.Height);
            BufferGraphics = Graphics.FromImage(BackBuffer);
            this.Refresh();
        }


        private void DrawBlinkingObjects(Graphics g)
        {
            try
            {
                if (ReplayTrial.ObjPosDataPrev != null)
                {
                    ObjPositionData posData = ReplayTrial.ObjPosDataPrev;
                    long timeStamp = posData.timeStamp;
                    LinkedList<ObjectData> objList = posData.getobjList();
                    IEnumerator<ObjectData> itr = objList.GetEnumerator();
                    while (itr.MoveNext())
                    {
                        ObjectData obj = (ObjectData)itr.Current;
                        string name = obj.objName;
                        string objFilePath;
                        int xpos = 0;
                        int ypos = 0;

                        bool isTargetObject = ReplayTrial.formSetting.getTargetObjects().Contains(name);

                        if (ReplayTrial.objects.ContainsKey(name))
                        {
                            objFilePath = (string)ReplayTrial.objects[name];
                            Bitmap objBmp = new Bitmap((String)objFilePath);
                            xpos = Convert.ToInt32(obj.xPos);
                            ypos = Convert.ToInt32(obj.yPos);
                            if (isTargetObject)
                            {
                                if (this.blinkColorOn == 1)
                                {
                                    g.DrawImage(objBmp, Convert.ToInt32(obj.xPos), Convert.ToInt32(obj.yPos), objBmp.Width, objBmp.Height);
                                }
                            }
                            else if (name == "gaze")
                            {
                                objBmp.MakeTransparent(objBmp.GetPixel(1, 1));
                                g.DrawImage(objBmp, Convert.ToInt32(obj.xPos), Convert.ToInt32(obj.yPos), objBmp.Width, objBmp.Height);
                            }
                            else
                            {
                                g.DrawImage(objBmp, Convert.ToInt32(obj.xPos), Convert.ToInt32(obj.yPos), objBmp.Width, objBmp.Height);
                            }

                        }
                    }// end of while
                }// end  of if 
                else
                {
                    g.DrawString("ReplayTrialNew.ObjPosDataPrev is null!!", new Font("Times New Roman", 20), new SolidBrush(Color.Black), 10, 50);
                }
            }// try block ends
            catch (System.Xml.XmlException ignore)
            {
                //  continue;
                throw;

            }

            catch (Exception ignore)
            {
                // continue;
                throw;

            }
        }

        /**
         * Routine being invoked by paintTimer interval to 
         * display trial objects along with Gaze position.
         * 
         */
        private void DrawReplayGraphics(Graphics g)
        {
            // Obtain objects information and create Bitmaps and write them to memory
            try
            {
                if (ReplayTrial.timeStampDataQueue.Count > 0)
                {
                    ObjPositionData posData = ReplayTrial.timeStampDataQueue.Dequeue();
                    ReplayTrial.ObjPosDataPrev = posData;

                    if (posData != null)
                    {
                        long timeStamp = posData.timeStamp;
                        LinkedList<ObjectData> objList = posData.getobjList();
                        IEnumerator<ObjectData> itr = objList.GetEnumerator();
                        while (itr.MoveNext())
                        {
                            ObjectData obj = (ObjectData)itr.Current;

                            string name = obj.objName;
                            if (ReplayTrial.objects.ContainsKey(name))
                            {
                                string objFilePath = (string)ReplayTrial.objects[name];

                                int xpos = Convert.ToInt32(obj.xPos);
                                int ypos = Convert.ToInt32(obj.yPos);
                                Bitmap objBmp = new Bitmap((String)objFilePath);

                                if (name == "gaze")
                                {
                                    objBmp.MakeTransparent(objBmp.GetPixel(1, 1));
                                   g.DrawImage(objBmp, Convert.ToInt32(obj.xPos), Convert.ToInt32(obj.yPos), objBmp.Width, objBmp.Height);
                                }
                                else
                                {
                                    g.DrawImage(objBmp, Convert.ToInt32(obj.xPos), Convert.ToInt32(obj.yPos), objBmp.Width, objBmp.Height);

                                    if (ReplayTrial.formSetting.isTargetObject(name) && this.markTargetObjectsFlag)
                                    {

                                        int bitmapWidth = objBmp.Width;
                                        int bitmapHeight = objBmp.Height;
                                        int objXpos = Convert.ToInt32(obj.xPos);
                                        int objYpos = Convert.ToInt32(obj.yPos);
                                        g.DrawRectangle(new Pen(this.ForeColor, (float)5.0), objXpos, objYpos, bitmapWidth + 10, bitmapHeight + 10);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }

            }
            catch (System.Xml.XmlException ignore)
            {
                //  continue;
                //  string errorStr = ignore.ToString();
                throw;

            }

            catch (Exception ignore)
            {
                // continue;
                //  string errorStr = ignore.ToString();
                throw;
            }
        }

        // Routine to handle Escape key press event
        private void KeyPress_event(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)Keys.Escape)
            {
                e.Handled = true;

                // pause the timer
                this.paintTimer.Stop();
                MessageBox.Show("Replay has been Paused");

                // create an instance of the Replay Controls Dialog Form 
                // subscribe  delegates to the Form instance

                this.dlg = new ReplayUserControlDialogFrm();
                //Subscribe this form for callback
                dlg.SetMarkTargetOptionCallback = new SetMarkTargetOptionDelegate(this.SetMarkTargetOptionCallbackFn);
                dlg.SetResumeReplayCallback = new SetResumeReplayDelegate(this.SetResumeReplayCallbackFn);
                dlg.SetStopReplayCallBack = new SetStopReplayDelegate(this.SetStopReplayCallBackFn);
                this.ParentForm.Visible = false;
                // show the dialog form
                dlg.ShowDialog();
            }
        }

        // Routine to handle Status of System target Objects either to Mark or Unmark
        private void SetMarkTargetOptionCallbackFn(int status)
        {
            if (status == 1)
                this.markTargetObjectsFlag = true;
            else
                this.markTargetObjectsFlag = false;
            // this.markTargetObjectsFlag = (status == 1) ? true : false;
        }

        private void SetResumeReplayCallbackFn(int resumeStatus)
        {
            this.dlg.Close();
            this.ParentForm.Visible = true;
            this.ParentForm.Enabled = true;
            this.paintTimer.Start();

        }

        private void SetStopReplayCallBackFn(int stopStatus)
        {
            try
            {
                this.paintTimer.Stop();
                this.paintTimer.Dispose();
                this.blinkTimer.Stop();
                this.blinkTimer.Dispose();

                initialSetup();

                this.dlg.Close();
                this.Dispose(true);

                this.ParentForm.Dispose();
                this.ParentForm.Close();
            }
            catch (Exception)
            {
                //ignore
            }
            finally
            {
                try
                {
                    this.ParentForm.Dispose();
                    this.ParentForm.Close();

                }
                catch (Exception)
                {
                    // ignore
                }

            }


        }


        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DoubleBufferReplayControl
            // 
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.DoubleBuffered = true;
            this.Name = "DoubleBufferReplayControl";
            this.Load += new System.EventHandler(this.DoubleBufferReplayControl_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPress_event);
            this.ResumeLayout(false);

        }

        // Technique of Alpha  Blending has been obtained from : 
        // www.vbdotnetheaven.com/UploadFile/mahesh/TransparentImagesShapes04212005052247AM/TransparentImagesShapes.aspx
        // I have modified the code to suit our application
        private ImageAttributes setGazeTransparency(Graphics g, Bitmap gazeBitmap, ObjectData gazePosition)
        {
            gazeBitmap.MakeTransparent(gazeBitmap.GetPixel(1, 1));
            Rectangle rect = new Rectangle(Convert.ToInt32(gazePosition.xPos), Convert.ToInt32(gazePosition.yPos), gazeBitmap.Width, gazeBitmap.Height);
            Single[][] ptsArray =  { new Single[] { 1, 0, 0, 0, 0 }, new Single[] { 0, 1, 0, 0, 0 }, new Single[] { 0, 0, 1, 0, 0 }, new Single[] { 0, 0, 0, 0.75F, 0 }, new Single[] { 0, 0, 0, 0, 1 } };
            ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
            ImageAttributes imgAttributes = new ImageAttributes();
            imgAttributes.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imgAttributes.SetGamma(0.75f);
            g.FillRectangle(Brushes.Blue, rect);
            g.FillEllipse(Brushes.Blue, rect);
            g.CompositingQuality = CompositingQuality.GammaCorrected;
            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(gazeBitmap, new Rectangle(Convert.ToInt32(gazePosition.xPos), Convert.ToInt32(gazePosition.yPos), gazeBitmap.Width, gazeBitmap.Height), 0, 0, gazeBitmap.Width, gazeBitmap.Height, GraphicsUnit.Pixel, imgAttributes);
            return imgAttributes;
        }

        private void DoubleBufferReplayControl_Load(object sender, EventArgs e)
        {

        }
    }
}


