namespace UUNATEK.WindowsForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpConnection = new GroupBox();
            lblComPort = new Label();
            txtComPort = new TextBox();
            lblBaudRate = new Label();
            txtBaudRate = new TextBox();
            btnConnect = new Button();
            grpStatus = new GroupBox();
            lblIsOpen = new Label();
            lblPortName = new Label();
            lblIsPrinting = new Label();
            btnGetStatus = new Button();
            grpSimulation = new GroupBox();
            lblPaper = new Label();
            cboPaper = new ComboBox();
            btnBrowse = new Button();
            lblFileName = new Label();
            picSimulation = new PictureBox();
            grpPrintSettings = new GroupBox();
            lblXPosition = new Label();
            txtXPosition = new TextBox();
            lblYPosition = new Label();
            txtYPosition = new TextBox();
            lblScale = new Label();
            nudScale = new NumericUpDown();
            lblRotation = new Label();
            nudRotation = new NumericUpDown();
            chkInvertX = new CheckBox();
            chkInvertY = new CheckBox();
            btnPrint = new Button();
            lblResult = new Label();
            openFileDialog = new OpenFileDialog();
            grpConnection.SuspendLayout();
            grpStatus.SuspendLayout();
            grpSimulation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSimulation).BeginInit();
            grpPrintSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRotation).BeginInit();
            SuspendLayout();
            // 
            // grpConnection
            // 
            grpConnection.Controls.Add(lblComPort);
            grpConnection.Controls.Add(txtComPort);
            grpConnection.Controls.Add(lblBaudRate);
            grpConnection.Controls.Add(txtBaudRate);
            grpConnection.Controls.Add(btnConnect);
            grpConnection.Dock = DockStyle.Top;
            grpConnection.Location = new Point(0, 0);
            grpConnection.Name = "grpConnection";
            grpConnection.Padding = new Padding(10);
            grpConnection.Size = new Size(885, 65);
            grpConnection.TabIndex = 0;
            grpConnection.TabStop = false;
            grpConnection.Text = "Serial Connection";
            // 
            // lblComPort
            // 
            lblComPort.AutoSize = true;
            lblComPort.Location = new Point(15, 28);
            lblComPort.Name = "lblComPort";
            lblComPort.Size = new Size(63, 15);
            lblComPort.TabIndex = 0;
            lblComPort.Text = "COM Port:";
            // 
            // txtComPort
            // 
            txtComPort.Location = new Point(85, 25);
            txtComPort.Name = "txtComPort";
            txtComPort.Size = new Size(80, 23);
            txtComPort.TabIndex = 1;
            txtComPort.Text = "COM5";
            // 
            // lblBaudRate
            // 
            lblBaudRate.AutoSize = true;
            lblBaudRate.Location = new Point(180, 28);
            lblBaudRate.Name = "lblBaudRate";
            lblBaudRate.Size = new Size(63, 15);
            lblBaudRate.TabIndex = 2;
            lblBaudRate.Text = "Baud Rate:";
            // 
            // txtBaudRate
            // 
            txtBaudRate.Location = new Point(255, 25);
            txtBaudRate.Name = "txtBaudRate";
            txtBaudRate.Size = new Size(80, 23);
            txtBaudRate.TabIndex = 3;
            txtBaudRate.Text = "250000";
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(350, 23);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(100, 28);
            btnConnect.TabIndex = 4;
            btnConnect.Text = "Connect";
            btnConnect.Click += btnConnect_Click;
            // 
            // grpStatus
            // 
            grpStatus.Dock = DockStyle.Top;
            grpStatus.Controls.Add(lblIsOpen);
            grpStatus.Controls.Add(lblPortName);
            grpStatus.Controls.Add(lblIsPrinting);
            grpStatus.Controls.Add(btnGetStatus);
            grpStatus.Location = new Point(0, 65);
            grpStatus.Name = "grpStatus";
            grpStatus.Padding = new Padding(10);
            grpStatus.Size = new Size(885, 55);
            grpStatus.TabIndex = 1;
            grpStatus.TabStop = false;
            grpStatus.Text = "Printer Status";
            // 
            // lblIsOpen
            // 
            lblIsOpen.AutoSize = true;
            lblIsOpen.Location = new Point(15, 25);
            lblIsOpen.Name = "lblIsOpen";
            lblIsOpen.Size = new Size(60, 15);
            lblIsOpen.TabIndex = 0;
            lblIsOpen.Text = "IsOpen: --";
            // 
            // lblPortName
            // 
            lblPortName.AutoSize = true;
            lblPortName.Location = new Point(150, 25);
            lblPortName.Name = "lblPortName";
            lblPortName.Size = new Size(45, 15);
            lblPortName.TabIndex = 1;
            lblPortName.Text = "Port: --";
            // 
            // lblIsPrinting
            // 
            lblIsPrinting.AutoSize = true;
            lblIsPrinting.Location = new Point(300, 25);
            lblIsPrinting.Name = "lblIsPrinting";
            lblIsPrinting.Size = new Size(65, 15);
            lblIsPrinting.TabIndex = 2;
            lblIsPrinting.Text = "Printing: --";
            // 
            // btnGetStatus
            // 
            btnGetStatus.Location = new Point(460, 20);
            btnGetStatus.Name = "btnGetStatus";
            btnGetStatus.Size = new Size(80, 28);
            btnGetStatus.TabIndex = 3;
            btnGetStatus.Text = "Refresh";
            btnGetStatus.Click += btnGetStatus_Click;
            // 
            // grpSimulation — fills left, stretches with form
            // 
            grpSimulation.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpSimulation.Controls.Add(lblPaper);
            grpSimulation.Controls.Add(cboPaper);
            grpSimulation.Controls.Add(btnBrowse);
            grpSimulation.Controls.Add(lblFileName);
            grpSimulation.Controls.Add(picSimulation);
            grpSimulation.Location = new Point(10, 125);
            grpSimulation.Name = "grpSimulation";
            grpSimulation.Padding = new Padding(10);
            grpSimulation.Size = new Size(555, 485);
            grpSimulation.TabIndex = 2;
            grpSimulation.TabStop = false;
            grpSimulation.Text = "Simulation";
            // 
            // lblPaper
            // 
            lblPaper.AutoSize = true;
            lblPaper.Location = new Point(15, 28);
            lblPaper.Name = "lblPaper";
            lblPaper.Size = new Size(40, 15);
            lblPaper.TabIndex = 0;
            lblPaper.Text = "Paper:";
            // 
            // cboPaper
            // 
            cboPaper.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPaper.Location = new Point(60, 25);
            cboPaper.Name = "cboPaper";
            cboPaper.Size = new Size(130, 23);
            cboPaper.TabIndex = 1;
            cboPaper.SelectedIndexChanged += OnSettingChanged;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(210, 24);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(90, 25);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.Click += btnBrowse_Click;
            // 
            // lblFileName
            // 
            lblFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFileName.AutoEllipsis = true;
            lblFileName.Location = new Point(310, 28);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(230, 15);
            lblFileName.TabIndex = 3;
            lblFileName.Text = "No file selected";
            // 
            // picSimulation
            // 
            picSimulation.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picSimulation.BackColor = Color.LightGray;
            picSimulation.BorderStyle = BorderStyle.FixedSingle;
            picSimulation.Location = new Point(15, 55);
            picSimulation.Name = "picSimulation";
            picSimulation.Size = new Size(525, 415);
            picSimulation.SizeMode = PictureBoxSizeMode.Zoom;
            picSimulation.TabIndex = 4;
            picSimulation.TabStop = false;
            picSimulation.Resize += OnSimulationResize;
            // 
            // grpPrintSettings — fixed width, anchored to right
            // 
            grpPrintSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            grpPrintSettings.Controls.Add(lblXPosition);
            grpPrintSettings.Controls.Add(txtXPosition);
            grpPrintSettings.Controls.Add(lblYPosition);
            grpPrintSettings.Controls.Add(txtYPosition);
            grpPrintSettings.Controls.Add(lblScale);
            grpPrintSettings.Controls.Add(nudScale);
            grpPrintSettings.Controls.Add(lblRotation);
            grpPrintSettings.Controls.Add(nudRotation);
            grpPrintSettings.Controls.Add(chkInvertX);
            grpPrintSettings.Controls.Add(chkInvertY);
            grpPrintSettings.Controls.Add(btnPrint);
            grpPrintSettings.Controls.Add(lblResult);
            grpPrintSettings.Location = new Point(575, 125);
            grpPrintSettings.Name = "grpPrintSettings";
            grpPrintSettings.Size = new Size(300, 485);
            grpPrintSettings.TabIndex = 3;
            grpPrintSettings.TabStop = false;
            grpPrintSettings.Text = "Print Settings";
            // 
            // lblXPosition
            // 
            lblXPosition.AutoSize = true;
            lblXPosition.Location = new Point(15, 33);
            lblXPosition.Name = "lblXPosition";
            lblXPosition.Size = new Size(63, 15);
            lblXPosition.TabIndex = 0;
            lblXPosition.Text = "X Position:";
            // 
            // txtXPosition
            // 
            txtXPosition.Location = new Point(95, 30);
            txtXPosition.Name = "txtXPosition";
            txtXPosition.Size = new Size(190, 23);
            txtXPosition.TabIndex = 1;
            txtXPosition.Text = "50mm";
            txtXPosition.TextChanged += OnSettingChanged;
            // 
            // lblYPosition
            // 
            lblYPosition.AutoSize = true;
            lblYPosition.Location = new Point(15, 68);
            lblYPosition.Name = "lblYPosition";
            lblYPosition.Size = new Size(63, 15);
            lblYPosition.TabIndex = 2;
            lblYPosition.Text = "Y Position:";
            // 
            // txtYPosition
            // 
            txtYPosition.Location = new Point(95, 65);
            txtYPosition.Name = "txtYPosition";
            txtYPosition.Size = new Size(190, 23);
            txtYPosition.TabIndex = 3;
            txtYPosition.Text = "50mm";
            txtYPosition.TextChanged += OnSettingChanged;
            // 
            // lblScale
            // 
            lblScale.AutoSize = true;
            lblScale.Location = new Point(15, 103);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(37, 15);
            lblScale.TabIndex = 4;
            lblScale.Text = "Scale:";
            // 
            // nudScale
            // 
            nudScale.Location = new Point(95, 100);
            nudScale.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudScale.Name = "nudScale";
            nudScale.Size = new Size(190, 23);
            nudScale.TabIndex = 5;
            nudScale.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudScale.ValueChanged += OnSettingChanged;
            // 
            // lblRotation
            // 
            lblRotation.AutoSize = true;
            lblRotation.Location = new Point(15, 138);
            lblRotation.Name = "lblRotation";
            lblRotation.Size = new Size(55, 15);
            lblRotation.TabIndex = 6;
            lblRotation.Text = "Rotation:";
            // 
            // nudRotation
            // 
            nudRotation.Location = new Point(95, 135);
            nudRotation.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudRotation.Name = "nudRotation";
            nudRotation.Size = new Size(190, 23);
            nudRotation.TabIndex = 7;
            nudRotation.ValueChanged += OnSettingChanged;
            // 
            // chkInvertX
            // 
            chkInvertX.AutoSize = true;
            chkInvertX.Location = new Point(95, 175);
            chkInvertX.Name = "chkInvertX";
            chkInvertX.Size = new Size(69, 19);
            chkInvertX.TabIndex = 8;
            chkInvertX.Text = "Invert X";
            chkInvertX.CheckedChanged += OnSettingChanged;
            // 
            // chkInvertY
            // 
            chkInvertY.AutoSize = true;
            chkInvertY.Checked = true;
            chkInvertY.CheckState = CheckState.Checked;
            chkInvertY.Location = new Point(195, 175);
            chkInvertY.Name = "chkInvertY";
            chkInvertY.Size = new Size(69, 19);
            chkInvertY.TabIndex = 9;
            chkInvertY.Text = "Invert Y";
            chkInvertY.CheckedChanged += OnSettingChanged;
            // 
            // btnPrint
            // 
            btnPrint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnPrint.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPrint.Location = new Point(15, 435);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(270, 35);
            btnPrint.TabIndex = 10;
            btnPrint.Text = "Print";
            btnPrint.Click += btnPrint_Click;
            // 
            // lblResult
            // 
            lblResult.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblResult.AutoEllipsis = true;
            lblResult.Location = new Point(15, 415);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(270, 17);
            lblResult.TabIndex = 11;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "SVG Files (*.svg)|*.svg";
            openFileDialog.Title = "Select an SVG File";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(885, 620);
            Controls.Add(grpSimulation);
            Controls.Add(grpPrintSettings);
            Controls.Add(grpStatus);
            Controls.Add(grpConnection);
            MinimumSize = new Size(750, 550);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "UunaTek Printer";
            grpConnection.ResumeLayout(false);
            grpConnection.PerformLayout();
            grpStatus.ResumeLayout(false);
            grpStatus.PerformLayout();
            grpSimulation.ResumeLayout(false);
            grpSimulation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picSimulation).EndInit();
            grpPrintSettings.ResumeLayout(false);
            grpPrintSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRotation).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpConnection;
        private Label lblComPort;
        private TextBox txtComPort;
        private Label lblBaudRate;
        private TextBox txtBaudRate;
        private Button btnConnect;

        private GroupBox grpStatus;
        private Label lblIsOpen;
        private Label lblPortName;
        private Label lblIsPrinting;
        private Button btnGetStatus;

        private GroupBox grpSimulation;
        private Label lblPaper;
        private ComboBox cboPaper;
        private Button btnBrowse;
        private Label lblFileName;
        private PictureBox picSimulation;

        private GroupBox grpPrintSettings;
        private Label lblXPosition;
        private TextBox txtXPosition;
        private Label lblYPosition;
        private TextBox txtYPosition;
        private Label lblScale;
        private NumericUpDown nudScale;
        private Label lblRotation;
        private NumericUpDown nudRotation;
        private CheckBox chkInvertX;
        private CheckBox chkInvertY;

        private Button btnPrint;
        private Label lblResult;

        private OpenFileDialog openFileDialog;
    }
}
