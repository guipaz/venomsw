namespace VenomTools
{
    partial class ConfigurerPrint
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
            this.printSelectionPanel1 = new VenomTools.PrintSelectionPanel();
            this.SuspendLayout();
            // 
            // printSelectionPanel1
            // 
            this.printSelectionPanel1.Location = new System.Drawing.Point(13, 13);
            this.printSelectionPanel1.Name = "printSelectionPanel1";
            this.printSelectionPanel1.Size = new System.Drawing.Size(200, 129);
            this.printSelectionPanel1.TabIndex = 1;
            // 
            // ConfigurerPrint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 154);
            this.Controls.Add(this.printSelectionPanel1);
            this.Name = "ConfigurerPrint";
            this.Text = "ConfigurerPrint";
            this.ResumeLayout(false);

        }

        #endregion
        private PrintSelectionPanel printSelectionPanel1;
    }
}