namespace SignRider
{
    partial class MainForm
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
            this.helloWorldTestButton = new System.Windows.Forms.Button();
            this.colourSegmentiseTestButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // helloWorldTestButton
            // 
            this.helloWorldTestButton.Location = new System.Drawing.Point(77, 12);
            this.helloWorldTestButton.Name = "helloWorldTestButton";
            this.helloWorldTestButton.Size = new System.Drawing.Size(221, 23);
            this.helloWorldTestButton.TabIndex = 0;
            this.helloWorldTestButton.Text = "Hello World Test";
            this.helloWorldTestButton.UseVisualStyleBackColor = true;
            this.helloWorldTestButton.Click += new System.EventHandler(this.helloWorldTestButton_Click);
            // 
            // colourSegmentiseTestButton
            // 
            this.colourSegmentiseTestButton.Location = new System.Drawing.Point(77, 41);
            this.colourSegmentiseTestButton.Name = "colourSegmentiseTestButton";
            this.colourSegmentiseTestButton.Size = new System.Drawing.Size(221, 23);
            this.colourSegmentiseTestButton.TabIndex = 1;
            this.colourSegmentiseTestButton.Text = "Bulk Segmentise Images in Directory";
            this.colourSegmentiseTestButton.UseVisualStyleBackColor = true;
            this.colourSegmentiseTestButton.Click += new System.EventHandler(this.colourSegmentiseTestButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 256);
            this.Controls.Add(this.colourSegmentiseTestButton);
            this.Controls.Add(this.helloWorldTestButton);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button helloWorldTestButton;
        private System.Windows.Forms.Button colourSegmentiseTestButton;
    }
}

