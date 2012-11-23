using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Merlin.DomainModel;

namespace DebugProject
{
    public partial class MazeEditorView : UserControl
    {
        SolidBrush solidBrush;
        SolidBrush selectedObjectBrush;
        private Font font;
        private Pen gridPen;
        private Pen wallPen;
        private Pen selectedObjectPen;

        private bool isDragging;
        private Point dragOrigin;
        private Point dragViewportOrigin;

        public Maze Maze { get; set; }

        private int _mazeScale;
        [DefaultValue(100)]
        public int MazeScale
        {
            get { return _mazeScale; }
            set
            {
                this._mazeScale = value;
                statusBarZoom.Text = "Zoom: " + _mazeScale.ToString();
                Invalidate();
            }
        }

        private Point _viewportOrigin;
        public Point ViewportOrigin
        {
            get { return _viewportOrigin; }
            set
            {
                this._viewportOrigin = value;
                statusBarViewport.Text = "Viewport: " + _viewportOrigin.ToString();
                Invalidate();
            }
        }

        private CMerlinObject _selectedObject;
        public CMerlinObject SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                this._selectedObject = value;
                Invalidate();
            }
        }

        public MazeEditorView()
        {
            InitializeComponent();

            MazeScale = 100;

            solidBrush = new SolidBrush(Color.White);
            selectedObjectBrush = new SolidBrush(Color.Red);
            font = new Font(Font.SystemFontName, 10.0f);
            gridPen = new Pen(Color.Gray);
            wallPen = new Pen(Color.Yellow);
            selectedObjectPen = new Pen(Color.Red);
        }

        private void MazeEditorView_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {

                g.Clear(Color.CornflowerBlue);

                g.TranslateTransform(ViewportOrigin.X, ViewportOrigin.Y);
                g.ScaleTransform(MazeScale / 1000.0f, MazeScale / 1000.0f);

                for (int i = Maze.MinCoordinate; i <= Maze.MaxCoordinate; i += 1024)
                {
                    g.DrawLine(gridPen, new Point(i, 0), new Point(i, Maze.MaxCoordinate));
                    g.DrawLine(gridPen, new Point(0, i), new Point(Maze.MaxCoordinate, i));
                }
                
                // Can't reference instance data if in VS designer
                if (DesignMode)
                {
                    g.ResetTransform();
                    g.DrawString("Design Mode", font, solidBrush, 16, 32);
                    return;
                }

                foreach (var wall in Maze.Geometry)
                {
                    g.DrawLine(wall == _selectedObject ? selectedObjectPen : wallPen, new Point(wall.X1, wall.Y1), new Point(wall.X2, wall.Y2));
                }

                foreach (var location in Maze.Locations)
                {
                    g.FillRectangle(location == _selectedObject ? selectedObjectBrush : solidBrush, location.X - 25, location.Y - 25, 50, 50);
                }

                g.ResetTransform();
                g.DrawString(MazeScale.ToString(), font, solidBrush, 16, 32);
                g.DrawString(ViewportOrigin.ToString(), font, solidBrush, 16, 64);

            }
            catch (Exception ex) {
                System.Diagnostics.Debugger.Break();
            }
        }

        private void MazeEditorView_MouseWheel(object sender, MouseEventArgs e)
        {
            ViewportOrigin = new Point(ViewportOrigin.X+1,1);
            if (e.Delta > 0)
                this.MazeScale += 5;
            else if (e.Delta < 0 && this.MazeScale > 5)
                this.MazeScale -= 5;
        }

        private void MazeEditorView_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            dragOrigin = e.Location;
            dragViewportOrigin = ViewportOrigin;
        }

        private void MazeEditorView_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void MazeEditorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                ViewportOrigin = new Point(
                    dragViewportOrigin.X + (e.Location.X - dragOrigin.X),
                    dragViewportOrigin.Y + (e.Location.Y - dragOrigin.Y));
                statusBarViewport.Text = "Viewport: " + ViewportOrigin.ToString();
                Invalidate();
            }
        }
    }
}
