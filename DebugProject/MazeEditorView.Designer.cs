namespace DebugProject
{
    partial class MazeEditorView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusBarViewport = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarZoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarViewport,
            this.statusBarZoom});
            this.statusBar.Location = new System.Drawing.Point(0, 128);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(150, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 4;
            this.statusBar.Text = "statusBar";
            // 
            // statusBarViewport
            // 
            this.statusBarViewport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusBarViewport.Name = "statusBarViewport";
            this.statusBarViewport.Size = new System.Drawing.Size(57, 17);
            this.statusBarViewport.Text = "Viewport:";
            // 
            // statusBarZoom
            // 
            this.statusBarZoom.Name = "statusBarZoom";
            this.statusBarZoom.Size = new System.Drawing.Size(42, 17);
            this.statusBarZoom.Text = "Zoom:";
            // 
            // MazeEditorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusBar);
            this.Name = "MazeEditorView";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MazeEditorView_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MazeEditorView_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MazeEditorView_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MazeEditorView_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MazeEditorView_MouseWheel);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel statusBarViewport;
        private System.Windows.Forms.ToolStripStatusLabel statusBarZoom;
    }
}
