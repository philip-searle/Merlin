using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Merlin.DomainModel;
using System.Runtime.InteropServices;

namespace DebugProject
{
    public partial class MazeBrowser : Form, INotifyPropertyChanged, IMessageFilter
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Maze _maze;
        public Maze Maze
        {
            get { return _maze; }
            set
            {
                this._maze = value; RaisePropertyChanged("Maze");
            }
        }

        private CMerlinObject _selectedObject;
        public CMerlinObject SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                this._selectedObject = value;
                RaisePropertyChanged("SelectedObject");
            }
        }

        private void RaisePropertyChanged(string caller)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x20a)
            {
                // WM_MOUSEWHEEL, find the control at screen position m.LParam
                Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                IntPtr hWnd = WindowFromPoint(pos);
                if (hWnd != IntPtr.Zero && hWnd != m.HWnd && Control.FromHandle(hWnd) != null)
                {
                    SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
                    return true;
                }
            }
            return false;
        }

        // P/Invoke declarations
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point pt);
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        public MazeBrowser(Maze maze)
        {
            this.Maze = maze;

            InitializeComponent();
            Application.AddMessageFilter(this);

            editorView.Maze = maze;

            BindingSource locationsDataSource = new BindingSource(this.Maze, "Locations");
            BindingSource geometryDataSource = new BindingSource(this.Maze, "Geometry");

            listBox1.DataSource = locationsDataSource;
            listBox1.DisplayMember = "Name";

            listBox2.DataSource = geometryDataSource;
            listBox2.DisplayMember = "Name";

            propertyGrid.DataBindings.Add(new Binding("SelectedObject", this, "SelectedObject"));
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedObject = (CMerlinObject)((ListBox)sender).SelectedItem;
            this.SelectedObject = selectedObject;
            this.editorView.SelectedObject = selectedObject;
        }
    }
}
