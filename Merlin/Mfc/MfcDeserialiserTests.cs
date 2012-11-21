using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merlin.Mfc
{
    using System.IO;
    using NUnit.Framework;
    using Merlin.DomainModel;

    [TestFixture]
    class MfcDeserialiserTests
    {
        [Test]
        public void TestDeserialiseMerlinStatic()
        {
            FileStream smallMazeFile = new FileStream(@"C:\Users\Philip\Desktop\HoverGame\HOVER\MAZES\MAZE1.MAZ", FileMode.Open);
            var classRegistry = new MfcClassRegistry();
            classRegistry.RegisterClass("CMerlinStatic", typeof(CMerlinStatic));
            classRegistry.RegisterClass("CMerlinLocation", typeof(CMerlinLocation));

            MfcDeserialiser archive = new MfcDeserialiser(smallMazeFile, classRegistry);
            Maze maze = new Maze();
            maze.Deserialise(archive);
        }
    }
}
