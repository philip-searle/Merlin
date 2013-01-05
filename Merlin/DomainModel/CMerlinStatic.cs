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
        public string LeftTopTexture; // left side: texture top of linedef to top/bottom of screen (if below/above)
        public string RightTopTexture; // right side: texture top of linedef to top/bottom of screen (if below/above)
        public string LeftWallTexture; // Wall texture, left side
        public string RightWallTexture; // Wall texture, right side
        public string LeftBottomTexture; // left side: texture bottom of linedef to top/bottom of screen (if below/above)
        public string RightBottomTexture; // right side: texture top of linedef to top/bottom of screen (if below/above)
        public ushort BottomZ;
        public ushort TopZ;
        public ushort unknown16;
        public ushort unknown17;
        public byte LeftTextureIsTransparent;  // left side: if set, render wall texture with transparency
        public byte RightTextureIsTransparent;  // right side: if set, render wall texture with transparency
        public byte EnableCollision;  // If set, enable collision. If not set, no collision.
        public ushort unknown21;
        public byte unknown22;
        public ushort LeftTextureOffset; // left side: texture offset in pixels
        public ushort RightTextureOffset; // right side: texture offset in pixels

        public override void Deserialise(MfcDeserialiser archive)
        {
            base.Deserialise(archive);

            LeftTopTexture = archive.DeserialiseString();
            RightTopTexture = archive.DeserialiseString();
            LeftWallTexture = archive.DeserialiseString();
            RightWallTexture = archive.DeserialiseString();
            LeftBottomTexture = archive.DeserialiseString();
            RightBottomTexture = archive.DeserialiseString();
            BottomZ = archive.DeserialiseUInt16();
            TopZ = archive.DeserialiseUInt16();
            unknown16 = archive.DeserialiseUInt16();
            unknown17 = archive.DeserialiseUInt16();
            LeftTextureIsTransparent = archive.DeserialiseByte();
            RightTextureIsTransparent = archive.DeserialiseByte();
            EnableCollision = archive.DeserialiseByte();
            unknown21 = archive.DeserialiseUInt16();
            unknown22 = archive.DeserialiseByte();
            LeftTextureOffset = archive.DeserialiseUInt16();
            RightTextureOffset = archive.DeserialiseUInt16();
        }

        public override void Serialise(MfcSerialiser archive)
        {
            base.Serialise(archive);

            archive.SerialiseString(LeftTopTexture);
            archive.SerialiseString(RightTopTexture);
            archive.SerialiseString(LeftWallTexture);
            archive.SerialiseString(RightWallTexture);
            archive.SerialiseString(LeftBottomTexture);
            archive.SerialiseString(RightBottomTexture);
            archive.SerialiseUInt16(BottomZ);
            archive.SerialiseUInt16(TopZ);
            archive.SerialiseUInt16(unknown16);
            archive.SerialiseUInt16(unknown17);
            archive.SerialiseByte(LeftTextureIsTransparent);
            archive.SerialiseByte(RightTextureIsTransparent);
            archive.SerialiseByte(EnableCollision);
            archive.SerialiseUInt16(unknown21);
            archive.SerialiseByte(unknown22);
            archive.SerialiseUInt16(LeftTextureOffset);
            archive.SerialiseUInt16(RightTextureOffset);
        }
    }
}
