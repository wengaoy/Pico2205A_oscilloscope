namespace Pico2205A
{
    partial class SignalGenerator_builtIn
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
            this.label1 = new System.Windows.Forms.Label();
            this.chkSG_ON = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.numUD_offset = new System.Windows.Forms.NumericUpDown();
            this.numUD_pk2pk = new System.Windows.Forms.NumericUpDown();
            this.numUD_StartFreq = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboWaveType = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numUD_time_ms_forIncFreq = new System.Windows.Forms.NumericUpDown();
            this.numUD_IncFreq = new System.Windows.Forms.NumericUpDown();
            this.numUD_StopFreq = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cboSweepT = new System.Windows.Forms.ComboBox();
            this.chkSweepActive = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_offset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_pk2pk)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_StartFreq)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_time_ms_forIncFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_IncFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_StopFreq)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Frequency";
            // 
            // chkSG_ON
            // 
            this.chkSG_ON.AutoSize = true;
            this.chkSG_ON.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkSG_ON.Location = new System.Drawing.Point(168, 17);
            this.chkSG_ON.Name = "chkSG_ON";
            this.chkSG_ON.Size = new System.Drawing.Size(86, 20);
            this.chkSG_ON.TabIndex = 1;
            this.chkSG_ON.Text = "Signal On";
            this.chkSG_ON.UseVisualStyleBackColor = true;
            this.chkSG_ON.CheckedChanged += new System.EventHandler(this.chkSG_ON_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.numUD_offset);
            this.groupBox1.Controls.Add(this.numUD_pk2pk);
            this.groupBox1.Controls.Add(this.numUD_StartFreq);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cboWaveType);
            this.groupBox1.Controls.Add(this.chkSG_ON);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(1, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(297, 164);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(6, 43);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(146, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Arbitrary...";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // numUD_offset
            // 
            this.numUD_offset.Location = new System.Drawing.Point(168, 127);
            this.numUD_offset.Maximum = new decimal(new int[] {
            2000000,
            0,
            0,
            0});
            this.numUD_offset.Minimum = new decimal(new int[] {
            2000000,
            0,
            0,
            -2147483648});
            this.numUD_offset.Name = "numUD_offset";
            this.numUD_offset.Size = new System.Drawing.Size(120, 21);
            this.numUD_offset.TabIndex = 8;
            this.toolTip1.SetToolTip(this.numUD_offset, "OffsetMax<Offset+Amplitude (mV)");
            this.numUD_offset.ValueChanged += new System.EventHandler(this.numUD_offset_ValueChanged);
            // 
            // numUD_pk2pk
            // 
            this.numUD_pk2pk.Location = new System.Drawing.Point(168, 100);
            this.numUD_pk2pk.Maximum = new decimal(new int[] {
            2000000,
            0,
            0,
            0});
            this.numUD_pk2pk.Name = "numUD_pk2pk";
            this.numUD_pk2pk.Size = new System.Drawing.Size(120, 21);
            this.numUD_pk2pk.TabIndex = 7;
            this.toolTip1.SetToolTip(this.numUD_pk2pk, "Max Amplitude is 2V( Pk2Pk is 4V)");
            this.numUD_pk2pk.Value = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numUD_pk2pk.ValueChanged += new System.EventHandler(this.numUD_pk2pk_ValueChanged);
            this.numUD_pk2pk.Enter += new System.EventHandler(this.numUD_pk2pk_Enter);
            // 
            // numUD_StartFreq
            // 
            this.numUD_StartFreq.Location = new System.Drawing.Point(168, 73);
            this.numUD_StartFreq.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numUD_StartFreq.Name = "numUD_StartFreq";
            this.numUD_StartFreq.Size = new System.Drawing.Size(120, 21);
            this.numUD_StartFreq.TabIndex = 6;
            this.numUD_StartFreq.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numUD_StartFreq.ValueChanged += new System.EventHandler(this.numUD_freq_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "Offset";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Amplitude";
            // 
            // cboWaveType
            // 
            this.cboWaveType.FormattingEnabled = true;
            this.cboWaveType.Items.AddRange(new object[] {
            "Arbitrary (AWG)",
            "Sine",
            "Square",
            "Triangle",
            "Ramp Up",
            "Ramp Down",
            "Half Sine",
            "Gaussian",
            "DC Voltage",
            "Sin(x)/x"});
            this.cboWaveType.Location = new System.Drawing.Point(168, 43);
            this.cboWaveType.Name = "cboWaveType";
            this.cboWaveType.Size = new System.Drawing.Size(121, 21);
            this.cboWaveType.TabIndex = 2;
            this.cboWaveType.SelectedIndexChanged += new System.EventHandler(this.cboWaveType_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.numUD_time_ms_forIncFreq);
            this.groupBox2.Controls.Add(this.numUD_IncFreq);
            this.groupBox2.Controls.Add(this.numUD_StopFreq);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cboSweepT);
            this.groupBox2.Controls.Add(this.chkSweepActive);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(1, 173);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(297, 163);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            // 
            // numUD_time_ms_forIncFreq
            // 
            this.numUD_time_ms_forIncFreq.Location = new System.Drawing.Point(168, 122);
            this.numUD_time_ms_forIncFreq.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numUD_time_ms_forIncFreq.Name = "numUD_time_ms_forIncFreq";
            this.numUD_time_ms_forIncFreq.Size = new System.Drawing.Size(120, 21);
            this.numUD_time_ms_forIncFreq.TabIndex = 11;
            this.toolTip1.SetToolTip(this.numUD_time_ms_forIncFreq, "1~10000ms");
            this.numUD_time_ms_forIncFreq.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numUD_time_ms_forIncFreq.ValueChanged += new System.EventHandler(this.numUD_time_ms_forIncFreq_ValueChanged);
            // 
            // numUD_IncFreq
            // 
            this.numUD_IncFreq.Location = new System.Drawing.Point(168, 94);
            this.numUD_IncFreq.Maximum = new decimal(new int[] {
            99000,
            0,
            0,
            0});
            this.numUD_IncFreq.Name = "numUD_IncFreq";
            this.numUD_IncFreq.Size = new System.Drawing.Size(120, 21);
            this.numUD_IncFreq.TabIndex = 10;
            this.toolTip1.SetToolTip(this.numUD_IncFreq, "1~99000Hz");
            this.numUD_IncFreq.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numUD_IncFreq.ValueChanged += new System.EventHandler(this.numUD_IncFreq_ValueChanged);
            // 
            // numUD_StopFreq
            // 
            this.numUD_StopFreq.Location = new System.Drawing.Point(168, 66);
            this.numUD_StopFreq.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numUD_StopFreq.Name = "numUD_StopFreq";
            this.numUD_StopFreq.Size = new System.Drawing.Size(120, 21);
            this.numUD_StopFreq.TabIndex = 9;
            this.toolTip1.SetToolTip(this.numUD_StopFreq, "1~100000Hz");
            this.numUD_StopFreq.Value = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numUD_StopFreq.ValueChanged += new System.EventHandler(this.numUD_StopFreq_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(6, 124);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(146, 16);
            this.label8.TabIndex = 8;
            this.label8.Text = "Increment Time Interval";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(90, 16);
            this.label7.TabIndex = 7;
            this.label7.Text = "Sweep Mode";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 16);
            this.label4.TabIndex = 5;
            this.label4.Text = "Frequency Increment";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 16);
            this.label5.TabIndex = 3;
            this.label5.Text = "Stop Frequency";
            // 
            // cboSweepT
            // 
            this.cboSweepT.FormattingEnabled = true;
            this.cboSweepT.Items.AddRange(new object[] {
            "UP",
            "DOWN",
            "UPDOWN",
            "DOWNUP"});
            this.cboSweepT.Location = new System.Drawing.Point(168, 38);
            this.cboSweepT.Name = "cboSweepT";
            this.cboSweepT.Size = new System.Drawing.Size(121, 21);
            this.cboSweepT.TabIndex = 2;
            this.cboSweepT.SelectedIndexChanged += new System.EventHandler(this.cboSweepT_SelectedIndexChanged);
            // 
            // chkSweepActive
            // 
            this.chkSweepActive.AutoSize = true;
            this.chkSweepActive.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkSweepActive.Location = new System.Drawing.Point(168, 13);
            this.chkSweepActive.Name = "chkSweepActive";
            this.chkSweepActive.Size = new System.Drawing.Size(69, 20);
            this.chkSweepActive.TabIndex = 1;
            this.chkSweepActive.Text = "Active";
            this.chkSweepActive.UseVisualStyleBackColor = true;
            this.chkSweepActive.CheckedChanged += new System.EventHandler(this.chkSweepActive_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(6, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 16);
            this.label6.TabIndex = 0;
            this.label6.Text = "Sweep Type";
            // 
            // SignalGenerator_builtIn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(302, 339);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SignalGenerator_builtIn";
            this.Opacity = 0.8D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Signal Generator";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SignalGenerator_builtIn_FormClosing);
            this.Load += new System.EventHandler(this.SignalGenerator_builtIn_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_offset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_pk2pk)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_StartFreq)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_time_ms_forIncFreq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_IncFreq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUD_StopFreq)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkSG_ON;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboWaveType;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboSweepT;
        private System.Windows.Forms.CheckBox chkSweepActive;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numUD_offset;
        private System.Windows.Forms.NumericUpDown numUD_pk2pk;
        private System.Windows.Forms.NumericUpDown numUD_StartFreq;
        private System.Windows.Forms.NumericUpDown numUD_time_ms_forIncFreq;
        private System.Windows.Forms.NumericUpDown numUD_IncFreq;
        private System.Windows.Forms.NumericUpDown numUD_StopFreq;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}