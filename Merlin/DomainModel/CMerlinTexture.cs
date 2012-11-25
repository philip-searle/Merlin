using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merlin.Mfc;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace Merlin.DomainModel
{
    public class MipMap
    {
        public Size ImageDimensionsMinusOne { get; set; }
        public Size ImageDimensions { get; set; }
        public uint Level { get; set; }
        public byte[] ImageData { get; set; }

        public List<List<PixelSpan>> PixelSpans { get; set; }

        public void UpdateImage(Bitmap bitmap, int mipmapLevel, bool hasTransparency, int transparentPaletteIndex)
        {
            if (bitmap.Size != ImageDimensions) throw new ArgumentException("Bitmap size must be " + ImageDimensions);
            bitmap.RotateFlip(RotateFlipType.Rotate270FlipY);

            var pixelSpans = new List<List<PixelSpan>>();
            MemoryStream data = new MemoryStream();
            byte[] rowData = new byte[ImageDimensions.Width];
            for (int y = 0; y < ImageDimensions.Height; y++)
            {
                List<PixelSpan> rowSpans = new List<PixelSpan>();
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, y, ImageDimensions.Width, 1), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                Marshal.Copy(bitmapData.Scan0, rowData, 0, ImageDimensions.Width);
                bitmap.UnlockBits(bitmapData);

                if (hasTransparency)
                {
                    ushort x = 0;
                    while (x < ImageDimensions.Width)
                    {
                        // Skip over transparent pixels
                        while (x < ImageDimensions.Width && rowData[x] == transparentPaletteIndex) x++;
                        if (x == ImageDimensions.Width) break;

                        // Start a pixel span
                        ushort spanStart = x;
                        while (x < ImageDimensions.Width && rowData[x] != transparentPaletteIndex) data.WriteByte(rowData[x++]);
                        rowSpans.Add(new PixelSpan((ushort)(spanStart << mipmapLevel), (ushort)((x - 1) << mipmapLevel)));
                    }
                }
                else
                {
                    data.Write(rowData, 0, ImageDimensions.Width);
                    rowSpans.Add(new PixelSpan(0, (ushort)((ImageDimensions.Width << mipmapLevel) - 1)));
                }
                pixelSpans.Add(rowSpans);
            }
            
            ImageData = data.ToArray();
            PixelSpans = pixelSpans;
        }
    }

    public struct PixelSpan
    {
        public ushort StartIndex;
        public ushort EndIndex;

        public PixelSpan(ushort startIndex, ushort endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    };

    [MfcSerialisable("CMerlinTexture")]
    public class CMerlinTexture : CMerlinObject
    {
        public bool HasTransparency { get; private set; }
        public List<MipMap> Mipmaps { get; private set; }

        public void UpdateImage(TexturePack containingPack, bool hasTransparency, Bitmap bitmap)
        {
            int mipLevel = 0;
            HasTransparency = hasTransparency;
            foreach (var mipmap in Mipmaps)
            {
                using (Bitmap scaledBitmap = ScaleBitmap(bitmap, mipLevel))
                {
                    mipmap.UpdateImage(scaledBitmap, mipLevel, HasTransparency, containingPack.TransparentPaletteIndex);
                    mipLevel++;
                }
            }
        }

        private Bitmap ScaleBitmap(Bitmap bitmap, int mipLevel)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width >> mipLevel, bitmap.Height >> mipLevel, bitmap.PixelFormat);
            byte[] sourcePixels = new byte[bitmap.Width];
            byte[] destPixels = new byte[newBitmap.Width];

            for (int y = 0; y < bitmap.Height; y += 1 << mipLevel)
            {
                BitmapData sourceData = bitmap.LockBits(new Rectangle(0, y, bitmap.Width, 1), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                BitmapData destData = newBitmap.LockBits(new Rectangle(0, y >> mipLevel, newBitmap.Width, 1), ImageLockMode.WriteOnly, newBitmap.PixelFormat);

                Marshal.Copy(sourceData.Scan0, sourcePixels, 0, bitmap.Width);
                for (int x = 0; x < bitmap.Width; x += 1 << mipLevel)
                {
                    destPixels[x >> mipLevel] = sourcePixels[x];
                }
                Marshal.Copy(destPixels, 0, destData.Scan0, destPixels.Length);

                newBitmap.UnlockBits(destData);
                bitmap.UnlockBits(sourceData);
            }

            return newBitmap;
        }

        public override void Deserialise(MfcDeserialiser archive)
        {
            base.Deserialise(archive);

            var flags = archive.DeserialiseUInt16();
            this.HasTransparency = (flags & 1) != 0;
            if ((flags & ~1) != 0)
            {
                throw new NotImplementedException("Unexpected flag set in texture header");
            }

            ushort mipmapCount = archive.DeserialiseUInt16();
            this.Mipmaps = new List<MipMap>(mipmapCount);
            for (int i = 0; i < mipmapCount; i++)
            {
                MipMap mipmap = new MipMap();
                var nextLargestHeight = archive.DeserialiseUInt16();
                var imageHeight = archive.DeserialiseUInt16();
                var nextLargestWidth = archive.DeserialiseUInt16();
                var imageWidth = archive.DeserialiseUInt16();

                mipmap.ImageDimensionsMinusOne = new Size(imageWidth, imageHeight);
                mipmap.ImageDimensions = new Size(nextLargestWidth, nextLargestHeight);
                mipmap.Level = archive.DeserialiseUInt16();

                var imageDataLength = archive.DeserialiseUInt32();
                mipmap.ImageData = archive.DeserialiseBytes((int)imageDataLength);

                int totalSpanCount = (int)archive.DeserialiseUInt32();
                mipmap.PixelSpans = new List<List<PixelSpan>>(totalSpanCount);
                for (int y = 0; y < nextLargestHeight; y++)
                {
                    ushort rowSpanCount = archive.DeserialiseUInt16();
                    var rowSpans = new List<PixelSpan>(rowSpanCount);
                    for (int k = 0; k < rowSpanCount; k++)
                    {
                        ushort startIndex = archive.DeserialiseUInt16();
                        ushort endIndex = archive.DeserialiseUInt16();
                        rowSpans.Add(new PixelSpan(startIndex, endIndex));
                    }
                    mipmap.PixelSpans.Add(rowSpans);
                }

                archive.DeserialiseObjectNoHeader<TrailingBytes>();

                this.Mipmaps.Add(mipmap);
            }
        }

        public override void Serialise(MfcSerialiser archive)
        {
            base.Serialise(archive);

            archive.SerialiseUInt16((ushort)(HasTransparency ? 1 : 0));

            archive.SerialiseUInt16((ushort)Mipmaps.Count);
            foreach (var mipmap in Mipmaps)
            {
                archive.SerialiseUInt16((ushort)mipmap.ImageDimensions.Height);
                archive.SerialiseUInt16((ushort)mipmap.ImageDimensionsMinusOne.Height);
                archive.SerialiseUInt16((ushort)mipmap.ImageDimensions.Width);
                archive.SerialiseUInt16((ushort)mipmap.ImageDimensionsMinusOne.Width);

                archive.SerialiseUInt16((ushort)mipmap.Level);
                archive.SerialiseUInt32((uint)mipmap.ImageData.Length);
                archive.SerialiseBytes(mipmap.ImageData);

                var spanCounts = from s in mipmap.PixelSpans select s.Count;
                archive.SerialiseUInt32((uint)spanCounts.Sum());
                foreach (var spanList in mipmap.PixelSpans)
                {
                    archive.SerialiseUInt16((ushort)spanList.Count);
                    foreach (var pixelSpan in spanList)
                    {
                        archive.SerialiseUInt16(pixelSpan.StartIndex);
                        archive.SerialiseUInt16(pixelSpan.EndIndex);
                    }
                }

                archive.SerialiseObjectNoHeader(new TrailingBytes());
            }
        }
    }
}
