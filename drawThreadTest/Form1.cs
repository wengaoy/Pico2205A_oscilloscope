using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Pico2205A
{
    public partial class frm2205A : Form
    {
        public frm2205A()
        {
            InitializeComponent();
        }

        public static short _handle;
        private Thread GraphThread;
        private int Ymid;
        private int YValue;
        private ChannelSettings[] _channelSettings;
        short _timebase = 8;
        short _oversample = 1;
        public const int BUFFER_SIZE =16256;//4096;// 8000;//1024;//4096;//2048;//1024;
        private int _channelCount = 2;
        PinnedArray<short>[] Pinned = new PinnedArray<short>[4];
        ushort[] inputRanges = {10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };
        public const int MAX_CHANNELS = 2;//4;
        private bool triggerEn;
        private short triggerSsource;//0 CHA, 1 CHB
        private short triggerLevelmv;
        public short trigerDirection;
        public short trigerDelay;
        public short auto_trigger_ms_delay;


        private short CHA_enabled, CHB_enabled;
        private int sampleCount;
        private int timeInterval;
        private short timeunit;
        private int timeIndisposed;
//        time_units: can be one of the following:
//PS2000_FS (0), femtoseconds,
//PS2000_PS (1), picoseconds,
//PS2000_NS (2), nanoseconds [default]
//PS2000_US (3), microseconds,
//PS2000_MS (4), milliseconds,
//PS2000_S (5), seconds

        public float CH1Freq, CH2Freq, CH1Vpp, CH2Vpp, CH1Vrms, CH2Vrms;
        public float Fs;

        //FFT
        public bool timeDomain_mode;
        public bool FFT_mode;

        public AForge.Math.Complex[] complexDataCH1a;// = new AForge.Math.Complex[4096];//126 power=15876, max sample count is 16000(Pico2205A)
        public AForge.Math.Complex[] complexDataCH2a;// = new AForge.Math.Complex[4096];//= new AForge.Math.Complex[15625];
        public static float[] CH1freqBinIndex;// = new float[4096];
        public static float[] CH2freqBinIndex;// = new float[4096];
        public static float[] CH1FFTDrawData;// = new float[4096];
        public static float[] CH2FFTDrawData;// = new float[4096];
        public static float[] CH1FFTlogDrawData;// = new float[4096];
        public static float[] CH2FFTlogDrawData;// = new float[4096];


        public static float[][] FFTdata;//store folded FreqBin index and level
        public int NP2_int, NP2, FFT_Bin_Len;
        public bool FFT_Yunit_dBu;

        public float Fs_N1; //for FFT Freq bins step
        public float F1_fundamental,V1_fund, V1_2nd, V1_3rd, V1_4th, V1_5th, V1_6th, V1_7th, V1_8th, V1_9th, V1_10th, V1_11th, V1_12th;
        public float F2_fundamental, V2_fund, V2_2nd, V2_3rd, V2_4th, V2_5th, V2_6th, V2_7th, V2_8th, V2_9th, V2_10th, V2_11th, V2_12th;
        public float THD1, THD_N1;
        public float THD2, THD_N2;

        struct ChannelSettings
        {
            public short DCcoupled;
            public Imports.Range range;
            public short enabled;
            public short[] values;
        }

        class Pwq
        {
            public Imports.PwqConditions[] conditions;
            public short nConditions;
            public Imports.ThresholdDirection direction;
            public uint lower;
            public uint upper;
            public Imports.PulseWidthType type;

            public Pwq(Imports.PwqConditions[] conditions,
                short nConditions,
                Imports.ThresholdDirection direction,
                uint lower, uint upper,
                Imports.PulseWidthType type)
            {
                this.conditions = conditions;
                this.nConditions = nConditions;
                this.direction = direction;
                this.lower = lower;
                this.upper = upper;
                this.type = type;
            }
        }

        #region PS2000 APIs

        /****************************************************************************
         * adc_to_mv
         *
         * Convert an 16-bit ADC count into millivolts
         ****************************************************************************/
        float adc_to_mv(int raw, int ch)
        {
            return (float)((raw * inputRanges[ch]) / Imports.MaxValue);//MaxValue=32767
        }


        /****************************************************************************
         * mv_to_adc
         *
         * Convert a millivolt value into a 16-bit ADC count
         *
         *  (useful for setting trigger thresholds)
         ****************************************************************************/
        short mv_to_adc(short mv, short ch)
        {
            return (short)((mv * Imports.MaxValue) / inputRanges[ch]);
        }

        //public short trigerDirection;
        //public short trigerDelay;
        //public short auto_trigger_ms_delay;
        void BlockDataHandler(string text, int offset)
        {

           //for trigger
            short result = 0;
            short trigV = mv_to_adc(triggerLevelmv, (short)_channelSettings[0].range);//convert mv to adc value

            if (triggerEn)
            {
                //result = Imports.SetTrigger(_handle, triggerSsource, trigV, 0, 0, 0);
                result = Imports.SetTrigger(_handle, triggerSsource, trigV, trigerDirection, trigerDelay, auto_trigger_ms_delay);
            }
            else 
            {
                result = Imports.SetTrigger(_handle, 5, 1000, 0, 0, 0);
            }
            //end trigger
            
            //int sampleCount = 15625;// 100;// 500;// 16000;// 8000;// BUFFER_SIZE;
            //timeunit = 2;// 0;
            //int timeIndisposed;


            //SetDefaults();
            Imports.SetChannel(_handle, (Imports.Channel)(0),
                                   _channelSettings[0].enabled = CHA_enabled,
                                   _channelSettings[0].DCcoupled,
                                   _channelSettings[0].range);
            Imports.SetChannel(_handle, (Imports.Channel)(1),//disable CH2 will increase CH1 sample rate.
                                _channelSettings[1].enabled = CHB_enabled,
                                _channelSettings[1].DCcoupled,
                                _channelSettings[1].range);
         

            for (int i = 0; i < _channelCount; i++)
            {
                short[] buffer = new short[sampleCount];
                Pinned[i] = new PinnedArray<short>(buffer);
            }

            /* find the maximum number of samples, the time interval (in timeUnits),
                * the most suitable time units, and the maximum _oversample at the current _timebase*/
           // timeInterval = 0;
            int maxSamples;
           // _oversample = 0;

          
            //_timebase = 9;// 6;//
            while ((Imports.GetTimebase(_handle, _timebase, sampleCount, out timeInterval, out timeunit, _oversample, out maxSamples)) == 0)
            {
               // Console.WriteLine("Selected timebase {0} could not be used\n", _timebase);
                _timebase++;
                if (_timebase == 100)
                {
                    _timebase = 0;
                }

            }
         //   Console.WriteLine("Timebase: {0}\toversample: {1}\tomaxSample: {2}\totimeInterval: {3}\n", _timebase, _oversample, maxSamples, timeInterval);

            /* Start it collecting, then wait for completion*/

            Imports.RunBlock(_handle, sampleCount, _timebase, _oversample, out timeIndisposed);



            short ready = 0;
            while ((ready = Imports.Isready(_handle)) == 0)// && !Console.KeyAvailable)
            {
                Thread.Sleep(100);
            }    



            if (ready > 0)
            {

                short overflow;

                Imports.GetValues(_handle, Pinned[0], Pinned[1], null, null, out overflow, sampleCount);

                sampleCount = Math.Min(sampleCount, BUFFER_SIZE);        

            }
            else
            {
                Console.WriteLine("data collection aborted");
            }
      
        }
        #endregion

        #region thread stuff
        // Draw a graph until stopped.
        private void DrawGraph1()
        {
            try
            {
                // Generate pseudo-random values.
                int y = YValue;
                for (; ; )
                {


                    BlockDataHandler("test", 10);

                    // Generate the next value.
                    // NewValue();

                    // Plot the new value.
                    PlotValue1(y, YValue);
                    y = YValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Thread] " + ex.Message);
            }
        }
        #endregion

        #region draw function

        private float P_Width, P_Height;//for pictureBox1
       // private static float P_topMargin=10, P_botMargin=10, P_leftMargin=10, P_rightMargin=10;
        private static float P_topMargin = 30, P_botMargin = 30, P_leftMargin = 30, P_rightMargin = 30;
        // Define a delegate type that takes two int parameters.
        private delegate void PlotValueDelegate(int old_y, int new_y);
        private delegate void PlotValueDelegate1(int old_y, int new_y);
        // Plot a new value.
        private void PlotValue1(int old_y, int new_y)
        {

            // See if we're on the worker thread and thus
            // need to invoke the main UI thread.
            if (this.InvokeRequired)
            {
                // Make arguments for the delegate.
                object[] args = new object[] { old_y, new_y };

                // Make the delegate.
                PlotValueDelegate plot_value_delegate1 = PlotValue1;

                // Invoke the delegate on the main UI thread.
                this.Invoke(plot_value_delegate1, args);

                // We're done.
                return;
            }
            P_Width = pictureBox1.Width;
            P_Height = pictureBox1.Height;
            Bitmap bmp;
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //if (pictureBox1.Image == null)
            //{
            //    bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //}
            //else
            //{
            //    bmp = (Bitmap)pictureBox1.Image;
            //}

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
               
                //draw gride
                // Create a new pen.
                Pen grayPen = new Pen(Brushes.LightGray);

                // Set the pen's width.
                grayPen.Width = 1.0F;
                grayPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                // Set the DashCap to round.     
                grayPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;

                //start Horizontal lines 
                for (int a = 0; a < 11; a++)//11 lines
                {
                    //grayPen.DashPattern = new float[] { 1.0F, 9.0F, 1.0F, 9.0F };//dot 1, space 9, total 10. 500W, 50dot/div.
                    grayPen.DashPattern = new float[] { 1.0F, 3.0F };
                    // g.DrawLine(grayPen, 0.0F, (48.25F * (float)a), P_Width, (48.25F * (float)a));\
                    float H_div = (P_Height - P_topMargin - P_botMargin) / 10.0F;
                    g.DrawLine(grayPen, P_leftMargin, (H_div * (float)a + P_topMargin), P_Width-P_rightMargin, (H_div * (float)a) + P_topMargin);
                }

                //then vertical lines
                for (int b = 0; b < 11; b++)//11 lines, 10 div
                {
                    //grayPen.DashPattern = new float[] { 1.0F, 8.65F, 1.0F, 8.65F };//dot 1, space 9, total 10. 500W, 50dot/div.
                    //g.DrawLine(grayPen, ((P_Width / 10) * (float)b), 0.0F, ((P_Width / 10) * (float)b), P_Height);
                   // grayPen.DashPattern = new float[] { 1.0F, 9.0F, 1.0F, 9.0F };//dot 1, space 9, total 10. 500W, 50dot/div.
                    grayPen.DashPattern = new float[] { 1.0F, 3.0F };
                    float V_div = (P_Width - P_leftMargin - P_rightMargin) / 10.0F;

                    g.DrawLine(grayPen, (V_div * (float)b + P_leftMargin), P_topMargin, (V_div * (float)b + P_leftMargin), P_Height-P_botMargin);
                }

                //draw data
                Pen pen_CH1 = new Pen(Color.Green);
                Pen pen_CH2 = new Pen(Color.Yellow);
                Pen pen_CH1_FFT = new Pen(Color.Red);
                Pen pen_CH2_FFT = new Pen(Color.Purple);

                float x1, x2, y1, y2;
                float xf1, xf2, yf1, yf2;//fft

                //float Hscale = (float)Pinned[0].Target.Length / (float)pictureBox1.Width;
                float Hscale = (float)Pinned[0].Target.Length / (float)(pictureBox1.Width - P_leftMargin - P_rightMargin);
              
                float Box_width = (float)(pictureBox1.Width - P_leftMargin - P_rightMargin);
                float Box_height = (float)(P_Height - P_topMargin - P_botMargin);
                float Yscale = (float)(P_Height - P_topMargin - P_botMargin)/ 2;

                if (timeDomain_mode)
                {
                    //CHA
                    if (CHA_enabled == 1)
                    {
                        for (int iPixel = 0; iPixel < Pinned[0].Target.Length - 1; iPixel++)
                        {

                            // float Hscale = (float)(Pinned[0].Target.Length / pictureBox1.Width);
                            //x1 = (float)iPixel / Hscale + P_leftMargin;
                            //y1 = (float)(((adc_to_mv(Pinned[0].Target[iPixel], (int)_channelSettings[0].range)) / (float)(inputRanges[(int)(_channelSettings[0].range)]) * (P_Height / 2) / 2));
                            //y1 = y1 + (float)(P_Height * (float)(Convert.ToDouble(CHA_pos.Value) / 100));
                            //x2 = (float)(iPixel + 1) / Hscale + P_leftMargin;
                            // y2 = (float)(((adc_to_mv(Pinned[0].Target[iPixel + 1], (int)_channelSettings[0].range)) / (float)(inputRanges[(int)(_channelSettings[0].range)]) * (P_Height / 2) /2));
                            //y2 = y2 + (float)(P_Height * (float)(Convert.ToDouble(CHA_pos.Value) / 100));
                            // g.DrawLine(pen_CH1, x1, ((P_Height / 2) - y1), x2, ((P_Height / 2) - y2));
                           
                            x1 = (float)iPixel / Hscale + P_leftMargin;
                            y1 = P_Height / 2 - (adc_to_mv(Pinned[0].Target[iPixel], (int)_channelSettings[0].range) / inputRanges[(int)(_channelSettings[0].range)]) * Yscale; 
                            y1 = y1 + (float)(Box_height * (float)(Convert.ToDouble(CHA_pos.Value) / 100));                         
                            x2 = (float)(iPixel + 1) / Hscale + P_leftMargin;
                            y2 = P_Height / 2 - (adc_to_mv(Pinned[0].Target[iPixel + 1], (int)_channelSettings[0].range) / inputRanges[(int)(_channelSettings[0].range)]) * Yscale;        
                            y2 = y2 + (float)(Box_height * (float)(Convert.ToDouble(CHA_pos.Value) / 100));  
                            g.DrawLine(pen_CH1, x1,  y1, x2, y2);
                            
                        }
                    }


                    //CHB
                    if (CHB_enabled == 1)
                    {
                       
                        for (int iPixel = 0; iPixel < Pinned[1].Target.Length - 1; iPixel++)
                        {
                            //x1 = (float)iPixel / Hscale + P_leftMargin;
                            //y1 = (float)(((adc_to_mv(Pinned[1].Target[iPixel], (int)_channelSettings[1].range)) / (float)(inputRanges[(int)(_channelSettings[1].range)]) * (P_Height / 2) / 2));
                            //y1 = y1 + (float)(P_Height * (float)(Convert.ToDouble(CHB_pos.Value) / 100));

                            //x2 = (float)(iPixel + 1) / Hscale + P_leftMargin;
                            //y2 = (float)(((adc_to_mv(Pinned[1].Target[iPixel + 1], (int)_channelSettings[1].range)) / (float)(inputRanges[(int)(_channelSettings[1].range)]) * (P_Height / 2) / 2));
                            //y2 = y2 + (float)(P_Height * (float)(Convert.ToDouble(CHB_pos.Value) / 100));
                            //g.DrawLine(pen_CH2, x1, ((P_Height / 2) - y1), x2, ((P_Height / 2) - y2));
                            x1 = (float)iPixel / Hscale + P_leftMargin;
                            y1 = P_Height / 2 - (adc_to_mv(Pinned[1].Target[iPixel], (int)_channelSettings[1].range) / inputRanges[(int)(_channelSettings[1].range)]) * Yscale;
                            y1 = y1 + (float)(Box_height * (float)(Convert.ToDouble(CHB_pos.Value) / 100));
                            x2 = (float)(iPixel + 1) / Hscale + P_leftMargin;
                            y2 = P_Height / 2 - (adc_to_mv(Pinned[1].Target[iPixel + 1], (int)_channelSettings[1].range) / inputRanges[(int)(_channelSettings[1].range)]) * Yscale;
                            y2 = y2 + (float)(Box_height * (float)(Convert.ToDouble(CHB_pos.Value) / 100));
                            g.DrawLine(pen_CH2, x1, y1, x2, y2);

                        }
                       
                    }
                }
               
             
                //show waveform info            
                CH1Vpp = adc_to_mv((int)Pinned[0].Target.Max(), (int)_channelSettings[0].range) - adc_to_mv((int)Pinned[0].Target.Min(), (int)_channelSettings[0].range);
                CH2Vpp = adc_to_mv((int)Pinned[1].Target.Max(), (int)_channelSettings[1].range) - adc_to_mv((int)Pinned[1].Target.Min(), (int)_channelSettings[1].range); 
                lblInfo.Text = "CH1Vp-p: " + CH1Vpp.ToString() +  " mV" + "\r\n";
                lblCH2Vpp.Text = "CH2Vp-p: " + CH2Vpp.ToString() + " mV" + "\r\n";

                CH1Vrms = (float)getChRMS2(Pinned[0].Target, (int)_channelSettings[0].range);
                lblCH1rms.Text = "CH1Vrms: " + CH1Vrms.ToString() + " mV" + "\r\n";
                CH2Vrms = (float)getChRMS2(Pinned[1].Target, (int)_channelSettings[1].range);
                lblCH2rms.Text = "CH2Vrms: " + CH2Vrms.ToString() + " mV" + "\r\n";


                txtinfo.Text = "sampleCount: " + sampleCount.ToString() + "\r\n" + 
                                "intervel: " + timeInterval.ToString() + " ns" + "\r\n" +
                                "timeIndisposed: "+ timeIndisposed.ToString();

                //Get Freq
                CH1Freq = getChFreq1(Pinned[0].Target);
                CH2Freq = getChFreq1(Pinned[1].Target);
                FreqUnit(CH1Freq, CH2Freq);

                //FFT

                System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 10);
                System.Drawing.SolidBrush drawBrush1 = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                System.Drawing.SolidBrush drawBrush2 = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow);
                if (FFT_mode)
                {
                    try {
                      processFFT1();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message+" FFT handle error!");
                    }
                  
                  
                    float FFT_Hscale = (float)(CH1freqBinIndex.Length / 2) / (float)(pictureBox1.Width - P_leftMargin - P_rightMargin);
                  
                    if (CHA_enabled ==1)
                    {
                        for (int iPixel = 0; iPixel < CH1freqBinIndex.Length / 2; iPixel++)
                        {

                            xf1 = (float)iPixel / FFT_Hscale + P_leftMargin;
                            // yf1 = (float)(((CH1FFTlogDrawData[iPixel] - 10 ) / (-120.0)) * (P_Height));
                            yf1 = (float)(((CH1FFTlogDrawData[iPixel] - 10) / (-120.0)) * (P_Height - P_topMargin - P_botMargin)) + (float)CHA_FFT_pos.Value;

                            xf2 = (float)(iPixel + 1) / FFT_Hscale + P_leftMargin;
                            //yf2 = (float)(((CH1FFTlogDrawData[iPixel + 1] - 10 ) / (-120.0)) * (P_Height));
                            yf2 = (float)(((CH1FFTlogDrawData[iPixel + 1] - 10) / (-120.0)) * (P_Height - P_topMargin - P_botMargin)) + (float)CHA_FFT_pos.Value;

                            g.DrawLine(pen_CH1_FFT, xf1, yf1, xf2, yf2);

                        }

                      
                           
                        float x =P_Width * 10 /15;
                        float y = P_Height / 10;
                        string drawStr1 = "CH1 THD: " + THD1 + "%";
                        g.DrawString(drawStr1, drawFont, drawBrush1, x, y);
                        g.DrawString("Freq_CH1: " + F1_fundamental+" Hz", drawFont, drawBrush1, x, y + 20);
                       
                    }
                    if (CHB_enabled == 1)
                    {
                        try
                        {
                            for (int iPixel = 0; iPixel < CH2freqBinIndex.Length / 2; iPixel++)
                            {

                                xf1 = (float)iPixel / FFT_Hscale + P_leftMargin;
                                // yf1 = (float)(((CH1FFTlogDrawData[iPixel] - 10 ) / (-120.0)) * (P_Height));
                                yf1 = (float)(((CH2FFTlogDrawData[iPixel] - 10) / (-120.0)) * (P_Height - P_topMargin - P_botMargin)) + (float)CHB_FFT_pos.Value;

                                xf2 = (float)(iPixel + 1) / FFT_Hscale + P_leftMargin;
                                //yf2 = (float)(((CH1FFTlogDrawData[iPixel + 1] - 10 ) / (-120.0)) * (P_Height));
                                yf2 = (float)(((CH2FFTlogDrawData[iPixel + 1] - 10) / (-120.0)) * (P_Height - P_topMargin - P_botMargin)) + (float)CHB_FFT_pos.Value;

                                g.DrawLine(pen_CH2_FFT, xf1, yf1, xf2, yf2);

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                     
                        float x = P_Width * 10 / 15;
                        float y = P_Height / 10;
                        string drawStr2 = "CH2 THD: " + THD2 + "%";
                        g.DrawString(drawStr2, drawFont, drawBrush2, x, y+40);
                        g.DrawString("Freq_CH2: " + F2_fundamental + " Hz", drawFont, drawBrush2, x, y + 60);

                    }
                   
                  
                }
               

                // the following is caculate RMS from FFT data
                //float testRms=0;
                //    for(int b=0;b<CH1FFTDrawData.Length;b++)
                //    {
                //        testRms = testRms + (float)Math.Pow(CH1FFTDrawData[b],2);
                //    }
                //    testRms = (float)Math.Sqrt(testRms);

                drawFont.Dispose();
                drawBrush1.Dispose();
                drawBrush2.Dispose();
                grayPen.Dispose();
                pen_CH1.Dispose();
                pen_CH2.Dispose();
                pen_CH1_FFT.Dispose();
                pen_CH2_FFT.Dispose();
                pictureBox1.Image = bmp;
            }
        }
        private void drawGrid()
        {
        
        }
        #endregion

        #region Initial
        //open device
        private void openOSC()
        {
            //open device
            _handle = Imports.OpenUnit();
            if (_handle <= 0)
            {
                Console.WriteLine("Unable to open device!" + "\r\n");
                Console.WriteLine("code: " + _handle.ToString() + "\r\n");
                MessageBox.Show("UNable to Open the Device!");
                return;
            }
        }

        //exiting and close device
        private void Existing()
        {
            Imports.CloseUnit(_handle);
        }
        private void initialBox()
        {
            Encoding cp437 = Encoding.GetEncoding(437);//codepage 437
            var s = cp437.GetString(new byte[] { 241 });//get +- 
            //CHA
           
            tsCHA_Vdiv.Items.Add( s + "20mV");
            tsCHA_Vdiv.Items.Add(s + "50mV");
            tsCHA_Vdiv.Items.Add(s + "100mV");
            tsCHA_Vdiv.Items.Add(s + "200mV");
            tsCHA_Vdiv.Items.Add(s + "500mV");
            tsCHA_Vdiv.Items.Add(s + "1V");
            tsCHA_Vdiv.Items.Add(s + "2V");
            tsCHA_Vdiv.Items.Add(s + "5V");
            tsCHA_Vdiv.Items.Add(s + "10V");
            tsCHA_Vdiv.Items.Add(s + "20V");
            tsCHA_Vdiv.Items.Add("Auto");
            tsCHA_Vdiv.Items.Add("OFF");

          
            //CHB
            
            tsCHB_Vdiv.Items.Add(s + "20mV");
            tsCHB_Vdiv.Items.Add(s + "50mV");
            tsCHB_Vdiv.Items.Add(s + "100mV");
            tsCHB_Vdiv.Items.Add(s + "200mV");
            tsCHB_Vdiv.Items.Add(s + "500mV");
            tsCHB_Vdiv.Items.Add(s + "1V");
            tsCHB_Vdiv.Items.Add(s + "2V");
            tsCHB_Vdiv.Items.Add(s + "5V");
            tsCHB_Vdiv.Items.Add(s + "10V");
            tsCHB_Vdiv.Items.Add(s + "20V");
            tsCHB_Vdiv.Items.Add("Auto");
            tsCHB_Vdiv.Items.Add("OFF");

           

        }


        #endregion

        #region other funtions

        //measurement
        private float getChFreq(short[] chhardwareData)
        {
            float freq = 0;

            float Vcenter = (float)(chhardwareData.Max() - chhardwareData.Min()) / 2;

            int crossP = 0;
            bool firstCross = true;
            bool skipone = false;
            int firstCrossIndex = 0;
            int lastCrossIndex = 0;

            for (int a = 0; a < chhardwareData.Length-1; a++)
            {
                if (skipone)
                {
                    if (chhardwareData[a] < Vcenter)//find fall 
                    {
                        if (chhardwareData[a] > chhardwareData[a + 1])
                        {
                            skipone = false;
                        }
                    }
                }
                else
                {
                    if (chhardwareData[a] > Vcenter) //raise
                    {
                        //ushort currentData = chhardwareData[a];
                        if (chhardwareData[a] < chhardwareData[a + 1])
                        {
                            if (firstCross)
                            {
                                firstCrossIndex = a;
                                firstCross = false;
                            }
                            crossP++;
                            a = a + 1;
                            lastCrossIndex = a;
                            skipone = true;
                        }

                    }
                }

            }

            Console.WriteLine("First cross index: " + firstCrossIndex);
            Console.WriteLine("CrossP: " + crossP);
            Console.WriteLine("Last cross index: " + lastCrossIndex);
            Console.WriteLine("timeunit: " + timeunit);
         
            try
            {               
                freq = 1 / ((float)((lastCrossIndex - firstCrossIndex) * timeInterval) / (crossP - 1));
                Console.WriteLine("freq: " + freq);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
          
            
            return freq;
        }
        private float getChFreq1(short[] chhardwareData)
        {
            float freq = 0;

            //float Vcenter = (float)(chhardwareData.Max() - chhardwareData.Min()) / 2;

            int crossP = 0;
            bool firstCross = true;
            bool skipone = false;
            int firstCrossIndex = 0;
            int lastCrossIndex = 0;

            bool square = false;
            int v1 = 0, v2 = 0;
            double TempOffset = 0;          
            //check if square
            short[] temp = new short[512];
            short tempAvg = 0;
            if (chhardwareData.Length > 512)
            {
                v1 = chhardwareData.Length / 512;

                for (int a = 0; a < 512; a++)
                {

                    for (int w = 0; w < v1; w++)
                    {
                        if (w == v1)
                        {
                            Console.WriteLine("tet");
                        }
                        tempAvg = (short)(tempAvg + (short)adc_to_mv(chhardwareData[a * v1 + w], (int)_channelSettings[0].range));
                        //  Console.WriteLine((a * v1 + w).ToString());
                    }
                    temp[a] = (short)(tempAvg / v1);
                    tempAvg = 0;

                }


            }
            else
            {
                v1 = 1;
                for (int a = 0; a < chhardwareData.Length; a++)
                { 
                    temp[a]=(short)adc_to_mv(chhardwareData[a], (int)_channelSettings[0].range);
                }
            }


            if (temp.Min() < 0)//add offset fix AC couple problem
            {
                for (int b = 0; b < temp.Length; b++)
                {
                    temp[b] = (short)(temp[b] + Math.Abs(temp.Min()));
                }
            }
            float Vcenter = (float)(temp.Max() - temp.Min()) / 2;
            for (int a = 0; a < temp.Length - 1; a++)
            {
                if (skipone)
                {
                    if (temp[a] < Vcenter)//find fall 
                    {
                        if (temp[a] > temp[a + 1])
                        {
                            skipone = false;
                        }
                    }
                }
                else
                {
                    if (temp[a] > Vcenter) //raise
                    {
                        //ushort currentData = temp[a];
                        if (temp[a] < temp[a + 1])
                        {
                            if (firstCross)
                            {
                                firstCrossIndex = a;
                                firstCross = false;
                            }
                            crossP++;
                            a = a + 1;
                            lastCrossIndex = a;
                            skipone = true;
                        }

                    }
                }

            }

            //Console.WriteLine("First cross index: " + firstCrossIndex);
            //Console.WriteLine("CrossP: " + crossP);
            //Console.WriteLine("Last cross index: " + lastCrossIndex);
            //Console.WriteLine("timeunit: " + timeunit);

            try
            {
                freq = 1 / ((float)((lastCrossIndex - firstCrossIndex) * timeInterval*v1) / (crossP - 1));
                //Console.WriteLine("freq: " + freq);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message+" Freq problem!");
            }


            return freq;
        }
        private void FreqUnit(float Ch1freq, float Ch2freq)
        {
            switch (timeunit)
            {
                case 0://fs
                    //ch1
                    Ch1freq = (float)(Ch1freq * (1.0E+15));
                    if (Ch1freq > 1000000000)
                    {
                        Ch1freq = (float)(Ch1freq / 1000000000000.00);
                        lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Mhz";
                    }
                    else if (Ch1freq > 1000000 & Ch1freq < 1000000000)
                    {
                        Ch1freq = (float)(Ch1freq / 1000000000.00);
                        lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Khz";
                    }

                    //ch2
                    Ch2freq = (float)(Ch2freq * (1.0E+15));
                    if (Ch2freq > 1000000000)
                    {
                        Ch2freq = (float)(Ch2freq / 1000000000000.00);
                        lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Mhz";
                    }
                    else if (Ch2freq > 1000000 & Ch2freq < 1000000000)
                    {
                        Ch2freq = (float)(Ch2freq / 1000000000.00);
                        lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Khz";
                    }
                    
                    break;
                case 1://ps
                    //ch1
                    Ch1freq =(float)(Ch1freq * (1.0E+12));
                    if (Ch1freq > 1000000000)
                    {
                        Ch1freq = (float)(Ch1freq / 1000000000.00);
                        lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Mhz";
                    }
                    else if (Ch1freq > 1000000 & Ch1freq < 1000000000)
                    {
                        Ch1freq = (float)(Ch1freq / 1000000.00);
                        lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Khz";
                    }

                    //ch2
                     Ch2freq =(float)(Ch2freq * (1.0E+12));
                    if (Ch2freq > 1000000000)
                    {
                        Ch2freq = (float)(Ch2freq / 1000000000.00);
                        lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Mhz";
                    }
                    else if (Ch2freq > 1000000 & Ch2freq < 1000000000)
                    {
                        Ch2freq = (float)(Ch2freq / 1000000.00);
                        lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Khz";
                    }
                    break;
                case 2://ns
                        //ch1
                    Ch1freq = (float)(Ch1freq *(1.0E+9));
                        if (Ch1freq > 1000000000)
                        {
                            Ch1freq = (float)(Ch1freq / 1000000000.00);
                            lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Mhz";
                        }
                        else if (Ch1freq > 1000 & Ch1freq < 1000000000)
                        {
                            Ch1freq = (float)(Ch1freq / 1000.00);
                            lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Khz";
                        }
                        else if (Ch1freq > 1 & Ch1freq < 1000)
                        {
                            Ch1freq = (float)(Ch1freq);
                            lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Hz";
                        }

                    //ch2
                      Ch2freq = (float)(Ch2freq *(1.0E+9));
                        if (Ch2freq > 1000000000)
                        {
                            Ch2freq = (float)(Ch2freq / 1000000000.00);
                            lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Mhz";
                        }
                        else if (Ch2freq > 1000 & Ch2freq < 1000000000)
                        {
                            Ch2freq = (float)(Ch2freq / 1000.00);
                            lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Khz";
                        }
                        else if (Ch2freq > 1 & Ch2freq < 1000)
                        {
                            Ch2freq = (float)(Ch2freq);
                            lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Hz";
                        }
                    break;
                case 3://us
                    //ch1
                    Ch1freq = (float)(Ch1freq * (1.0E+6));
                    if (Ch1freq > 0 & Ch1freq < 1000)
                    {
                        Ch1freq = (float)(Ch1freq);
                        lblCH1freq.Text = "CH1 Freq: " + Ch1freq.ToString() + "Hz";
                    }

                    //ch2
                     Ch2freq = (float)(Ch2freq * (1.0E+6));
                    if (Ch2freq > 0 & Ch2freq < 1000)
                    {
                        Ch2freq = (float)(Ch2freq);
                        lblCH2freq.Text = "CH2 Freq: " + Ch2freq.ToString() + "Hz";
                    }

                    break;
                case 4:

                    break;
                case 5:

                    break;
            }
        }
        private double getChRMS2(short[] mychhardwareData, int ch_range)
        {
            double Vrms = 0;
            double offset1 = 0;

            double temp1 = 0;
            for (int n = 0; n < mychhardwareData.Length; n++)
            {
                offset1 = Math.Pow(adc_to_mv(mychhardwareData[n], ch_range), 2);
                temp1 = temp1 + offset1;
            }
            Vrms = Math.Sqrt(temp1 / mychhardwareData.Length);

            return Vrms;
        }
    

        private void processFFT1()
        {
            //The method accepts data array of 2 power n size only, where n may vary in the [1, 14] range.

            //Fs = (float)(1000 / getRealSample());//real ADC sample rate
            Fs = (float)(1/(timeInterval*1E-9));//real ADC sample rate

            NP2 = Pinned[0].Target.Length;
            NP2_int = 0;
            for (int n = 14; n >1; n--)
            {
               NP2_int=NP2>>n;
               if (NP2_int == 1)
               {
                   FFT_Bin_Len = (int)Math.Pow(2, n);
                   FFT_Bin_Len = 1 << n;
                   break;
               }
            }

            complexDataCH1a = new AForge.Math.Complex[FFT_Bin_Len];
            complexDataCH2a = new AForge.Math.Complex[FFT_Bin_Len];
            CH1freqBinIndex = new float[FFT_Bin_Len];
            CH2freqBinIndex = new float[FFT_Bin_Len];
            CH1FFTDrawData = new float[FFT_Bin_Len];
            CH2FFTDrawData = new float[FFT_Bin_Len];
            CH1FFTlogDrawData = new float[FFT_Bin_Len];
            CH2FFTlogDrawData = new float[FFT_Bin_Len];   

            for (int i = 0; i < FFT_Bin_Len; ++i)
            {
                complexDataCH1a[i].Re = adc_to_mv(Pinned[0].Target[i], (int)_channelSettings[0].range); // Add your real part here
                complexDataCH1a[i].Im = 0;// ; // Add your imaginary part here
                complexDataCH2a[i].Re = adc_to_mv(Pinned[1].Target[i], (int)_channelSettings[1].range); // Add your real part here
                complexDataCH2a[i].Im = 0;// ; // Add your imaginary part here
            }
            AForge.Math.FourierTransform.FFT(complexDataCH1a, AForge.Math.FourierTransform.Direction.Forward);
            AForge.Math.FourierTransform.FFT(complexDataCH2a, AForge.Math.FourierTransform.Direction.Forward);

            Fs_N1 = (float)(Fs / FFT_Bin_Len);


            for (int j = 0; j < FFT_Bin_Len; j++)
            {
                CH1freqBinIndex[j] = (float)j * Fs_N1; //for freq info
                CH1FFTDrawData[j] = (float)(complexDataCH1a[j].Magnitude);

                CH2freqBinIndex[j] = (float)j * Fs_N1; //for freq info
                CH2FFTDrawData[j] = (float)(complexDataCH2a[j].Magnitude);
            }

          //convert to dbv
            for (int c = 0; c < FFT_Bin_Len; c++)
            {
                if (CH1FFTDrawData[c]!=0)//log can not be 0!!!
                {
                    CH1FFTlogDrawData[c] = (float)(20 * Math.Log10(CH1FFTDrawData[c] / 775));// *P_Height;
                }
                if (CH2FFTDrawData[c] != 0)//log can not be 0!!!
                {
                    CH2FFTlogDrawData[c] = (float)(20 * Math.Log10(CH2FFTDrawData[c] / 775));// *P_Height;
                }
            }
         

            //float FFTmin = CH1FFTlogDrawData.Min();
            if (CHA_enabled==1)
            {
                //find max value except index 0 which is DC value.
                int index1 = 0;// Array.IndexOf(CH1FFTlogDrawData, CH1FFTlogDrawData.Max());
                float max1 =-1000;
                for (int i = 0; i < 8192 / (1 + CHB_enabled); i++)
                {
                    if (max1 < CH1FFTDrawData[i])
                    {
                        max1 = CH1FFTDrawData[i];
                        index1 = i;
                    }
                }

                if (index1 * 12 < 8192/(1+CHB_enabled))
                {
                    F1_fundamental = CH1freqBinIndex[index1];
                    V1_fund = CH1FFTDrawData[index1];
                    V1_2nd = CH1FFTDrawData[index1 * 2];
                    V1_3rd = CH1FFTDrawData[index1 * 3];
                    V1_4th = CH1FFTDrawData[index1 * 4];
                    V1_5th = CH1FFTDrawData[index1 * 5];
                    V1_6th = CH1FFTDrawData[index1 * 6];
                    V1_7th = CH1FFTDrawData[index1 * 7];
                    V1_8th = CH1FFTDrawData[index1 * 8];
                    V1_9th = CH1FFTDrawData[index1 * 9];
                    V1_10th = CH1FFTDrawData[index1 * 10];
                    V1_11th = CH1FFTDrawData[index1 * 11];
                    V1_12th = CH1FFTDrawData[index1 * 12];
                }
              

                float SumHn1 = (float)(Math.Pow(V1_2nd, 2) + Math.Pow(V1_3rd, 2) + Math.Pow(V1_4th, 2) + Math.Pow(V1_5th, 2) + Math.Pow(V1_6th, 2)
                    + Math.Pow(V1_7th, 2) + Math.Pow(V1_8th, 2) + Math.Pow(V1_9th, 2) + Math.Pow(V1_10th, 2) + Math.Pow(V1_11th, 2) + Math.Pow(V1_12th, 2));
                SumHn1 = (float)Math.Sqrt(SumHn1);
                THD1 = (float)(SumHn1 / V1_fund) * 100;
            }

            if (CHB_enabled == 1)
            {   
                //find max value except index 0 which is DC value.
                int index2 = 0;// Array.IndexOf(CH1FFTlogDrawData, CH1FFTlogDrawData.Max());
                float max2 = -1000;
                for (int i = 0; i < 8192 / (1 + CHB_enabled); i++)
                {
                    if (max2 < CH2FFTDrawData[i])
                    {
                        max2 = CH2FFTDrawData[i];
                        index2 = i;
                    }
                }
                
                if (index2 * 12 < 8192/(1 + CHB_enabled))
                {
                    F2_fundamental = CH2freqBinIndex[index2];
                    V2_fund = CH2FFTDrawData[index2];
                    V2_2nd = CH2FFTDrawData[index2 * 2];
                    V2_3rd = CH2FFTDrawData[index2 * 3];
                    V2_4th = CH2FFTDrawData[index2 * 4];
                    V2_5th = CH2FFTDrawData[index2 * 5];
                    V2_6th = CH2FFTDrawData[index2 * 6];
                    V2_7th = CH2FFTDrawData[index2 * 7];
                    V2_8th = CH2FFTDrawData[index2 * 8];
                    V2_9th = CH2FFTDrawData[index2 * 9];
                    V2_10th = CH2FFTDrawData[index2 * 10];
                    V2_11th = CH2FFTDrawData[index2 * 11];
                    V2_12th = CH2FFTDrawData[index2 * 12];
                }

                float SumHn2 = (float)(Math.Pow(V2_2nd, 2) + Math.Pow(V2_3rd, 2) + Math.Pow(V2_4th, 2) + Math.Pow(V2_5th, 2) + Math.Pow(V2_6th, 2)
                    + Math.Pow(V2_7th, 2) + Math.Pow(V2_8th, 2) + Math.Pow(V2_9th, 2) + Math.Pow(V2_10th, 2) + Math.Pow(V2_11th, 2) + Math.Pow(V2_12th, 2));
                SumHn2 = (float)Math.Sqrt(SumHn2);
                THD2 = (float)(SumHn2 / V2_fund) * 100;
            }



            //// the following is caculate RMS from FFT data
            //float testRms = 0;
            //for (int b = 0; b < CH1FFTDrawData.Length; b++)
            //{
            //    testRms = testRms + (float)Math.Pow(CH1FFTDrawData[b], 2);
            //}
            //testRms = (float)Math.Sqrt(testRms);

          //  Console.WriteLine("THD1: " + THD1+" " +" THD2: " +THD2);
        }


        private void setTimeBase()
        {
            switch (tsCbo_timebase.SelectedIndex)
            {
                case 0://50ns, only when CHB disabled
                    if (_channelSettings[1].enabled == 0)
                    {
                        _timebase = 0;
                        sampleCount = 100;
                    }
                    else
                    {
                        _timebase = 0;
                        sampleCount = 200 / (1 + _channelSettings[1].enabled);
                    }

                    break;
                case 1://100ns
                    _timebase = 0;
                    sampleCount = 200 / (1 + _channelSettings[1].enabled);
                    break;
                case 2://200ns
                    _timebase = 0;
                    sampleCount = 400 / (1 + _channelSettings[1].enabled);
                    break;
                case 3://500ns
                    _timebase = 0;
                    sampleCount = 1000 / (1 + _channelSettings[1].enabled);
                    break;
                case 4://1us
                    _timebase = 0;
                    sampleCount = 2000 / (1 + _channelSettings[1].enabled);
                    break;
                case 5://2us
                    _timebase = 0;
                    sampleCount = 4000 / (1 + _channelSettings[1].enabled);
                    break;
                case 6://5us
                    _timebase = 0;
                    sampleCount = 10000 / (1 + _channelSettings[1].enabled);
                    break;
                case 7://10us
                    _timebase = (short)(1 + _channelSettings[1].enabled);
                    sampleCount = 10000 / (1 + _channelSettings[1].enabled);
                    break;
                case 8://20us
                    _timebase = (short)(2 + _channelSettings[1].enabled);
                    sampleCount = 10000 / (1 + _channelSettings[1].enabled);
                    break;
                case 9://50us
                    _timebase = (short)(3 + _channelSettings[1].enabled);
                    
                    sampleCount = 10000 / (1 + _channelSettings[1].enabled);
                    break;
                case 10://100us
                    _timebase = (short)(4 + _channelSettings[1].enabled);
                    sampleCount = 12500 / (1 + _channelSettings[1].enabled);
                    break;
                case 11://200us
                    //_timebase = 2;
                    //sampleCount = 16252;
                    _timebase = (short)(5 + _channelSettings[1].enabled);
                    sampleCount = 12500 / (1 + _channelSettings[1].enabled);
                    break;
                case 12://500us
                    _timebase = (short)(6 + _channelSettings[1].enabled);
                    sampleCount = 15625 / (1 + _channelSettings[1].enabled);
                    break;
                case 13://1ms
                    _timebase = (short)(7 + _channelSettings[1].enabled);
                    sampleCount = 15625 / (1 + _channelSettings[1].enabled);
                    break;
                case 14://2ms
                    _timebase = (short)(8 + _channelSettings[1].enabled);
                    sampleCount = 15625 / (1 + _channelSettings[1].enabled);
                    break;
                case 15://5ms
                    _timebase = (short)(10 + _channelSettings[1].enabled);;//15; from MFG software 2ms to 5ms seems skip timebase 9, don't know why yet
                    sampleCount = 9766 / (1 + _channelSettings[1].enabled);
                    break;
                case 16://10ms
                    _timebase = (short)(11 + _channelSettings[1].enabled);
                    sampleCount = 9766 / (1 + _channelSettings[1].enabled);
                    break;
                case 17://20ms
                    _timebase = (short)(12 + _channelSettings[1].enabled);
                    sampleCount = 9766 / (1 + _channelSettings[1].enabled);

                    break;
                case 18://50ms
                    _timebase = (short)(13 + _channelSettings[1].enabled);
                    sampleCount = 12207 / (1 + _channelSettings[1].enabled);
                    break;
                case 19://100ms
                    _timebase = (short)(14 + _channelSettings[1].enabled);
                    sampleCount = 12207 / (1 + _channelSettings[1].enabled);
                    break;
                case 20://200ms, from here MFG software seems to change captrue mode, sample interval is 125us, scan mode???
                    _timebase = (short)(15 + _channelSettings[1].enabled);
                    sampleCount = 12207 / (1 + _channelSettings[1].enabled);//16000
                    break;
                case 21://500ms
                    _timebase = (short)(16 + _channelSettings[1].enabled);
                    sampleCount = 15974 / (1 + _channelSettings[1].enabled);
                    break;
                case 22://1s
                    _timebase = (short)(17+ _channelSettings[1].enabled);
                    sampleCount = 16000 / (1 + _channelSettings[1].enabled);
                    break;
                case 23://2s
                    _timebase = (short)(18 + _channelSettings[1].enabled);
                    sampleCount = 16000 / (1 + _channelSettings[1].enabled);
                    break;
                case 24://5s
                    _timebase = (short)(19 + _channelSettings[1].enabled);
                    sampleCount = 16000 / (1 + _channelSettings[1].enabled);
                    break;
            }
        }
        private void setFFT_TimeBase()
        {
            switch (tsCbo_timebase.SelectedIndex)
            {
                case 0://25MHz
                    _timebase = 2;
                    sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 1://12.5MHz
                    _timebase = 3;
                    sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 2://6.25 MHz
                     _timebase = 4;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 3://3.125 MHz
                     _timebase = 5;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 4://1.563 MHz
                     _timebase = 6;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 5:// MHz
                    _timebase = 7;
                    sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 6:// MHz
                    _timebase = 8;
                    sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 7:// MHz
                     _timebase = 9;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 8:// MHz
                     _timebase = 10;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 9:// MHz
                     _timebase = 11;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 10:// MHz
                     _timebase = 12;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 11:// MHz
                     _timebase = 13;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 12:// MHz
                     _timebase = 14;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 13:// MHz
                    _timebase = 15;
                    sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 14:// MHz
                     _timebase = 16;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 15:// MHz
                     _timebase = 17;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 16:// MHz
                     _timebase = 18;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
                case 17:// MHz
                     _timebase = 19;
                     sampleCount = 16252 - 8192 * (_channelSettings[1].enabled);
                    break;
               
            }
        }
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            
            
            timeDomain_mode = true;
            FFT_mode = false;

            initialBox();
            //probe
            tsbtnProbe.SelectedIndex = 1;//x10
          

           //trigger
            auto_trigger_ms_delay = 1000;   //no triger after 1000ms then get data
            trigerDirection = 0;            //rising
            trigerDelay = 0;//see PS2000Imports
            tsCbTriger.SelectedIndex = 0;//no trigger
            //triggerSsource = 5;//disable trigger
            tsCbo_Trigger_CHn.SelectedIndex = 0;//CHA
            //open device
            openOSC();
         
            

            Thread.Sleep(1000);

            _channelSettings = new ChannelSettings[MAX_CHANNELS];
            _channelSettings[0].range = Imports.Range.Range_1V;//.Range_5V;
            _channelSettings[1].range = Imports.Range.Range_1V;//.Range_5V;

            _channelSettings[0].enabled = 1;
            _channelSettings[1].enabled = 0;

            //AC/DC Couple
            _channelSettings[0].DCcoupled = 1;
            _channelSettings[1].DCcoupled = 1;
            tsCHA_ACDC.SelectedIndex = 1;
            tsCHB_ACDC.SelectedIndex = 1;

            tsCHA_Vdiv.SelectedIndex = 5;//1V
            tsCHB_Vdiv.SelectedIndex = 11;//OFF

            _timebase = 7;//1 ms/div
            sampleCount = 15625 / (1 + _channelSettings[1].enabled); ;
           

            if (GraphThread == null & _handle == 1) //Thread not start yet and Device was opened
            {
                Console.WriteLine("Starting capture thread...");


                GraphThread = new Thread(DrawGraph1);
                GraphThread.Priority = ThreadPriority.BelowNormal;
                GraphThread.IsBackground = true;
                GraphThread.Start();
                tslblcaptureStatus.Text = "Started";
                tsbtnStart_Stop.Image = Properties.Resources.stop48;

            }
            else
            {
                Console.WriteLine("Stop capture thread...");
                tslblcaptureStatus.Text = "Stoped";
                tsbtnStart_Stop.Image = Properties.Resources.play48;
            }

          
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            PlotValue1(0, 0);
        }

        private void splitContainer3_SizeChanged(object sender, EventArgs e)
        {
            PlotValue1(0, 0);
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
           PlotValue1(0, 0);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Existing();
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Existing();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tsbtnStop_Click(object sender, EventArgs e)
        {
         
        }

        private void tsbtnStart_Click(object sender, EventArgs e)
        {
            if (GraphThread == null & _handle == 1) //Thread not start yet and Device was opened
            {
                Console.WriteLine("Starting capture thread...");


                GraphThread = new Thread(DrawGraph1);
                GraphThread.Priority = ThreadPriority.BelowNormal;
                GraphThread.IsBackground = true;
                GraphThread.Start();
                tslblcaptureStatus.Text = "Started";
                tsbtnStart_Stop.Image = Properties.Resources.stop48;

            }
            else
            {
                Console.WriteLine("Stop capture thread...");
                GraphThread.Abort();
                GraphThread = null;
                tslblcaptureStatus.Text = "Stoped";
                tsbtnStart_Stop.Image = Properties.Resources.play48;
            }
        }

        private void tsCHA_Vdiv_Click(object sender, EventArgs e)
        {

        }

        private void tsCHA_Vdiv_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tsCHA_Vdiv.SelectedIndex)
            {
                case 0:
                    _channelSettings[0].range = Imports.Range.Range_20MV;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 1:
                    _channelSettings[0].range = Imports.Range.Range_50MV;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 2:
                    _channelSettings[0].range = Imports.Range.Range_100MV;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 3:
                    _channelSettings[0].range = Imports.Range.Range_200MV;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 4:
                    _channelSettings[0].range = Imports.Range.Range_500MV;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 5:
                    _channelSettings[0].range = Imports.Range.Range_1V;
                    _channelSettings[0].enabled = 1;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 6:
                    _channelSettings[0].range = Imports.Range.Range_2V;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 7:
                    _channelSettings[0].range = Imports.Range.Range_5V;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 8:
                    _channelSettings[0].range = Imports.Range.Range_10V;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 9:       
                    _channelSettings[0].range = Imports.Range.Range_20V;
                    _channelSettings[0].enabled = 1;
                    CHA_enabled = 1;
                    break;
                case 10:
                    CHA_enabled = 1;
                    _channelSettings[0].enabled = 1;
                    break;

                case 11:
                    _channelSettings[0].enabled = 0;
                    CHA_enabled = 0;
                    tsCbTriger.SelectedIndex = 0;//disable triggering, if not do this, it will cause not capture when enable again
                    break;
            }

            //need set timebase again
            if (timeDomain_mode)
            {

                setTimeBase();
            }
            else if (FFT_mode)
            {

                setFFT_TimeBase();
            }
            // _channelSettings[0].range =(int)cboRange.SelectedIndex;
        }

      
        private void tsCbo_timebase_Click(object sender, EventArgs e)
        {

        }

        private void tsCbo_timebase_SelectedIndexChanged(object sender, EventArgs e)
        {
           // setTimeBase();
            if (timeDomain_mode)
            {
                
                setTimeBase();
            }
            else if(FFT_mode)
            {
               
                setFFT_TimeBase();
            }
           
         
        }

        private void tsCbo_timebase_Click_1(object sender, EventArgs e)
        {

        }

        private void tsCbTriger_Click(object sender, EventArgs e)
        {

        }

        private void tsCbTriger_SelectedIndexChanged(object sender, EventArgs e)
        {
            //None
            //Auto
            //Repeat
            //Single
            //ETS
            switch (tsCbTriger.SelectedIndex)
            { 
                case 0:
                    triggerEn = false;
                    //triggerSsource = 5;//for disable trigger
                    break;
                case 1:
                    triggerEn = true;
                    break;
                case 2:
                    triggerEn = true;
                    break;
                case 3:
                    triggerEn = true;
                    break;
                case 4:
                    triggerEn = true;
                    break;
            }
        }

        private void tsCbo_Trigger_CHn_SelectedIndexChanged(object sender, EventArgs e)
        {
            //tsCbo_Trigger_CHn
            switch(tsCbo_Trigger_CHn.SelectedIndex)
            {
                case 0:
                    triggerSsource = 0;//CHA
                    break;
                case 1:
                    triggerSsource = 1;//CHB
                    break;
                case 2:
                     triggerSsource = 5;//disable
                    break;
            }
        }

        private void tsCbo_Trigger_CHn_Click(object sender, EventArgs e)
        {

        }

        private void txtTriggerVoltage_TextChanged(object sender, EventArgs e)
        {
            int n;
            bool isNumeric = int.TryParse(txtTriggerVoltage.Text, out n);
            if (isNumeric)
            {
                try
                {
                    triggerLevelmv = Convert.ToInt16(txtTriggerVoltage.Text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
           txtinfo.AppendText(triggerLevelmv.ToString()+"\r\n");
        }

        private void txtTriggerVoltage_Click(object sender, EventArgs e)
        {

        }

        private void tsCHB_Vdiv_Click(object sender, EventArgs e)
        {

        }

        private void tsCHB_Vdiv_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            switch (tsCHB_Vdiv.SelectedIndex)
            {
                case 0:
                    _channelSettings[1].range = Imports.Range.Range_20MV;
                  _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 1:
                    _channelSettings[1].range = Imports.Range.Range_50MV;
                   _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 2:
                    _channelSettings[1].range = Imports.Range.Range_100MV; 
                   _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 3:
                    _channelSettings[1].range = Imports.Range.Range_200MV;
                    _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 4:
                    _channelSettings[1].range = Imports.Range.Range_500MV; 
                  _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 5:
                    _channelSettings[1].range = Imports.Range.Range_1V; 
                    _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 6:
                    _channelSettings[1].range = Imports.Range.Range_2V; 
                    _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 7:
                    _channelSettings[1].range = Imports.Range.Range_5V; 
                    _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 8:
                    _channelSettings[1].range = Imports.Range.Range_10V; 
                    _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 9:
                    _channelSettings[1].range = Imports.Range.Range_20V;
                    _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;
                case 10:
                     _channelSettings[1].enabled = 1;
                    CHB_enabled = 1;
                    break;

                case 11:           
                    _channelSettings[1].enabled = 0;
                    CHB_enabled = 0;
                    tsCbTriger.SelectedIndex = 0;//disable triggering, if not do this, it will cause not capture when enable again
                    break;
            }
            //need set timebase again
            if (timeDomain_mode)
            {

                setTimeBase();
            }
            else if (FFT_mode)
            {

                setFFT_TimeBase();
            }
        }

        private void tsCHA_ACDC_SelectedIndexChanged(object sender, EventArgs e)
        {

            switch (tsCHA_ACDC.SelectedIndex)
            { 
                case 0:
                     _channelSettings[0].DCcoupled = 0;//AC coupled
                    break;
                case 1:
                     _channelSettings[0].DCcoupled = 1;//DC coupled
                    break;
            }

        }

        private void tsCHB_ACDC_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tsCHB_ACDC.SelectedIndex)
            {
                case 0:
                    _channelSettings[1].DCcoupled = 0;//AC coupled
                    break;
                case 1:
                    _channelSettings[1].DCcoupled = 1;//DC coupled
                    break;
            }
        }

        private void CHA_pos_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tsFFT_Click(object sender, EventArgs e)
        {
            timeDomain_mode = false;
            FFT_mode = true;
            
            tsCbo_timebase.Items.Clear();
            tsCbo_timebase.Items.Add("25 MHz");
            tsCbo_timebase.Items.Add("12.5 MHz");
            tsCbo_timebase.Items.Add("6.25 MHz");
            tsCbo_timebase.Items.Add("3.125 MHz");
            tsCbo_timebase.Items.Add("1.563 MHz");
            tsCbo_timebase.Items.Add("781.3 KHz");
            tsCbo_timebase.Items.Add("390.6 KHz");
            tsCbo_timebase.Items.Add("195.3 KHz");
            tsCbo_timebase.Items.Add("97.66 KHz");
            tsCbo_timebase.Items.Add("48.83 KHz");
            tsCbo_timebase.Items.Add("24.41 KHz");
            tsCbo_timebase.Items.Add("12.21 KHz");
            tsCbo_timebase.Items.Add("6.104 KHz");
            tsCbo_timebase.Items.Add("3.052 KHz");
            tsCbo_timebase.Items.Add("762.9 Hz");
            tsCbo_timebase.Items.Add("381.5 Hz");
            tsCbo_timebase.Items.Add("190.7 Hz");
            tsCbo_timebase.SelectedIndex = 6;
        }

        private void tsTimeDomain_Click(object sender, EventArgs e)
        {
            timeDomain_mode = true;
            FFT_mode = false;

            tsCbo_timebase.Items.Clear();
            tsCbo_timebase.Items.Add("50 ns/div");
            tsCbo_timebase.Items.Add("100 ns/div");
            tsCbo_timebase.Items.Add("200 ns/div");
            tsCbo_timebase.Items.Add("500 ns/div");
            tsCbo_timebase.Items.Add("1 us/div");
            tsCbo_timebase.Items.Add("2 us/div");
            tsCbo_timebase.Items.Add("5 us/div");
            tsCbo_timebase.Items.Add("10 us/div");
            tsCbo_timebase.Items.Add("20 us/div");
            tsCbo_timebase.Items.Add("50 us/div");
            tsCbo_timebase.Items.Add("100 us/div");
            tsCbo_timebase.Items.Add("200 us/div");
            tsCbo_timebase.Items.Add("500 us/div");
            tsCbo_timebase.Items.Add("1 ms/div");
            tsCbo_timebase.Items.Add("2 ms/div");
            tsCbo_timebase.Items.Add("5 ms/div");
            tsCbo_timebase.Items.Add("10 ms/div");
            tsCbo_timebase.Items.Add("20 ms/div");
            tsCbo_timebase.Items.Add("50 ms/div");
            tsCbo_timebase.Items.Add("100 ms/div");
            tsCbo_timebase.Items.Add("200 ms/div");
            tsCbo_timebase.Items.Add("500 ms/div");
            tsCbo_timebase.Items.Add("1 s/div");
            tsCbo_timebase.Items.Add("2 s/div");
            tsCbo_timebase.Items.Add("5 s/div");

            tsCbo_timebase.SelectedIndex = 13;

        }

        private void tsCHB_Vdiv_RightToLeftChanged(object sender, EventArgs e)
        {

        }
        public static bool sgWindowOn;
        private void tsbtnSignalG_Click(object sender, EventArgs e)
        {
            if (!sgWindowOn)
            {
                SignalGenerator_builtIn sg = new SignalGenerator_builtIn();
                Point p = new Point(0, 0);

                p.X = Cursor.Position.X;
                p.Y = Cursor.Position.Y;

                sg.Location = p;
                sg.Show();
                sgWindowOn = true;
            }
            else
            {
                MessageBox.Show("Signal Generator Window is already ON!");
            }

            //Thread.Sleep(1000);//important!!!, otherwise won't work.
            //short code = Imports.ps2000_set_sig_gen_built_in(   _handle,
            //                                                    0,
            //                                                    1000000,
            //                                                    Imports.WaveType.SQUARE,
            //                                                    1000,
            //                                                    1000,
            //                                                    0,
            //                                                    0,
            //                                                    Imports.SweepType.UPDOWN,
            //                                                    0);           
            //Console.WriteLine("code: " + code.ToString() + "\r\n");

            
         
        }

        private void btnRisingEdge_Click(object sender, EventArgs e)
        {
            trigerDirection = 0;
            
        }

        private void btnFallingEdge_Click(object sender, EventArgs e)
        {
            trigerDirection = 1;
        }
    }
}
