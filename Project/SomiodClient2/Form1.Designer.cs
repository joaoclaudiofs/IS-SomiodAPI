namespace SomiodClient2
{
    partial class SomiodClient2Form
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
            this.listBoxContainers = new System.Windows.Forms.ListBox();
            this.trackBarTemp = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.labelTemp = new System.Windows.Forms.Label();
            this.buttonPostCI = new System.Windows.Forms.Button();
            this.checkBoxJson = new System.Windows.Forms.CheckBox();
            this.checkBoxXML = new System.Windows.Forms.CheckBox();
            this.checkBoxPlainText = new System.Windows.Forms.CheckBox();
            this.listBoxOutput = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTemp)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxContainers
            // 
            this.listBoxContainers.FormattingEnabled = true;
            this.listBoxContainers.ItemHeight = 25;
            this.listBoxContainers.Location = new System.Drawing.Point(12, 12);
            this.listBoxContainers.Name = "listBoxContainers";
            this.listBoxContainers.Size = new System.Drawing.Size(414, 679);
            this.listBoxContainers.TabIndex = 0;
            // 
            // trackBarTemp
            // 
            this.trackBarTemp.Location = new System.Drawing.Point(469, 52);
            this.trackBarTemp.Maximum = 30;
            this.trackBarTemp.Minimum = 15;
            this.trackBarTemp.Name = "trackBarTemp";
            this.trackBarTemp.Size = new System.Drawing.Size(472, 90);
            this.trackBarTemp.TabIndex = 1;
            this.trackBarTemp.Value = 20;
            this.trackBarTemp.Scroll += new System.EventHandler(this.trackBarTemp_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(469, 160);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Temperature: ";
            // 
            // labelTemp
            // 
            this.labelTemp.AutoSize = true;
            this.labelTemp.Location = new System.Drawing.Point(621, 160);
            this.labelTemp.Name = "labelTemp";
            this.labelTemp.Size = new System.Drawing.Size(70, 25);
            this.labelTemp.TabIndex = 3;
            this.labelTemp.Text = "label2";
            // 
            // buttonPostCI
            // 
            this.buttonPostCI.Location = new System.Drawing.Point(751, 134);
            this.buttonPostCI.Name = "buttonPostCI";
            this.buttonPostCI.Size = new System.Drawing.Size(167, 76);
            this.buttonPostCI.TabIndex = 4;
            this.buttonPostCI.Text = "Post";
            this.buttonPostCI.UseVisualStyleBackColor = true;
            this.buttonPostCI.Click += new System.EventHandler(this.buttonPostCI_Click);
            // 
            // checkBoxJson
            // 
            this.checkBoxJson.AutoSize = true;
            this.checkBoxJson.Checked = true;
            this.checkBoxJson.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxJson.Location = new System.Drawing.Point(465, 242);
            this.checkBoxJson.Name = "checkBoxJson";
            this.checkBoxJson.Size = new System.Drawing.Size(90, 29);
            this.checkBoxJson.TabIndex = 5;
            this.checkBoxJson.Text = "Json";
            this.checkBoxJson.UseVisualStyleBackColor = true;
            this.checkBoxJson.CheckedChanged += new System.EventHandler(this.checkBoxJson_CheckedChanged);
            // 
            // checkBoxXML
            // 
            this.checkBoxXML.AutoSize = true;
            this.checkBoxXML.Location = new System.Drawing.Point(621, 242);
            this.checkBoxXML.Name = "checkBoxXML";
            this.checkBoxXML.Size = new System.Drawing.Size(88, 29);
            this.checkBoxXML.TabIndex = 6;
            this.checkBoxXML.Text = "XML";
            this.checkBoxXML.UseVisualStyleBackColor = true;
            this.checkBoxXML.CheckedChanged += new System.EventHandler(this.checkBoxXML_CheckedChanged);
            // 
            // checkBoxPlainText
            // 
            this.checkBoxPlainText.AutoSize = true;
            this.checkBoxPlainText.Location = new System.Drawing.Point(777, 242);
            this.checkBoxPlainText.Name = "checkBoxPlainText";
            this.checkBoxPlainText.Size = new System.Drawing.Size(134, 29);
            this.checkBoxPlainText.TabIndex = 7;
            this.checkBoxPlainText.Text = "PlainText";
            this.checkBoxPlainText.UseVisualStyleBackColor = true;
            this.checkBoxPlainText.CheckedChanged += new System.EventHandler(this.checkBoxPlainText_CheckedChanged);
            // 
            // listBoxOutput
            // 
            this.listBoxOutput.FormattingEnabled = true;
            this.listBoxOutput.ItemHeight = 25;
            this.listBoxOutput.Location = new System.Drawing.Point(469, 326);
            this.listBoxOutput.Name = "listBoxOutput";
            this.listBoxOutput.Size = new System.Drawing.Size(449, 354);
            this.listBoxOutput.TabIndex = 8;
            // 
            // SomiodClient2Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.MenuBar;
            this.ClientSize = new System.Drawing.Size(1000, 725);
            this.Controls.Add(this.listBoxOutput);
            this.Controls.Add(this.checkBoxPlainText);
            this.Controls.Add(this.checkBoxXML);
            this.Controls.Add(this.checkBoxJson);
            this.Controls.Add(this.buttonPostCI);
            this.Controls.Add(this.labelTemp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarTemp);
            this.Controls.Add(this.listBoxContainers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "SomiodClient2Form";
            this.Text = "SomiodClient2";
            this.Load += new System.EventHandler(this.SomiodClient2Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTemp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxContainers;
        private System.Windows.Forms.TrackBar trackBarTemp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelTemp;
        private System.Windows.Forms.Button buttonPostCI;
        private System.Windows.Forms.CheckBox checkBoxJson;
        private System.Windows.Forms.CheckBox checkBoxXML;
        private System.Windows.Forms.CheckBox checkBoxPlainText;
        private System.Windows.Forms.ListBox listBoxOutput;
    }
}

