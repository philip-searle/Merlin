using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Merlin.Mfc;

namespace Merlin.DomainModel
{
    /// <summary>
    /// A class representing a Hover .MAZ file.
    /// Contains a list of static geometry, locations of point entities, and a binary space partition tree.
    /// </summary>
    [MfcSerialisable]
    public class Maze : MfcObject
    {
        /// <summary>
        /// The smallest possible coordinate any object in a maze can have.
        /// </summary>
        public const int MinCoordinate = ushort.MinValue;

        /// <summary>
        /// The largest possible coordinate any object in a maze can have.
        /// </summary>
        public const int MaxCoordinate = ushort.MaxValue;

        public ushort MinX;
        public ushort MinY;
        public ushort MaxX;
        public ushort MaxY;

        public List<CMerlinStatic> Geometry { get; set; }
        public List<CMerlinLocation> Locations { get; set; }
        public List<CMerlinBsp> Bsp;

        override public void Deserialise(MfcDeserialiser archive)
        {
            // The strange order here is intentional and matches the order in the archive files
            MinY = archive.DeserialiseUInt16();
            MinX = archive.DeserialiseUInt16();
            MaxX = archive.DeserialiseUInt16();
            MaxY = archive.DeserialiseUInt16();

            Geometry = archive.DeserialiseBuggyList<CMerlinStatic>();

            int unknownPadding = archive.DeserialiseUInt16();
            if (unknownPadding != 0) throw new InvalidDataException("unknownPadding != 0; investigate!");

            Locations = archive.DeserialiseBuggyList<CMerlinLocation>();

            Bsp = archive.DeserialiseBuggyList<CMerlinBsp>();
        }

        public override void Serialise(MfcSerialiser archive)
        {
            archive.SerialiseUInt16(MinY);
            archive.SerialiseUInt16(MinX);
            archive.SerialiseUInt16(MaxX);
            archive.SerialiseUInt16(MaxY);

            ushort hack = 0;
            archive.SerialiseBuggyList(Geometry, hack);
            archive.SerialiseUInt16(0);

            hack += (ushort)Geometry.Count;
            archive.SerialiseBuggyList(Locations, hack);

            hack += (ushort)Locations.Count;
            archive.SerialiseBuggyList(Bsp, hack);
        }
    }
}
