using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merlin.Mfc;
using System.ComponentModel;

namespace Merlin.DomainModel
{
    [MfcSerialisable("CMerlinStatic")]
    public class CMerlinStatic : CMerlinLine
    {
        // left side: texture top of linedef to top/bottom of screen (if below/above)
        [Category("Texture, left")]
        public string LeftTopTexture { get; set; }

        // right side: texture top of linedef to top/bottom of screen (if below/above)
        [Category("Texture, right")]
        public string RightTopTexture { get; set; }

        // Wall texture, left side
        [Category("Texture, left")]
        public string LeftWallTexture { get; set; }

        // Wall texture, right side
        [Category("Texture, right")]
        public string RightWallTexture { get; set; }

        // left side: texture bottom of linedef to top/bottom of screen (if below/above)
        [Category("Texture, left")]
        public string LeftBottomTexture { get; set; }

        // right side: texture top of linedef to top/bottom of screen (if below/above)
        [Category("Texture, right")]
        public string RightBottomTexture { get; set; }

        [Category("Wall")]
        public ushort BottomZ { get; set; }

        [Category("Wall")]
        public ushort TopZ { get; set; }

        [Category("Unknown")]
        public ushort unknown16 { get; set; }

        [Category("Unknown")]
        public ushort unknown17 { get; set; }

        // left side: if set, render wall texture with transparency
        [Category("Texture, left")]
        public byte LeftTextureIsTransparent { get; set; }

        // right side: if set, render wall texture with transparency
        [Category("Texture, right")]
        public byte RightTextureIsTransparent { get; set; }

        // If set, enable collision. If not set, no collision.
        [Category("Wall")]
        public byte EnableCollision { get; set; } 

        [Category("Unknown")]
        public ushort unknown21 { get; set; }

        [Category("Unknown")]
        public byte unknown22 { get; set; }

        // left side: texture offset in pixels
        [Category("Texture, left")]
        public ushort LeftTextureOffset { get; set; }

        // right side: texture offset in pixels
        [Category("Texture, right")]
        public ushort RightTextureOffset { get; set; }

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

            System.Diagnostics.Debug.Assert(unknown16 == 0);
            System.Diagnostics.Debug.Assert(unknown17 == 0);
            System.Diagnostics.Debug.Assert(unknown21 == 5);
            System.Diagnostics.Debug.Assert(unknown22 == 0);
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
