using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SlimDX.Windows;
using System.IO;
using Merlin;
using Merlin.DomainModel;
using Merlin.Mfc;

namespace HoverRenderer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FileStream smallMazeFile = new FileStream(@"C:\Users\Philip\Desktop\HoverGame\HOVER\MAZES\small.MAZ", FileMode.Open);
            var classRegistry = new MfcClassRegistry();
            classRegistry.AutoRegisterClasses(typeof(Maze).Assembly);

            MfcDeserialiser archive = new MfcDeserialiser(smallMazeFile, classRegistry);
            Maze maze = new Maze();
            maze.Deserialise(archive);
            
            var form = new HoverForm(maze);
            MainLoop renderFrame = form.RunFrame;
            MessagePump.Run(form, renderFrame);
        }
    }
}
