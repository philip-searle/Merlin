
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using System.Linq;
using System.Windows.Forms;
using Merlin.DomainModel;
using System.Runtime.InteropServices;

namespace HoverRenderer
{
    [StructLayout(LayoutKind.Sequential)]
    struct ConstantBuffer
    {
        public Matrix worldMatrix;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;
        public Color4 colour;
    }

    class HoverForm : RenderForm
    {
        Device device;
        DeviceContext context;
        SwapChain swapChain;
        RenderTargetView renderTarget;
        DepthStencilView depthStencilView;
        DepthStencilState depthStencilState;
        RasterizerState rasteriserState;
        ShaderSignature inputSignature;
        VertexShader vertexShader;
        PixelShader pixelShader;
        DataStream vertices;
        InputLayout layout;
        Buffer vertexBuffer;
        Buffer constantBuffer;

        int triangleCount;
        FpsCamera camera;

        public HoverForm(Maze maze)
            : base("HoverRenderer")
        {
            this.ClientSize = new System.Drawing.Size(640, 480);

            var description = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = this.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, description, out device, out swapChain);

            // create a view of our render target, which is the backbuffer of the swap chain we just created
            using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
            {
                renderTarget = new RenderTargetView(device, resource);
            }

            // Create the depth buffer
            var depthBufferDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.D32_Float,
                Height = this.ClientSize.Height,
                Width = this.ClientSize.Width,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            };
            using (var depthBuffer = new Texture2D(device, depthBufferDescription))
            {
                depthStencilView = new DepthStencilView(device, depthBuffer);
                depthStencilState = DepthStencilState.FromDescription(device, new DepthStencilStateDescription {
                    IsDepthEnabled = true,
                    IsStencilEnabled = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.LessEqual
                });
            }

            // Setup wireframe mode
            rasteriserState = RasterizerState.FromDescription(device, new RasterizerStateDescription
            {
                CullMode = SlimDX.Direct3D11.CullMode.None,
                FillMode = SlimDX.Direct3D11.FillMode.Wireframe
            });

