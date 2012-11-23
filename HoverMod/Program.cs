using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Merlin;
using Merlin.DomainModel;
using Merlin.Mfc;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Xml;
using NDesk.Options;

namespace DebugProject
{
    class Program
    {
        string processName;
        MfcClassRegistry ClassRegistry = new MfcClassRegistry();

        bool ShowHelp = false;
        bool DumpSvg = false;
        bool ExtractTextures = false;
        Maze MazeArchive;
        TexturePack TexturePackArchive;
        string OutputDirectory = null;
        string SvgFile = null;

        static void Main(string[] args)
        {
            new Program(args).Run();
        }

        private Program(string[] args)
        {
            string TexturePackFile = null;
            string MazeFile = null;

            processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            ClassRegistry.AutoRegisterClasses(typeof(Maze).Assembly);

            var options = new OptionSet()
            {
                {   "h|help", "show this message and exit",
                    value => ShowHelp = value != null },
                {   "t|texturepack=", "the texture pack (.TEX) file to load",
                    value => TexturePackFile = value },
                {   "m|maze=", "the maze (.MAZ) file to load",
                    value => MazeFile = value },
                {   "s|svg=", "output an SVG file showing the maze layout",
                    value => DumpSvg = value != null },
                {   "x|extract=", "extract all textures to the specified directory",
                    value => { ExtractTextures = value != null; OutputDirectory = value; } }
            };

            try
            {
                options.Parse(args);
            }
            catch (OptionException ex)
            {
                Console.Write(processName);
                Console.Write(": ");
                Console.WriteLine(ex.Message);
                Console.WriteLine(string.Format("Try `{0} --help' for more information.", processName));
                return;
            }

            if (ShowHelp)
            {
                Console.WriteLine(string.Format("Usage: {0} [OPTIONS]+", processName));
                Console.WriteLine("Load, manipulate, and resave a Hover archive file.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (MazeFile != null)
            {
                using (var mazeStream = new FileStream(MazeFile, FileMode.Open))
                {
                    var mazeDeserialiser = new MfcDeserialiser(mazeStream, ClassRegistry);
                    MazeArchive = mazeDeserialiser.DeserialiseObjectNoHeader<Maze>();
                }
            }
            if (TexturePackFile != null)
            {
                using (var texturePackStream = new FileStream(TexturePackFile, FileMode.Open))
                {
                    var texturePackDeserialiser = new MfcDeserialiser(texturePackStream, ClassRegistry);
                    TexturePackArchive = texturePackDeserialiser.DeserialiseObjectNoHeader<TexturePack>();
                }
            }
        }
        
        private void Run()
        {
            if (DumpSvg)
            {
                DumpLevelToSvg();
            }/*
            switch (args[0])
            {
                case "-dumpsvg":
                DumpLevelToSvg(maze, args[1]);
                    break;
                case "-dumpbsp":
                Console.WriteLine("Dumping BSP to " + args[1]);
                DumpBspToDot(maze, args[1]);
                    break;
                case "-dumpmiptex":
                    Console.WriteLine("Dumping mipmap data to " + args[1]);

                    using (Bitmap b = new Bitmap(@"C:\Users\Philip\Desktop\HoverGame\FBASE_01_MIPMAP0.gif"))
                    {
                        texturePack["FBASE_01"].UpdateImage(texturePack, b);
                        //texturePack[string.Format("WBASE_{0:d2}", i)].HasTransparency = true;
                    }
                    using (Stream output = new FileStream(@"C:\Users\Philip\Desktop\HoverGame\HOVER - Copy\MAZES\TEXT1.TEX", FileMode.Create))
                    {
                        new MfcSerialiser(output, classRegistry).SerialiseObjectNoHeader(texturePack);
                    }
                    DumpTexturesToTxt(texturePack, args[1]);
                    break;
                case "-gui":
                    Console.WriteLine("Loading GUI");
                    new MazeBrowser(maze).ShowDialog();
                    break;
                default:
                Console.WriteLine("Unknown argument " + args[1]);
                    break;
            }*/
        }

        private static void DumpBspToDot(Maze maze, string filename)
        {
            var root = maze.Bsp[0];
            var output = new StreamWriter(filename, false, Encoding.UTF8);

            output.WriteLine("digraph g {");
            foreach (var node in maze.Bsp)
            {
                output.WriteLine("BSP{0:x4} [ label = \"{0:x4}:\\nCheck {1}\" ]", node.unknown01, node.unknown02);
                if (node.unknown03 == 0xffff)
                {
                    output.WriteLine("X{0}\t[ label = \"x\" shape=\"circle\" style=\"dotted\"]", node.unknown01);
                    output.WriteLine("BSP{0:x4} -> X{0}\t[ label = \"L\" ]", node.unknown01);
                }
                else
                {
                    output.WriteLine("BSP{0:x4} -> BSP{1:x4}\t[ label = \"L\" ]", node.unknown01, node.unknown03);
                }
                if (node.unknown04 == 0xffff)
                {
                    output.WriteLine("X{0}\t[ label = \"x\" shape=\"circle\" style=\"dotted\"]", node.unknown01);
                    output.WriteLine("BSP{0:x4} -> X{0}\t[ label = \"R\" ]", node.unknown01);
                }
                else
                {
                    output.WriteLine("BSP{0:x4} -> BSP{1:x4}\t[ label = \"R\" ]", node.unknown01, node.unknown04);
                }
            }
            foreach (var node in maze.Bsp)
            {
            }
            output.WriteLine("}");
            output.Close();
        }

        private void DumpLevelToSvg()
        {
            const double SVG_SCALE_FACTOR = 0.05;

            var x1 = from m in MazeArchive.Geometry select m.X1;
            var x2 = x1.Union(from m in MazeArchive.Geometry select m.X2);
            var y1 = from m in MazeArchive.Geometry select m.Y1;
            var y2 = y1.Union(from m in MazeArchive.Geometry select m.Y2);
            var z1 = from l in MazeArchive.Locations select l.Z;
            var z2 = z1.Union(from m in MazeArchive.Geometry select m.BottomZ);
            var z3 = z2.Union(from m in MazeArchive.Geometry select m.TopZ);

            var minX = x2.Min() * SVG_SCALE_FACTOR;
            var maxX = x2.Max() * SVG_SCALE_FACTOR;
            var minY = y2.Min() * SVG_SCALE_FACTOR;
            var maxY = y2.Max() * SVG_SCALE_FACTOR;
            var minZ = z3.Min() * SVG_SCALE_FACTOR;
            var maxZ = z3.Max() * SVG_SCALE_FACTOR;

            var output = new StreamWriter(SvgFile, false, Encoding.UTF8);
            output.WriteLine("<svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' version='1.1' viewBox='{0} {1} {2} {3}' preserveAspectRatio='xMidYMid'>", minX, minY, maxX, maxY);
            output.WriteLine(@"<defs>
	            <style type='text/css'><![CDATA[
		            line {
			            stroke: rgb(255,0,0);
			            stroke-width: 2;
			            /*marker-end: url(#tail);
			            marker-start: url(#head);*/
		            }
                    line:hover {
			            stroke: rgb(255,255,255);
                        stroke-width: 4;
			            marker-end: url(#tail);
			            marker-start: url(#head);
                    }
                    line.unwall {
                        stroke: rgb(128,128,128);
                    }
                    line.decal {
                        stroke: rgb(0,255,0);
                    }
                    line.step {
                        stroke: rgb(255,255,0);
                    }
		            g {
			            fill: rgb(255,255,255);
			            stroke: rgb(255,255,255);
		            }
                    g line {
                        marker-start: none;
                        marker-end: none;
                    }
                    g.location-HUMAN,
                    g.location-HUMAN line {
                        fill: rgb(255,255,255);
                        stroke: rgb(255,255,255);
                    }
                    g.location-ROBOT,
                    g.location-ROBOT line {
                        fill: rgb(255,0,0);
                        stroke: rgb(255,0,0);
                    }
                    g.location-FLAG,
                    g.location-FLAG line {
                        fill: rgb(255,255,0);
                        stroke: rgb(255,255,0);
                    }
                    g.location-POD,
                    g.location-POD line {
                        fill: rgb(0,255,0);
                        stroke: rgb(0,255,0);
                    }
                    g.location-BEACON,
                    g.location-BEACON line {
                        fill: rgb(0,255,255);
                        stroke: rgb(0,255,255);
                    }
                    g>text {
                        fill: transparent;
                        stroke: transparent;
                    }
                    g:hover>text {
                        stroke: rgb(255,255,255);
                    }
                    g.location line {
                        stroke: rgb(255,255,255);
                    }
	            ]]></style>
	            <marker id='head' orient='auto' markerWidth='1' markerHeight='4' refX='0' refY='4'>
		            <line x1='0' y1='0' x2='0' y2='4' stroke='blue'/>
	            </marker>
	            <marker id='tail' orient='auto' markerWidth='4' markerHeight='8' refX='4' refY='4'>
		            <path d='M0,0 V8 L4,4 Z' fill='blue' />
	            </marker>
                <line id='facing-East'  stroke='white' x1='0' y1='0' x2='20'  y2='0'    />
                <line id='facing-South' stroke='white' x1='0' y1='0' x2='0'    y2='20' />
                <line id='facing-West'  stroke='white' x1='0' y1='0' x2='-20' y2='0'    />
                <line id='facing-North' stroke='white' x1='0' y1='0' x2='0'    y2='-20'  />
            </defs>
            ");
            output.WriteLine("<line x1='{0}' y1='{1}' x2='{2}' y2='{3}' style='stroke:rgb(0,0,255);stroke-width:1'/>>", minX, minY, maxX, maxY);
            output.WriteLine("<rect x='{0}' y='{1}' width='{2}' height='{3}' style='fill:rgb(0,0,0);stroke:rgb(0,0,255);stroke-width:1'/>>", minX, minY, maxX - minX, maxY - minY);
            output.WriteLine(@"
                <foreignObject x='{0}' y='{1}' width='150' height='300'>
	                <p style='text-color:black;background:white;border:solid 1px blue;' xmlns='http://www.w3.org/1999/xhtml'>
		                Display statics:<br />
		                <label><input type='checkbox' checked='checked' id='showwalls' /> Walls</label><br />
		                <label><input type='checkbox' checked='checked' id='showunwalls' /> Unwalls</label><br />
		                <label><input type='checkbox' checked='checked' id='showdecals' /> Decals</label><br />
		                <label><input type='checkbox' checked='checked' id='showsteps' /> Steps</label><br />
		                Display locations:<br />
		                <label><input type='checkbox' checked='checked' /> Human spawn</label><br />
		                <label><input type='checkbox' checked='checked' /> Robot spawn</label><br />
		                <label><input type='checkbox' checked='checked' /> Flags</label><br />
		                <label><input type='checkbox' checked='checked' /> Pods</label><br />
		                <label><input type='checkbox' checked='checked' /> Beacons</label><br />
	                </p>
                </foreignObject>", minX - 150, minY + 1);
            int staticIndex = 0;
            foreach (var merlinStatic in MazeArchive.Geometry)
            {
                string cssClass = merlinStatic.unknown20 == 0 ? "unwall" :
                    merlinStatic.unknown18 == 1 || merlinStatic.unknown19 == 1 ? "decal" :
                            string.IsNullOrWhiteSpace(merlinStatic.unknown08) != string.IsNullOrWhiteSpace(merlinStatic.unknown09) ? "step" :
                                "wall";
                output.WriteLine("<line x1='{0}' y1='{1}' x2='{2}' y2='{3}' class='{11}' id='static-{4:x4}' title='{5,8}\t{6,8}\t{7,8}\t{8,8}\t{9,8}\t{10,8}'>",
                    merlinStatic.X1 * SVG_SCALE_FACTOR, merlinStatic.Y1 * SVG_SCALE_FACTOR, merlinStatic.X2 * SVG_SCALE_FACTOR, merlinStatic.Y2 * SVG_SCALE_FACTOR,
                    staticIndex++, merlinStatic.unknown08, merlinStatic.unknown09, merlinStatic.unknown10, merlinStatic.unknown11, merlinStatic.unknown12, merlinStatic.unknown13,
                    cssClass);
                output.WriteLine("<set attributeName='stroke' to='pink' begin='showwalls.click' />");
                output.WriteLine("</line>");
            }
            staticIndex = 0;
            foreach (var merlinStatic in MazeArchive.Geometry)
            {
                output.WriteLine("<!-- Static strings {6:x4}: {0,8}\t{1,8}\t{2,8}\t{3,8}\t{4,8}\t{5,8} -->", merlinStatic.unknown08, merlinStatic.unknown09, merlinStatic.unknown10, merlinStatic.unknown11, merlinStatic.unknown12, merlinStatic.unknown13, staticIndex++);
            }
            staticIndex = 0;
            foreach (var merlinStatic in MazeArchive.Geometry)
            {
                output.WriteLine("<!-- Static numerics {0:x4}: {1:x4}\t{2:x4}\t{3:x4}\t{4:x4}\t{5:x2}\t{6:x2}\t{7:x2}\t{8:x4}\t{9:x2}\t{10:x4}\t{11:x4}\t -->", staticIndex++,
                    merlinStatic.BottomZ, merlinStatic.TopZ, merlinStatic.unknown16, merlinStatic.unknown17, merlinStatic.unknown18,
                    merlinStatic.unknown19, merlinStatic.unknown20, merlinStatic.unknown21, merlinStatic.unknown22, merlinStatic.unknown23,
                    merlinStatic.unknown24);
            }

            foreach (var location in MazeArchive.Locations)
            {
                output.WriteLine("<g class='location-{0}'>", location.Name.Split(new char[] { '_' })[0]);
                output.WriteLine("\t<use xlink:href='#facing-{0}' x='{1}' y='{2}' />", location.FacingDirection.ToString(), location.X / 10, location.Y / 10); 
                output.WriteLine("\t<circle r='{3}' cx='{0}' cy='{1}' title='{2}' />", location.X / 10, location.Y / 10, location.Name, (location.Z / 10.0 - minZ) / (maxZ - minZ) * 10.0 + 8);
                output.WriteLine("\t<text x='{0}' y='{1}'>{2}</text>", location.X / 10 + 9, location.Y / 10, location.Name);
                output.WriteLine("</g>");
            }

            int bspIndex = 0;
            foreach (var bsp in MazeArchive.Bsp)
            {
                output.WriteLine("<!-- BSP {4:x4}: {0:x4}\t{1:x4}\t{2:x4}\t{3:x4} -->", bsp.unknown01, bsp.unknown02, bsp.unknown03, bsp.unknown04, bspIndex++);
            }

            output.WriteLine("</svg>");
            output.Close();
        }

        private const string NAMESPACE = "http://schema.philip-searle.me.uk/Merlin.Texture.1";

        private static void DumpTexturesToTxt(TexturePack texturePack, string directoryPath)
        {
            Console.WriteLine("Dumping textures to " + directoryPath);

            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                CloseOutput = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false
            };
            using (XmlWriter xml = XmlWriter.Create(directoryPath + "\\_textures.xml", xmlSettings))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("TexturePack", NAMESPACE);

                xml.WriteStartElement("Palette", NAMESPACE);
                for (int i = 0; i < TexturePack.PALETTE_ENTRIES; i++)
                {
                    xml.WriteStartElement("Entry", NAMESPACE);
                    xml.WriteAttributeString("Index", i.ToString());
                    xml.WriteAttributeString("Red", texturePack.Palette[i].R.ToString());
                    xml.WriteAttributeString("Green", texturePack.Palette[i].G.ToString());
                    xml.WriteAttributeString("Blue", texturePack.Palette[i].B.ToString());
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();

                xml.WriteStartElement("Textures", NAMESPACE);
                foreach (var texture in texturePack.Textures)
                {
                    xml.WriteStartElement("Texture", NAMESPACE);
                    xml.WriteAttributeString("Name", texture.Name);
                    xml.WriteAttributeString("HasTransparency", texture.HasTransparency.ToString());
                    foreach (var mipmap in texture.Mipmaps)
                    {
                        xml.WriteStartElement("MipMap", NAMESPACE);
                        xml.WriteAttributeString("Level", mipmap.Level.ToString());
                        xml.WriteAttributeString("Width", mipmap.ImageDimensions.Width.ToString());
                        xml.WriteAttributeString("Height", mipmap.ImageDimensions.Height.ToString());
                        xml.WriteAttributeString("Data", string.Format("{0}_MIPMAP_{1}.gif", texture.Name, mipmap.Level));
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
                xml.Close();

                /*foreach (var texture in texturePack.textures)
                {
                    output.WriteLine("\n{0,8}: NEW TEXTURE: transparent = {1}", texture.Name, texture.HasTransparency);
                    foreach (var mipmap in texture.Mipmaps)
                    {
                        output.Write("{0,8}: {1:x4}\t{2:x4}\t{3:x4}\t{4:x4}\t{5:x4}\t{6:x8}\n[\n",
                            texture.Name,
                            mipmap.ImageDimensions.Width, mipmap.ImageDimensions.Height, mipmap.NextLargestPowerOfTwo.Width, mipmap.NextLargestPowerOfTwo.Height, mipmap.Level, mipmap.ImageData.Length);
                        foreach (var unknown in mipmap.PixelSpans)
                        {
                            output.Write("\t[ ");
                            foreach (var unknown2 in unknown)
                            {
                                output.Write("{0:x4} {1:x4}, ", unknown2.StartIndex, unknown2.EndIndex);
                            }
                            output.WriteLine("]");
                        }
                        output.WriteLine("]");
                    }
                }
                output.Close();*/
            }

            foreach (var texture in texturePack.Textures)
            {
                Console.WriteLine("Skipping texture " + texture.Name + "; unknown1c != 0");
                DumpTexture(texturePack, texture, directoryPath);
            }
        }

        private static void DumpTexture(TexturePack texturePack, CMerlinTexture texture, string directoryPath)
        {
            int mipmapIndex = 0;
            foreach (var mipmap in texture.Mipmaps)
            {
                Console.WriteLine("Dumping decal mipmap " + mipmapIndex);

                using (Bitmap bitmap = new Bitmap(mipmap.NextLargestPowerOfTwo.Width, mipmap.NextLargestPowerOfTwo.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
                {
                    var palette = bitmap.Palette;
                    for (int i = 0; i < palette.Entries.Length; i++)
                    {
                        palette.Entries.SetValue(texturePack.Palette[i], i);
                    }
                    // Need to set the palette as we were given a clone of the original
                    bitmap.Palette = palette;

                    int imageDataPtr = 0;
                    for (int y = 0; y < mipmap.NextLargestPowerOfTwo.Height; y++)
                    {
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, y, mipmap.NextLargestPowerOfTwo.Width, 1), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        byte[] row = new byte[mipmap.NextLargestPowerOfTwo.Width];

                        foreach (var span in mipmap.PixelSpans[y])
                        {
                            for (int x = span.StartIndex >> mipmapIndex; x < (span.EndIndex+1) >> mipmapIndex; x++)
                            {
                                row[x] = mipmap.ImageData[imageDataPtr++];
                            }
                        }

                        Marshal.Copy(row, 0, bitmapData.Scan0, row.Length);
                        bitmap.UnlockBits(bitmapData);
                    }
                    //if (imageDataPtr != mipmap.ImageData.Length) throw new InvalidOperationException();

                    // Convert from column-major bottom-up Hover format to human-editable foramt
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                    bitmap.Save(directoryPath + "\\" + texture.Name + "_MIPMAP" + mipmapIndex + ".gif", ImageFormat.Gif);

                    mipmapIndex++;
                }
            }
        }
    }
}
