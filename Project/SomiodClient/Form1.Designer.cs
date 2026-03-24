namespace SomiodClient
{
    partial class SomiodClientForm
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
            this.listBoxMessages = new System.Windows.Forms.ListBox();
            this.lblTemperature = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblHouseName = new System.Windows.Forms.Label();
            this.lblContainerName = new System.Windows.Forms.Label();
            this.lblACName = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.comboBoxContainers = new System.Windows.Forms.ComboBox();
            this.comboBoxSubscriptions = new System.Windows.Forms.ComboBox();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.btnStopSub = new System.Windows.Forms.Button();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxMessages
            // 
            this.listBoxMessages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxMessages.FormattingEnabled = true;
            this.listBoxMessages.Location = new System.Drawing.Point(11, 379);
            this.listBoxMessages.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxMessages.Name = "listBoxMessages";
            this.listBoxMessages.Size = new System.Drawing.Size(321, 173);
            this.listBoxMessages.TabIndex = 6;
            this.listBoxMessages.SelectedIndexChanged += new System.EventHandler(this.listBoxMessages_SelectedIndexChanged);
            // 
            // lblTemperature
            // 
            this.lblTemperature.AutoSize = true;
            this.lblTemperature.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTemperature.Location = new System.Drawing.Point(78, 62);
            this.lblTemperature.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTemperature.Name = "lblTemperature";
            this.lblTemperature.Size = new System.Drawing.Size(92, 33);
            this.lblTemperature.TabIndex = 7;
            this.lblTemperature.Text = "25 ºC";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 362);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Broker Chat";
            // 
            // lblHouseName
            // 
            this.lblHouseName.AutoSize = true;
            this.lblHouseName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHouseName.Location = new System.Drawing.Point(35, 19);
            this.lblHouseName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHouseName.Name = "lblHouseName";
            this.lblHouseName.Size = new System.Drawing.Size(105, 13);
            this.lblHouseName.TabIndex = 24;
            this.lblHouseName.Text = "Available devices at ";
            // 
            // lblContainerName
            // 
            this.lblContainerName.AutoSize = true;
            this.lblContainerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContainerName.Location = new System.Drawing.Point(35, 76);
            this.lblContainerName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblContainerName.Name = "lblContainerName";
            this.lblContainerName.Size = new System.Drawing.Size(88, 13);
            this.lblContainerName.TabIndex = 25;
            this.lblContainerName.Text = "Subscriptions for ";
            // 
            // lblACName
            // 
            this.lblACName.AutoSize = true;
            this.lblACName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblACName.Location = new System.Drawing.Point(67, 16);
            this.lblACName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblACName.Name = "lblACName";
            this.lblACName.Size = new System.Drawing.Size(96, 16);
            this.lblACName.TabIndex = 27;
            this.lblACName.Text = "Temperature";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(81, 139);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(81, 16);
            this.lblStatus.TabIndex = 28;
            this.lblStatus.Text = "Label Status";
            // 
            // comboBoxContainers
            // 
            this.comboBoxContainers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxContainers.FormattingEnabled = true;
            this.comboBoxContainers.Location = new System.Drawing.Point(38, 37);
            this.comboBoxContainers.Name = "comboBoxContainers";
            this.comboBoxContainers.Size = new System.Drawing.Size(183, 21);
            this.comboBoxContainers.TabIndex = 29;
            this.comboBoxContainers.SelectedIndexChanged += new System.EventHandler(this.comboBoxContainers_SelectedIndexChanged);
            // 
            // comboBoxSubscriptions
            // 
            this.comboBoxSubscriptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxSubscriptions.FormattingEnabled = true;
            this.comboBoxSubscriptions.Location = new System.Drawing.Point(38, 94);
            this.comboBoxSubscriptions.Name = "comboBoxSubscriptions";
            this.comboBoxSubscriptions.Size = new System.Drawing.Size(183, 21);
            this.comboBoxSubscriptions.TabIndex = 30;
            this.comboBoxSubscriptions.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubscriptions_SelectedIndexChanged);
            // 
            // pnlStatus
            // 
            this.pnlStatus.Controls.Add(this.lblTemperature);
            this.pnlStatus.Controls.Add(this.lblACName);
            this.pnlStatus.Controls.Add(this.lblStatus);
            this.pnlStatus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pnlStatus.Location = new System.Drawing.Point(38, 135);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(271, 167);
            this.pnlStatus.TabIndex = 31;
            // 
            // btnStopSub
            // 
            this.btnStopSub.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnStopSub.Location = new System.Drawing.Point(234, 93);
            this.btnStopSub.Name = "btnStopSub";
            this.btnStopSub.Size = new System.Drawing.Size(75, 23);
            this.btnStopSub.TabIndex = 32;
            this.btnStopSub.Text = "Stop";
            this.btnStopSub.UseVisualStyleBackColor = true;
            this.btnStopSub.Click += new System.EventHandler(this.btnStopSub_Click);
            // 
            // SomiodClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(347, 596);
            this.Controls.Add(this.btnStopSub);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.comboBoxSubscriptions);
            this.Controls.Add(this.comboBoxContainers);
            this.Controls.Add(this.lblContainerName);
            this.Controls.Add(this.lblHouseName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxMessages);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SomiodClientForm";
            this.Text = "SomiodClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SomiodClientForm_FormClosing);
            this.Load += new System.EventHandler(this.SomiodClientForm_Load);
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox listBoxMessages;
        private System.Windows.Forms.Label lblTemperature;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblHouseName;
        private System.Windows.Forms.Label lblContainerName;
        private System.Windows.Forms.Label lblACName;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox comboBoxContainers;
        private System.Windows.Forms.ComboBox comboBoxSubscriptions;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Button btnStopSub;
    }
}

