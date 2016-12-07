namespace XamarinSimulatedSensors.Windows
{
    partial class Form1
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
            this.textDeviceName = new System.Windows.Forms.TextBox();
            this.textConnectionString = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelDeviceName = new System.Windows.Forms.Label();
            this.labelConnectionString = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.trackBarTemperature = new System.Windows.Forms.TrackBar();
            this.labelTemperature = new System.Windows.Forms.Label();
            this.trackBarHumidity = new System.Windows.Forms.TrackBar();
            this.labelHumidity = new System.Windows.Forms.Label();
            this.buttonSend = new System.Windows.Forms.Button();
            this.textAlerts = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTemperature)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarHumidity)).BeginInit();
            this.SuspendLayout();
            // 
            // textDeviceName
            // 
            this.textDeviceName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textDeviceName.Location = new System.Drawing.Point(12, 331);
            this.textDeviceName.Name = "textDeviceName";
            this.textDeviceName.Size = new System.Drawing.Size(693, 29);
            this.textDeviceName.TabIndex = 0;
            // 
            // textConnectionString
            // 
            this.textConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textConnectionString.Location = new System.Drawing.Point(12, 401);
            this.textConnectionString.Multiline = true;
            this.textConnectionString.Name = "textConnectionString";
            this.textConnectionString.Size = new System.Drawing.Size(693, 98);
            this.textConnectionString.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::XamarinSimulatedSensors.Windows.Properties.Resources.CTDLogoMedium;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(693, 247);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // labelDeviceName
            // 
            this.labelDeviceName.AutoSize = true;
            this.labelDeviceName.ForeColor = System.Drawing.Color.White;
            this.labelDeviceName.Location = new System.Drawing.Point(12, 303);
            this.labelDeviceName.Name = "labelDeviceName";
            this.labelDeviceName.Size = new System.Drawing.Size(129, 25);
            this.labelDeviceName.TabIndex = 3;
            this.labelDeviceName.Text = "Device Name";
            // 
            // labelConnectionString
            // 
            this.labelConnectionString.AutoSize = true;
            this.labelConnectionString.ForeColor = System.Drawing.Color.White;
            this.labelConnectionString.Location = new System.Drawing.Point(12, 373);
            this.labelConnectionString.Name = "labelConnectionString";
            this.labelConnectionString.Size = new System.Drawing.Size(168, 25);
            this.labelConnectionString.TabIndex = 4;
            this.labelConnectionString.Text = "Connection String";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnect.Location = new System.Drawing.Point(251, 510);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(223, 34);
            this.buttonConnect.TabIndex = 5;
            this.buttonConnect.Text = "Connect The Dots";
            this.buttonConnect.UseVisualStyleBackColor = true;
            // 
            // trackBarTemperature
            // 
            this.trackBarTemperature.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarTemperature.Location = new System.Drawing.Point(12, 577);
            this.trackBarTemperature.Maximum = 100;
            this.trackBarTemperature.Name = "trackBarTemperature";
            this.trackBarTemperature.Size = new System.Drawing.Size(693, 80);
            this.trackBarTemperature.TabIndex = 7;
            this.trackBarTemperature.TabStop = false;
            this.trackBarTemperature.Value = 70;
            // 
            // labelTemperature
            // 
            this.labelTemperature.AutoSize = true;
            this.labelTemperature.ForeColor = System.Drawing.Color.White;
            this.labelTemperature.Location = new System.Drawing.Point(12, 549);
            this.labelTemperature.Name = "labelTemperature";
            this.labelTemperature.Size = new System.Drawing.Size(124, 25);
            this.labelTemperature.TabIndex = 6;
            this.labelTemperature.Text = "Temperature";
            // 
            // trackBarHumidity
            // 
            this.trackBarHumidity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarHumidity.Location = new System.Drawing.Point(12, 666);
            this.trackBarHumidity.Maximum = 100;
            this.trackBarHumidity.Name = "trackBarHumidity";
            this.trackBarHumidity.Size = new System.Drawing.Size(693, 80);
            this.trackBarHumidity.TabIndex = 9;
            this.trackBarHumidity.TabStop = false;
            this.trackBarHumidity.Value = 70;
            // 
            // labelHumidity
            // 
            this.labelHumidity.AutoSize = true;
            this.labelHumidity.ForeColor = System.Drawing.Color.White;
            this.labelHumidity.Location = new System.Drawing.Point(17, 638);
            this.labelHumidity.Name = "labelHumidity";
            this.labelHumidity.Size = new System.Drawing.Size(87, 25);
            this.labelHumidity.TabIndex = 8;
            this.labelHumidity.Text = "Humidity";
            // 
            // buttonSend
            // 
            this.buttonSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSend.Location = new System.Drawing.Point(251, 732);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(223, 34);
            this.buttonSend.TabIndex = 10;
            this.buttonSend.Text = "Send Data";
            this.buttonSend.UseVisualStyleBackColor = true;
            // 
            // textAlerts
            // 
            this.textAlerts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textAlerts.Location = new System.Drawing.Point(12, 788);
            this.textAlerts.Multiline = true;
            this.textAlerts.Name = "textAlerts";
            this.textAlerts.ReadOnly = true;
            this.textAlerts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textAlerts.Size = new System.Drawing.Size(693, 192);
            this.textAlerts.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(717, 992);
            this.Controls.Add(this.textAlerts);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.trackBarHumidity);
            this.Controls.Add(this.labelHumidity);
            this.Controls.Add(this.trackBarTemperature);
            this.Controls.Add(this.labelTemperature);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.labelConnectionString);
            this.Controls.Add(this.labelDeviceName);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textConnectionString);
            this.Controls.Add(this.textDeviceName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Device Simulator";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTemperature)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarHumidity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textDeviceName;
        private System.Windows.Forms.TextBox textConnectionString;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelDeviceName;
        private System.Windows.Forms.Label labelConnectionString;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TrackBar trackBarTemperature;
        private System.Windows.Forms.Label labelTemperature;
        private System.Windows.Forms.TrackBar trackBarHumidity;
        private System.Windows.Forms.Label labelHumidity;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.TextBox textAlerts;
    }
}

