using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using Merlin.Mfc;

namespace Merlin.DomainModel
{
    /// <summary>
    /// A class representing a Hover .TEX file.
    /// Contains a shared palette and up to 65535 textures.
    /// </summary>
    [MfcSerialisable]
    public class TexturePack : MfcObject
    {
        public const int PALETTE_ENTRIES = 256;

        public Color[] Palette { get; set; }
        public List<CMerlinTexture> Textures { get; set; }

        public CMerlinTexture this[string name]
        {
            get
            {
                foreach (var texture in Textures)
                {
                    if (texture.Name == name) return texture;
                }
                throw new ArgumentException("No such texture: " + name);
            }
        }

        public int TransparentPaletteIndex
        {
            get
            {
                return 0;
            }
        }

        public override void Deserialise(MfcDeserialiser archive)
        {
            Palette = new Color[PALETTE_ENTRIES];
            for (int i = 0; i < PALETTE_ENTRIES; i++)
            {
                int red = archive.DeserialiseByte();
                int green = archive.DeserialiseByte();
                int blue = archive.DeserialiseByte();
                int alpha = archive.DeserialiseByte(); if (alpha != 0) throw new Exception();
                Palette[i] = Color.FromArgb(alpha, red, green, blue);
            }

            Textures = archive.DeserialiseBuggyList<CMerlinTexture>();

            archive.DeserialiseObjectNoHeader<TrailingBytes>();
        }

        public override void Serialise(MfcSerialiser archive)
        {
            foreach (var colour in Palette)
            {
                archive.SerialiseByte(colour.R);
                archive.SerialiseByte(colour.G);
                archive.SerialiseByte(colour.B);
                archive.SerialiseByte(0); /* Never any alpha, transparency is handled by PixelSpans */
            }

            archive.SerialiseBuggyList(Textures, 0);
            archive.SerialiseObjectNoHeader(new TrailingBytes());
        }
    }
}
