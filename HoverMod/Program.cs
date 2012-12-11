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
        static string processName;
        MfcClassRegistry ClassRegistry = new MfcClassRegistry();

        bool ShowHelp = false;
        string RequestedAction = "";
        string MazeFile = null;
        string TexturePackFile = null;
        string SvgFile = null;
        string OutputDirectory = null;
        string XmlFile = null;

        static void Main(string[] args)
        {
            processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            try
            {
                new Program(args).Run();
            }
            catch (OptionException ex)
            {
                Console.Write(processName);
                Console.Write(": ");
                Console.Write(ex.Message);
                Console.Write(": while processing option '");
                Console.Write(ex.OptionName);
                Console.WriteLine(string.Format("'\nTry `{0} --help' for more information.", processName));
                return;
            }
        }

        private Program(string[] args)
        {
            ClassRegistry.AutoRegisterClasses(typeof(Maze).Assembly);

            var options = new OptionSet()
            {
                {   "h|H|?|help", "show this message and exit",
                    value => ShowHelp = value != null },
                {   "a|action=", "specify the action to take.",
                    value => RequestedAction = value },
                {   "t|texturepack=", "specify a texture pack (.TEX) file to load/save",
                    value => TexturePackFile = value },
                {   "m|maze=", "specify a maze (.MAZ) file to load/save",
                    value => MazeFile = value },
                {   "s|svg=", "specify an SVG file to save",
                    value => SvgFile = value },
                {   "d|dir|directory=", "specify a directory to load/save to",
                    value => OutputDirectory = value },
                {   "x|xml=", "specify an XML file to load",
                    value => XmlFile = value }
            };

            options.Parse(args);

            if (ShowHelp || string.IsNullOrWhiteSpace(RequestedAction))
            {
                Console.WriteLine(string.Format("Usage: {0} [OPTIONS]+", processName));
                Console.WriteLine("Load, manipulate, and resave a Hover archive file.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                Console.WriteLine("\nPossible actions are:");
                Console.WriteLine("  svg    \tOutput an SVG image of the specified maze");
                Console.WriteLine("  extract\tExtract all textures from a .TEX file");
                Console.WriteLine("  combine\tCreate a new .TEX file using the specified XML file as input");
                Console.WriteLine("  compile\tCompile a new .MAZ file using the specified SVG file as input");
                return;
            }
        }

        private T LoadHoverFile<T>(string filename) where T : MfcObject
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                return new MfcDeserialiser(stream, ClassRegistry).DeserialiseObjectNoHeader<T>();
            }
        }

        private void SaveHoverFile<T>(string filename, T data) where T : MfcObject
        {
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                new MfcSerialiser(stream, ClassRegistry).SerialiseObjectNoHeader(data);
            }
        }

        private void RequireParameter(string parameterValue, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                throw new OptionException("Action '" + RequestedAction + "' requires a non-empty value", parameterName);
            }
        }
        
        private void Run()
        {
            if ("svg".Equals(RequestedAction, StringComparison.CurrentCultureIgnoreCase))
            {
                RequireParameter(MazeFile, "maze file");
                RequireParameter(SvgFile, "SVG file");
                using (StreamWriter svgStream = new StreamWriter(SvgFile, false, Encoding.UTF8))
                {
                    DumpLevelToSvg(LoadHoverFile<Maze>(MazeFile), svgStream);
                }
            }
            if ("extract".Equals(RequestedAction, StringComparison.CurrentCultureIgnoreCase))
            {
                RequireParameter(TexturePackFile, "texture pack file");
                RequireParameter(OutputDirectory, "output directory");
                DumpTextures(LoadHoverFile<TexturePack>(TexturePackFile), OutputDirectory);
            }
            if ("combine".Equals(RequestedAction, StringComparison.CurrentCultureIgnoreCase))
            {
                RequireParameter(TexturePackFile, "texture pack file");
                RequireParameter(XmlFile, "XML file");
                CombineTexturesToFile(XmlFile, TexturePackFile);
            }
            if ("compile".Equals(RequestedAction, StringComparison.CurrentCultureIgnoreCase))
            {
                CompileMazeToFile(@"C:\Users\Philip\Desktop\HoverGame\HOVER\MAZES\SMALL.MAZ", MazeFile);
            }
            /*
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

        private void DumpLevelToSvg(Maze maze, StreamWriter output)
        {
            const double SVG_SCALE_FACTOR = 0.05;

            var x1 = from m in maze.Geometry select m.X1;
            var x2 = x1.Union(from m in maze.Geometry select m.X2);
            var y1 = from m in maze.Geometry select m.Y1;
            var y2 = y1.Union(from m in maze.Geometry select m.Y2);
            var z1 = from l in maze.Locations select l.Z;
            var z2 = z1.Union(from m in maze.Geometry select m.BottomZ);
            var z3 = z2.Union(from m in maze.Geometry select m.TopZ);

            var minX = x2.Min() * SVG_SCALE_FACTOR;
            var maxX = x2.Max() * SVG_SCALE_FACTOR;
            var minY = y2.Min() * SVG_SCALE_FACTOR;
            var maxY = y2.Max() * SVG_SCALE_FACTOR;
            var minZ = z3.Min() * SVG_SCALE_FACTOR;
            var maxZ = z3.Max() * SVG_SCALE_FACTOR;

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
            foreach (var merlinStatic in maze.Geometry)
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
            foreach (var merlinStatic in maze.Geometry)
            {
                output.WriteLine("<!-- Static strings {6:x4}: {0,8}\t{1,8}\t{2,8}\t{3,8}\t{4,8}\t{5,8} -->", merlinStatic.unknown08, merlinStatic.unknown09, merlinStatic.unknown10, merlinStatic.unknown11, merlinStatic.unknown12, merlinStatic.unknown13, staticIndex++);
            }
            staticIndex = 0;
            foreach (var merlinStatic in maze.Geometry)
            {
                output.WriteLine("<!-- Static numerics {0:x4}: {1:x4}\t{2:x4}\t{3:x4}\t{4:x4}\t{5:x2}\t{6:x2}\t{7:x2}\t{8:x4}\t{9:x2}\t{10:x4}\t{11:x4}\t -->", staticIndex++,
                    merlinStatic.BottomZ, merlinStatic.TopZ, merlinStatic.unknown16, merlinStatic.unknown17, merlinStatic.unknown18,
                    merlinStatic.unknown19, merlinStatic.unknown20, merlinStatic.unknown21, merlinStatic.unknown22, merlinStatic.unknown23,
                    merlinStatic.unknown24);
            }

            foreach (var location in maze.Locations)
            {
                output.WriteLine("<g class='location-{0}'>", location.Name.Split(new char[] { '_' })[0]);
                output.WriteLine("\t<use xlink:href='#facing-{0}' x='{1}' y='{2}' />", location.FacingDirection.ToString(), location.X * SVG_SCALE_FACTOR, location.Y * SVG_SCALE_FACTOR);
                output.WriteLine("\t<circle r='{3}' cx='{0}' cy='{1}' title='{2}' />", location.X * SVG_SCALE_FACTOR, location.Y * SVG_SCALE_FACTOR, location.Name, (location.Z * SVG_SCALE_FACTOR - minZ) / (maxZ - minZ) * SVG_SCALE_FACTOR + 8);
                output.WriteLine("\t<text x='{0}' y='{1}'>{2}</text>", location.X * SVG_SCALE_FACTOR + 9, location.Y * SVG_SCALE_FACTOR, location.Name);
                output.WriteLine("</g>");
            }

            int bspIndex = 0;
            foreach (var bsp in maze.Bsp)
            {
                output.WriteLine("<!-- BSP {4:x4}: {0:x4}\t{1:x4}\t{2:x4}\t{3:x4} -->", bsp.unknown01, bsp.unknown02, bsp.unknown03, bsp.unknown04, bspIndex++);
            }

            output.WriteLine("</svg>");
            output.Close();
        }

        private const string NAMESPACE = "http://schema.philip-searle.me.uk/Merlin.Texture.1";

        private void DumpTextures(TexturePack texturePack, String outputDirectory)
        {
            Console.WriteLine("Dumping textures to " + outputDirectory);

            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                CloseOutput = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false
            };
            using (XmlWriter xml = XmlWriter.Create(outputDirectory + "\\_textures.xml", xmlSettings))
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
            }

            foreach (var texture in texturePack.Textures)
            {
                DumpTexture(texturePack, texture, OutputDirectory);
            }
        }

        private void DumpTexture(TexturePack texturePack, CMerlinTexture texture, string directoryPath)
        {
            int mipmapIndex = 0;
            foreach (var mipmap in texture.Mipmaps)
            {
                Console.WriteLine("Dumping decal mipmap " + mipmapIndex);

                using (Bitmap bitmap = new Bitmap(mipmap.ImageDimensions.Width, mipmap.ImageDimensions.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
                {
                    var palette = bitmap.Palette;
                    for (int i = 0; i < palette.Entries.Length; i++)
                    {
                        palette.Entries.SetValue(texturePack.Palette[i], i);
                    }
                    // Need to set the palette as we were given a clone of the original
                    bitmap.Palette = palette;

                    int imageDataPtr = 0;
                    for (int y = 0; y < mipmap.ImageDimensions.Height; y++)
                    {
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, y, mipmap.ImageDimensions.Width, 1), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        byte[] row = new byte[mipmap.ImageDimensions.Width];

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
                    if (imageDataPtr != mipmap.ImageData.Length) throw new InvalidOperationException();

                    // Convert from column-major bottom-up Hover format to human-editable foramt
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                    bitmap.Save(directoryPath + "\\" + texture.Name + "_MIPMAP_" + mipmap.Level + ".gif", ImageFormat.Gif);

                    mipmapIndex++;
                }
            }
        }

        private void CombineTexturesToFile(string xmlFile, string texturePackFile)
        {
            Console.WriteLine("Creating {0} from XML file {1}:", texturePackFile, xmlFile);

            XmlDocument xmlDocument = new XmlDocument();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("t", NAMESPACE);
            xmlDocument.Load(xmlFile);

            Console.WriteLine("Processing palette...");
            TexturePack texturePack = new TexturePack();
            Color[] palette = new Color[TexturePack.PALETTE_ENTRIES];
            foreach (var paletteEntry in xmlDocument.SelectNodes("/t:TexturePack/t:Palette/t:Entry", xmlNamespaceManager).OfType<XmlNode>())
            {
                palette[int.Parse(paletteEntry.Attributes["Index"].Value)] = Color.FromArgb(
                    int.Parse(paletteEntry.Attributes["Red"].Value),
                    int.Parse(paletteEntry.Attributes["Green"].Value),
                    int.Parse(paletteEntry.Attributes["Blue"].Value));
            }
            texturePack.Palette = palette.ToArray();

            List<CMerlinTexture> textures = new List<CMerlinTexture>();
            foreach (var textureDef in xmlDocument.SelectNodes("/t:TexturePack/t:Textures/t:Texture", xmlNamespaceManager).OfType<XmlNode>())
            {
                string textureName = textureDef.Attributes["Name"].Value;
                Console.WriteLine("Processing texture {0}...", textureName);
                var mipmapDef = textureDef.SelectSingleNode("t:MipMap", xmlNamespaceManager);
                string bitmapPath = Path.Combine(Path.GetDirectoryName(xmlFile), mipmapDef.Attributes["Data"].Value);
                using (Bitmap bitmap = new Bitmap(bitmapPath))
                {
                    CMerlinTexture texture = new CMerlinTexture();
                    texture.Name = textureName;
                    texture.UpdateImage(
                        texturePack,
                        bool.Parse(textureDef.Attributes["HasTransparency"].Value),
                        bitmap);
                    textures.Add(texture);
                }
            }
            texturePack.Textures = textures;

            Console.WriteLine("Saving to {0}", texturePackFile);
            SaveHoverFile(texturePackFile, texturePack);
        }

        private void CompileMazeToFile(string inputFile, string outputFile)
        {
            Console.WriteLine("Compiling {0} to {1}:", inputFile, outputFile);
            var maze = LoadHoverFile<Maze>(inputFile);

            Console.WriteLine("Saving to {0}", outputFile);
            SaveHoverFile(outputFile, maze);
        }
    }
}
