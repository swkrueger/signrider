namespace Signrider
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.datasetToolStrip = new System.Windows.Forms.ToolStrip();
            this.trainButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.legacyTestFunctionsPanel = new System.Windows.Forms.Panel();
            this.helloWorldTestButton = new System.Windows.Forms.Button();
            this.colourSegmentiseTestButton = new System.Windows.Forms.Button();
            this.shapeClassifierTestButton = new System.Windows.Forms.Button();
            this.RecongizerTrainButton = new System.Windows.Forms.Button();
            this.RecognizerTestButton = new System.Windows.Forms.Button();
            this.photoPanel = new System.Windows.Forms.Panel();
            this.selectedImageBox = new Emgu.CV.UI.ImageBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.imageSegmentsToolStrip = new System.Windows.Forms.ToolStrip();
            this.imageSetPanel = new System.Windows.Forms.Panel();
            this.imageSetToolStrip = new System.Windows.Forms.ToolStrip();
            this.addDirectoryImagesButton = new System.Windows.Forms.ToolStripButton();
            this.addImageButton = new System.Windows.Forms.ToolStripButton();
            this.removeImageButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.datasetToolStrip.SuspendLayout();
            this.legacyTestFunctionsPanel.SuspendLayout();
            this.photoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectedImageBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.imageSegmentsToolStrip.SuspendLayout();
            this.imageSetPanel.SuspendLayout();
            this.imageSetToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // datasetToolStrip
            // 
            this.datasetToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.datasetToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trainButton,
            this.toolStripSeparator1});
            this.datasetToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.datasetToolStrip.Location = new System.Drawing.Point(0, 0);
            this.datasetToolStrip.Name = "datasetToolStrip";
            this.datasetToolStrip.Size = new System.Drawing.Size(715, 54);
            this.datasetToolStrip.TabIndex = 4;
            this.datasetToolStrip.Text = "Data Set";
            // 
            // trainButton
            // 
            this.trainButton.Image = ((System.Drawing.Image)(resources.GetObject("trainButton.Image")));
            this.trainButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.trainButton.Name = "trainButton";
            this.trainButton.Size = new System.Drawing.Size(38, 51);
            this.trainButton.Text = "Train";
            this.trainButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.trainButton.ToolTipText = "Train from data set directory";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 54);
            // 
            // legacyTestFunctionsPanel
            // 
            this.legacyTestFunctionsPanel.Controls.Add(this.helloWorldTestButton);
            this.legacyTestFunctionsPanel.Controls.Add(this.colourSegmentiseTestButton);
            this.legacyTestFunctionsPanel.Controls.Add(this.shapeClassifierTestButton);
            this.legacyTestFunctionsPanel.Controls.Add(this.RecongizerTrainButton);
            this.legacyTestFunctionsPanel.Controls.Add(this.RecognizerTestButton);
            this.legacyTestFunctionsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.legacyTestFunctionsPanel.Location = new System.Drawing.Point(585, 54);
            this.legacyTestFunctionsPanel.Name = "legacyTestFunctionsPanel";
            this.legacyTestFunctionsPanel.Size = new System.Drawing.Size(130, 399);
            this.legacyTestFunctionsPanel.TabIndex = 5;
            // 
            // helloWorldTestButton
            // 
            this.helloWorldTestButton.Location = new System.Drawing.Point(8, 3);
            this.helloWorldTestButton.Name = "helloWorldTestButton";
            this.helloWorldTestButton.Size = new System.Drawing.Size(119, 23);
            this.helloWorldTestButton.TabIndex = 4;
            this.helloWorldTestButton.Text = "Hello World Test";
            this.helloWorldTestButton.UseVisualStyleBackColor = true;
            this.helloWorldTestButton.Click += new System.EventHandler(this.helloWorldTestButton_Click);
            // 
            // colourSegmentiseTestButton
            // 
            this.colourSegmentiseTestButton.Location = new System.Drawing.Point(8, 32);
            this.colourSegmentiseTestButton.Name = "colourSegmentiseTestButton";
            this.colourSegmentiseTestButton.Size = new System.Drawing.Size(119, 41);
            this.colourSegmentiseTestButton.TabIndex = 5;
            this.colourSegmentiseTestButton.Text = "Bulk Segmentise Images in Directory";
            this.colourSegmentiseTestButton.UseVisualStyleBackColor = true;
            this.colourSegmentiseTestButton.Click += new System.EventHandler(this.colourSegmentiseTestButton_Click);
            // 
            // shapeClassifierTestButton
            // 
            this.shapeClassifierTestButton.Location = new System.Drawing.Point(8, 79);
            this.shapeClassifierTestButton.Name = "shapeClassifierTestButton";
            this.shapeClassifierTestButton.Size = new System.Drawing.Size(119, 23);
            this.shapeClassifierTestButton.TabIndex = 6;
            this.shapeClassifierTestButton.Text = "Test Shape Classifier";
            this.shapeClassifierTestButton.UseVisualStyleBackColor = true;
            this.shapeClassifierTestButton.Click += new System.EventHandler(this.shapeClassifierTestButton_Click);
            // 
            // RecongizerTrainButton
            // 
            this.RecongizerTrainButton.Location = new System.Drawing.Point(8, 137);
            this.RecongizerTrainButton.Name = "RecongizerTrainButton";
            this.RecongizerTrainButton.Size = new System.Drawing.Size(119, 23);
            this.RecongizerTrainButton.TabIndex = 8;
            this.RecongizerTrainButton.Text = "Train Recongizer from Directory";
            this.RecongizerTrainButton.UseVisualStyleBackColor = true;
            this.RecongizerTrainButton.Click += new System.EventHandler(this.TrainFeutureRecognizerButton);
            // 
            // RecognizerTestButton
            // 
            this.RecognizerTestButton.Location = new System.Drawing.Point(8, 108);
            this.RecognizerTestButton.Name = "RecognizerTestButton";
            this.RecognizerTestButton.Size = new System.Drawing.Size(119, 23);
            this.RecognizerTestButton.TabIndex = 7;
            this.RecognizerTestButton.Text = "Recognizer Test";
            this.RecognizerTestButton.UseVisualStyleBackColor = true;
            this.RecognizerTestButton.Click += new System.EventHandler(this.FeutureRecognizerTestButton_Click);
            // 
            // photoPanel
            // 
            this.photoPanel.Controls.Add(this.selectedImageBox);
            this.photoPanel.Controls.Add(this.splitter2);
            this.photoPanel.Controls.Add(this.panel1);
            this.photoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.photoPanel.Location = new System.Drawing.Point(170, 54);
            this.photoPanel.Name = "photoPanel";
            this.photoPanel.Size = new System.Drawing.Size(415, 399);
            this.photoPanel.TabIndex = 7;
            // 
            // selectedImageBox
            // 
            this.selectedImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectedImageBox.Location = new System.Drawing.Point(0, 0);
            this.selectedImageBox.Name = "selectedImageBox";
            this.selectedImageBox.Size = new System.Drawing.Size(415, 283);
            this.selectedImageBox.TabIndex = 2;
            this.selectedImageBox.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.listView1);
            this.panel1.Controls.Add(this.imageSegmentsToolStrip);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 286);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(415, 113);
            this.panel1.TabIndex = 0;
            // 
            // imageSegmentsToolStrip
            // 
            this.imageSegmentsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.imageSegmentsToolStrip.Location = new System.Drawing.Point(0, 0);
            this.imageSegmentsToolStrip.Name = "imageSegmentsToolStrip";
            this.imageSegmentsToolStrip.Size = new System.Drawing.Size(413, 25);
            this.imageSegmentsToolStrip.TabIndex = 0;
            this.imageSegmentsToolStrip.Text = "toolStrip1";
            // 
            // imageSetPanel
            // 
            this.imageSetPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imageSetPanel.Controls.Add(this.imageSetToolStrip);
            this.imageSetPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.imageSetPanel.Location = new System.Drawing.Point(0, 54);
            this.imageSetPanel.Name = "imageSetPanel";
            this.imageSetPanel.Size = new System.Drawing.Size(170, 399);
            this.imageSetPanel.TabIndex = 8;
            // 
            // imageSetToolStrip
            // 
            this.imageSetToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addDirectoryImagesButton,
            this.addImageButton,
            this.removeImageButton});
            this.imageSetToolStrip.Location = new System.Drawing.Point(0, 0);
            this.imageSetToolStrip.Name = "imageSetToolStrip";
            this.imageSetToolStrip.Size = new System.Drawing.Size(168, 25);
            this.imageSetToolStrip.TabIndex = 0;
            this.imageSetToolStrip.Text = "toolStrip1";
            // 
            // addDirectoryImagesButton
            // 
            this.addDirectoryImagesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addDirectoryImagesButton.Image = ((System.Drawing.Image)(resources.GetObject("addDirectoryImagesButton.Image")));
            this.addDirectoryImagesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addDirectoryImagesButton.Name = "addDirectoryImagesButton";
            this.addDirectoryImagesButton.Size = new System.Drawing.Size(23, 22);
            this.addDirectoryImagesButton.Text = "Add all images in a directory";
            // 
            // addImageButton
            // 
            this.addImageButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addImageButton.Image = ((System.Drawing.Image)(resources.GetObject("addImageButton.Image")));
            this.addImageButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addImageButton.Name = "addImageButton";
            this.addImageButton.Size = new System.Drawing.Size(23, 22);
            this.addImageButton.Text = "Add image file";
            // 
            // removeImageButton
            // 
            this.removeImageButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeImageButton.Image = ((System.Drawing.Image)(resources.GetObject("removeImageButton.Image")));
            this.removeImageButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeImageButton.Name = "removeImageButton";
            this.removeImageButton.Size = new System.Drawing.Size(23, 22);
            this.removeImageButton.Text = "Remove selected image(s)";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(0, 25);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(413, 86);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(170, 54);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 399);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(0, 283);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(415, 3);
            this.splitter2.TabIndex = 2;
            this.splitter2.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 453);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.photoPanel);
            this.Controls.Add(this.imageSetPanel);
            this.Controls.Add(this.legacyTestFunctionsPanel);
            this.Controls.Add(this.datasetToolStrip);
            this.Name = "MainForm";
            this.Text = "SignRider";
            this.datasetToolStrip.ResumeLayout(false);
            this.datasetToolStrip.PerformLayout();
            this.legacyTestFunctionsPanel.ResumeLayout(false);
            this.photoPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.selectedImageBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.imageSegmentsToolStrip.ResumeLayout(false);
            this.imageSegmentsToolStrip.PerformLayout();
            this.imageSetPanel.ResumeLayout(false);
            this.imageSetPanel.PerformLayout();
            this.imageSetToolStrip.ResumeLayout(false);
            this.imageSetToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip datasetToolStrip;
        private System.Windows.Forms.ToolStripButton trainButton;
        private System.Windows.Forms.Panel legacyTestFunctionsPanel;
        private System.Windows.Forms.Button helloWorldTestButton;
        private System.Windows.Forms.Button colourSegmentiseTestButton;
        private System.Windows.Forms.Button shapeClassifierTestButton;
        private System.Windows.Forms.Button RecongizerTrainButton;
        private System.Windows.Forms.Button RecognizerTestButton;
        private System.Windows.Forms.Panel photoPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Panel imageSetPanel;
        private System.Windows.Forms.ToolStrip imageSetToolStrip;
        private System.Windows.Forms.ToolStripButton addDirectoryImagesButton;
        private System.Windows.Forms.ToolStripButton addImageButton;
        private System.Windows.Forms.ToolStripButton removeImageButton;
        private System.Windows.Forms.ToolStrip imageSegmentsToolStrip;
        private Emgu.CV.UI.ImageBox selectedImageBox;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
    }
}