            // setting a viewport is required if you want to actually see anything
            context = device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, this.ClientSize.Width, this.ClientSize.Height);
            context.OutputMerger.SetTargets(depthStencilView, renderTarget);
            context.OutputMerger.DepthStencilState = depthStencilState;
            context.Rasterizer.State = rasteriserState;
            context.Rasterizer.SetViewports(viewport);

            // load and compile the vertex shader
            using (var bytecode = ShaderBytecode.CompileFromFile("shader.fx", "VShader", "vs_4_0", ShaderFlags.Debug, EffectFlags.None))
            {
                inputSignature = ShaderSignature.GetInputSignature(bytecode);
                vertexShader = new VertexShader(device, bytecode);
            }

            // load and compile the pixel shader
            using (var bytecode = ShaderBytecode.CompileFromFile("shader.fx", "PShader", "ps_4_0", ShaderFlags.Debug, EffectFlags.None))
                pixelShader = new PixelShader(device, bytecode);

            // create test vertex data, making sure to rewind the stream afterward
            vertices = CreateTriangleListFromMaze(maze);
            camera.Position = FindHumanStartPosition(maze);

            // create the vertex layout and buffer
            var elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("COLOR", 0, Format.R32G32B32_Float, 0)
            };
            layout = new InputLayout(device, inputSignature, elements);
            vertexBuffer = new Buffer(device, vertices, (int)vertices.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            // configure the Input Assembler portion of the pipeline with the vertex data
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 24, 0));

            // set the shaders
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            // crate the constant buffer
            constantBuffer = new Buffer(device, new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = Marshal.SizeOf(typeof(ConstantBuffer)),
                BindFlags = BindFlags.ConstantBuffer
            });

            // prevent DXGI handling of alt+enter, which doesn't work properly with Winforms
            using (var factory = swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(this.Handle, WindowAssociationFlags.IgnoreAltEnter);

            // handle alt+enter ourselves
            this.KeyDown += (o, e) =>
            {
                if (e.Alt && e.KeyCode == Keys.Enter)
                    swapChain.IsFullScreen = !swapChain.IsFullScreen;
            };

            // handle form size changes
            this.UserResized += (o, e) =>
            {
                renderTarget.Dispose();

                swapChain.ResizeBuffers(2, 0, 0, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
                using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
                    renderTarget = new RenderTargetView(device, resource);

                context.OutputMerger.SetTargets(renderTarget);
            };

            this.KeyDown += new KeyEventHandler(HoverForm_KeyDown);
        }

        void HoverForm_KeyDown(object sender, KeyEventArgs e)
        {
            const int SPEED = 100;
            switch (e.KeyCode)
            {
                case Keys.W:
                    camera.MoveForward(-SPEED);
                    break;
                case Keys.S:
                    camera.MoveForward(SPEED);
                    break;
                case Keys.A:
                    camera.MoveLeft(-SPEED);
                    break;
                case Keys.D:
                    camera.MoveLeft(SPEED);
                    break;
                case Keys.Q:
                    camera.MoveUp(-SPEED);
                    break;
                case Keys.E:
                    camera.MoveUp(SPEED);
                    break;
                case Keys.Left:
                    camera.Turn(1);
                    break;
                case Keys.Right:
                    camera.Turn(-1);
                    break;
                case Keys.PageDown:
                    camera.LookUp(-1);
                    break;
                case Keys.PageUp:
                    camera.LookUp(1);
                    break;
            }
        }

        private Vector3 FindHumanStartPosition(Maze maze)
        {
            //return new Vector3(0, 0, 0);
            var humanStart = maze.Locations.Single(l => l.Name.StartsWith("HUMAN_00"));
            return new Vector3(humanStart.X, humanStart.Y, humanStart.Z);
        }

        private DataStream CreateTriangleListFromMaze(Maze maze)
        {
            var geometry = maze.Geometry;
            triangleCount = geometry.Count * 2;
            //triangleCount += 1;

            // Each CMerlinStatic becomes two triangles
            DataStream vertices = new DataStream(triangleCount * 6 * Vector3.SizeInBytes, true, true);

            foreach (var merlinStatic in geometry)
            {
                vertices.Write(new Vector3(merlinStatic.X1, merlinStatic.Y1, merlinStatic.BottomZ));
                vertices.Write(new Vector3(1, 0, 0));
                vertices.Write(new Vector3(merlinStatic.X2, merlinStatic.Y2, merlinStatic.BottomZ));
                vertices.Write(new Vector3(0, 1, 0));
                vertices.Write(new Vector3(merlinStatic.X1, merlinStatic.Y1, merlinStatic.TopZ));
                vertices.Write(new Vector3(0, 0, 1));

                vertices.Write(new Vector3(merlinStatic.X1, merlinStatic.Y1, merlinStatic.TopZ));
                vertices.Write(new Vector3(1, 0, 0));
                vertices.Write(new Vector3(merlinStatic.X2, merlinStatic.Y2, merlinStatic.BottomZ));
                vertices.Write(new Vector3(0, 1, 0));
                vertices.Write(new Vector3(merlinStatic.X2, merlinStatic.Y2, merlinStatic.TopZ));
                vertices.Write(new Vector3(0, 0, 1));
            }

            // Add deug visualisation
            //vertices.Write(new Vector3(.0f, .1f, 5f));
            //vertices.Write(new Vector3( .1f, -.1f,  5f));
            //vertices.Write(new Vector3(-.1f, -.1f,  5f));

            // Reset stream so SlimDX can use it
            vertices.Position = 0;

            return vertices;
        }

        protected override void  Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // clean up all resources
            // anything we missed will show up in the debug output
            vertices.Close();
            constantBuffer.Dispose();
            vertexBuffer.Dispose();
            layout.Dispose();
            inputSignature.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            rasteriserState.Dispose();
            depthStencilState.Dispose();
            depthStencilView.Dispose();
            renderTarget.Dispose();
            swapChain.Dispose();
            device.Dispose();
        }

        public void RunFrame()
        {
            // clear the render target to a soothing blue
            context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            context.ClearRenderTargetView(renderTarget, new Color4(0.0f, 0.5f, 1.0f));

            var view = Matrix.LookAtLH(camera.Position, camera.LookAtPosition, camera.UpVector);
            var proj = Matrix.PerspectiveFovLH(DegToRad(45.0f), (float)this.ClientSize.Width / (float)this.ClientSize.Height, 0.1f, 100.0f);

            ConstantBuffer constantData = new ConstantBuffer {
                worldMatrix = Matrix.Identity,
                viewMatrix = Matrix.Transpose(view),
                projectionMatrix = proj,
                colour = new Color4(0.5f, 1.0f, 0.0f)
            };
            using (var data = new DataStream(Marshal.SizeOf(constantData), true, true))
            {
                data.Write(constantData);
                data.Position = 0;
                context.UpdateSubresource(new DataBox(0, 0, data), constantBuffer, 0);
            }

            context.VertexShader.SetConstantBuffer(constantBuffer, 0);
            context.PixelShader.SetConstantBuffer(constantBuffer, 0);

            // draw the triangle
            context.Draw(triangleCount * 3, 0);
            swapChain.Present(0, PresentFlags.None);
        }

        public static float DegToRad(float deg)
        {
            var fov = deg * (float)System.Math.PI / 180;
            return fov;
        }
    }
}
