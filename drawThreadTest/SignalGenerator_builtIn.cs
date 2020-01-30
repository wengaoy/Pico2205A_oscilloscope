using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Pico2205A
{
    public partial class SignalGenerator_builtIn : Form
    {
        public SignalGenerator_builtIn()
        {
            InitializeComponent();
        }
                          
        public int offsetV;
        public uint pk2pk;   
        Imports.WaveType waveT;
        public float startFreq;
        public float stopFreq;
        public float IncFreq;
        public float dwelltime;
        Imports.SweepType sweepT;
        public uint sweeps;

        public bool sgeneratorON;
        public bool sweepModeON;

        #region Functions
        private void setSGvalue()
        { 
            
        }
        public short setSingnalGen()
        {
            short rcode = 0;
            Thread.Sleep(1000);
            if (!sweepModeON)
            {
                stopFreq = startFreq;
                IncFreq = 0;//
                sweeps = 0;
            }
            rcode = Imports.ps2000_set_sig_gen_built_in(frm2205A._handle, offsetV, pk2pk, waveT, startFreq, stopFreq, IncFreq, dwelltime, sweepT, sweeps);
            return rcode;
        }

        public short SingnalGenOFF()
        {
            short rcode = 0;
            Thread.Sleep(1000);
            rcode = Imports.ps2000_set_sig_gen_built_in(frm2205A._handle, offsetV, 0, waveT, 0, 0, 0, 0, 0, 0);
            return rcode;
        }
        #endregion


        private void SignalGenerator_builtIn_FormClosing(object sender, FormClosingEventArgs e)
        {
            frm2205A.sgWindowOn = false;
            SingnalGenOFF();
        }


      

        private void SignalGenerator_builtIn_Load(object sender, EventArgs e)
        {
            sgeneratorON = false;
            sweepModeON = false;
            cboWaveType.SelectedIndex = 1;
            cboSweepT.SelectedIndex = 2;//UPDOWN

            offsetV = (int)numUD_offset.Value;
            pk2pk = (uint)numUD_pk2pk.Value*2;
            waveT = Imports.WaveType.SINE;
            startFreq = (float)numUD_StartFreq.Value;
            stopFreq = (float)numUD_StopFreq.Value;
            IncFreq = (float)numUD_IncFreq.Value;
            dwelltime =(float)numUD_time_ms_forIncFreq.Value/1000;
            sweepT = Imports.SweepType.UPDOWN;
            sweeps = 1000000;

            if (!sweepModeON)
            {
                stopFreq = startFreq;
                IncFreq = 0;//
                sweeps = 0;
            }

            //Thread.Sleep(1000);//important!!!, otherwise won't work.
            //short code = Imports.ps2000_set_sig_gen_built_in(frm2205A._handle,
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

            //Thread.Sleep(1000);
            //cboWaveType.SelectedIndex = 1;//sine default
            //offsetV = 0;
            //pk2pk = 2000000;
            //waveT = Imports.WaveType.SINE;
            //startFreq = 1000;
            //stopFreq = 1000;
            //FreqInc = 0;
            //dwelltime = 0;
            //sweepT = Imports.SweepType.UPDOWN;
            //sweeps = 0;
            //short rcode = Imports.ps2000_set_sig_gen_built_in(frm2205A._handle, offsetV, pk2pk, waveT, startFreq, stopFreq, FreqInc, dwelltime, sweepT, sweeps);
            //Console.WriteLine("return code: " + rcode);
        }

        private void cboWaveType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboWaveType.SelectedIndex)
            { 
                case 0:
                   //for Arbitrary
                    break;
                case 1:
                    waveT = Imports.WaveType.SINE;
                    break;
                case 2:
                    waveT = Imports.WaveType.SQUARE;
                    break;
                case 3:
                    waveT = Imports.WaveType.TRIANGLE;
                    break;
                case 4:
                    waveT = Imports.WaveType.RAMP_UP;
                    break;
                case 5:
                    waveT = Imports.WaveType.RAMP_DOWN;
                    break;
                case 6:
                    waveT = Imports.WaveType.DC_VOLTAGE;
                    //waveT = Imports.WaveType.SINC;
                    break;
                case 7:
                    waveT = Imports.WaveType.GAUSSIAN;
                    break;
                case 8:
                    waveT = Imports.WaveType.SINC;
                    //waveT = Imports.WaveType.HALF_SINE;
                    break;
                case 9:
                    waveT = Imports.WaveType.HALF_SINE;
                   // waveT = Imports.WaveType.DC_VOLTAGE;
                    break;
            }
            short rcode = 0;
            if (sgeneratorON)
            {
                rcode = setSingnalGen();
            }
        }

        private void chkSG_ON_CheckedChanged(object sender, EventArgs e)
        {
            short rcode = 0;
            if (chkSG_ON.Checked)
            {
                sgeneratorON = true;
                rcode = setSingnalGen();
            }
            else
            {
                sgeneratorON = false;
                rcode = SingnalGenOFF();
            }

            Console.WriteLine("rcode: " + rcode);
        }

        private void numUD_freq_ValueChanged(object sender, EventArgs e)
        {              
            startFreq = (float)numUD_StartFreq.Value;
            if (!sweepModeON)
            {
                stopFreq = startFreq;
            }
            if (sgeneratorON)
            {
                setSingnalGen();
            }
        }

        private void numUD_pk2pk_ValueChanged(object sender, EventArgs e)
        {
            pk2pk = (uint)numUD_pk2pk.Value*2;
            if (sgeneratorON)
            {
                setSingnalGen();
            }
        }

        private void numUD_offset_ValueChanged(object sender, EventArgs e)
        {
            offsetV = (int)numUD_offset.Value;
            if (sgeneratorON)
            {
                setSingnalGen();
            }
        }

        private void numUD_pk2pk_Enter(object sender, EventArgs e)
        {
         
        }

        private void numUD_IncFreq_ValueChanged(object sender, EventArgs e)
        {
            IncFreq = (float)numUD_IncFreq.Value;
        }

        private void chkSweepActive_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSweepActive.Checked)
            {
                sweepModeON = true;
                startFreq = (float)numUD_StartFreq.Value;
                stopFreq = (float)numUD_StopFreq.Value;
                IncFreq = (float)numUD_IncFreq.Value;
                dwelltime = (float)numUD_time_ms_forIncFreq.Value / 1000;       
                sweeps = 1000000;
                setSingnalGen();
            }
            else
            {
                sweepModeON = false;
                setSingnalGen();
            }
        }

        private void cboSweepT_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboSweepT.SelectedIndex)
            { 
                case 0:
                    sweepT = Imports.SweepType.UP;
                    break;
                case 1:
                    sweepT = Imports.SweepType.DOWN;
                    break;
                case 2:
                    sweepT = Imports.SweepType.UPDOWN;
                    break;
                case 3:
                    sweepT = Imports.SweepType.DOWNUP;
                    break;
            }
            setSingnalGen();
        }

        private void numUD_StopFreq_ValueChanged(object sender, EventArgs e)
        {
            stopFreq =(float)numUD_StopFreq.Value;
            setSingnalGen();
        }

        private void numUD_time_ms_forIncFreq_ValueChanged(object sender, EventArgs e)
        {
            dwelltime = (float)numUD_time_ms_forIncFreq.Value/1000;
            setSingnalGen();
        }
    }
}
