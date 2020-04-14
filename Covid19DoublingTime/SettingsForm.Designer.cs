namespace Covid19DoublingTime
{
    partial class SettingsForm
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
            this.listBoxTypeOfData = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // listBoxTypeOfData
            // 
            this.listBoxTypeOfData.FormattingEnabled = true;
            this.listBoxTypeOfData.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.listBoxTypeOfData.Location = new System.Drawing.Point(13, 13);
            this.listBoxTypeOfData.Name = "listBoxTypeOfData";
            this.listBoxTypeOfData.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxTypeOfData.Size = new System.Drawing.Size(120, 95);
            this.listBoxTypeOfData.TabIndex = 0;
            this.listBoxTypeOfData.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listBoxTypeOfData);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxTypeOfData;
    }
}