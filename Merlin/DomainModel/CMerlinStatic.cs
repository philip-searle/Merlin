using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merlin.Mfc;

namespace Merlin.DomainModel
{
    [MfcSerialisable("CMerlinStatic")]
    public class CMerlinStatic : CMerlinLine
    {
        public string unknown08; // left side: draws from top of linedef to top of screen ('inside polygon looking out', i.e. ceiling)
        public string unknown09; // right side: draws from bottom of linedef to bottom of screen ('outside polygon looking in', i.e. floor)
        public string unknown10; // Wall texture, left side - confirmed
        public string unknown11; // Wall texture, right side - confirmed
        public string unknown12; // left side: draws from bottom of linedef to bottom of screen ('inside polygon looking out', i.e. floor)
        public string unknown13; // right side: draws from top of linedef to top of screen ('outside polygon looking in', i.e. ceiling)
        public ushort BottomZ;
        public ushort TopZ;
        public ushort unknown16;
        public ushort unknown17;
        public byte unknown18;  // Flag? Only seems to be set on decals.
        public byte unknown19;  // Flag? Only seems to be set on decals.
        public byte unknown20;  // If set, enable collision. If not set, no collision.
        public ushort unknown21;
        public byte unknown22;
        public ushort unknown23;
        public ushort unknown24;

        public override void Deserialise(MfcDeserialiser archive)
        {
            base.Deserialise(archive);

            unknown08 = archive.DeserialiseString();
            unknown09 = archive.DeserialiseString();
            unknown10 = archive.DeserialiseString();
            unknown11 = archive.DeserialiseString();
            unknown12 = archive.DeserialiseString();
            unknown13 = archive.DeserialiseString();
            BottomZ = archive.DeserialiseUInt16();
            TopZ = archive.DeserialiseUInt16();
            unknown16 = archive.DeserialiseUInt16();
            unknown17 = archive.DeserialiseUInt16();
            unknown18 = archive.DeserialiseByte();
            unknown19 = archive.DeserialiseByte();
            unknown20 = archive.DeserialiseByte();
            unknown21 = archive.DeserialiseUInt16();
            unknown22 = archive.DeserialiseByte();
            unknown23 = archive.DeserialiseUInt16();
            unknown24 = archive.DeserialiseUInt16();
        }

        public override void Serialise(MfcSerialiser archive)
        {
            base.Serialise(archive);

            archive.SerialiseString(unknown08);
            archive.SerialiseString(unknown09);
            archive.SerialiseString(unknown10);
            archive.SerialiseString(unknown11);
            archive.SerialiseString(unknown12);
            archive.SerialiseString(unknown13);
            archive.SerialiseUInt16(BottomZ);
            archive.SerialiseUInt16(TopZ);
            archive.SerialiseUInt16(unknown16);
            archive.SerialiseUInt16(unknown17);
            archive.SerialiseByte(unknown18);
            archive.SerialiseByte(unknown19);
            archive.SerialiseByte(unknown20);
            archive.SerialiseUInt16(unknown21);
            archive.SerialiseByte(unknown22);
            archive.SerialiseUInt16(unknown23);
            archive.SerialiseUInt16(unknown24);
        }
    }
}
