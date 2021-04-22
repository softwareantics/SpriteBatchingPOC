namespace SpriteBatchingPOC
{
    using OpenTK.Graphics.OpenGL4;
    using OpenTK.Mathematics;
    using OpenTK.Windowing.Common;
    using OpenTK.Windowing.Desktop;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using System;
    using System.IO;
    using Color = System.Drawing.Color;
    using Vector2 = System.Numerics.Vector2;

    internal sealed class Program : GameWindow
    {
        private readonly float speed = 4;

        private int fragmentShader;

        private int program;

        private SpriteBatch spriteBatch;

        private int texA;

        private int texB;

        private int texC;

        private int texD;

        private int vao;

        private int vertexShader;

        private float x = 0;

        private float y = 0;

        public Program()
                                    : base(
                  GameWindowSettings.Default,
                  new NativeWindowSettings()
                  {
                      API = ContextAPI.OpenGL,
                      APIVersion = new Version(4, 6),
                      AutoLoadBindings = true,
                      Flags = ContextFlags.ForwardCompatible,
                      Profile = ContextProfile.Core,
                      WindowBorder = WindowBorder.Fixed,
                      Title = "Sprite Batching POC",
                      StartVisible = true,
                      Size = new Vector2i(1280, 720),
                  })
        {
        }

        protected override void OnLoad()
        {
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText("shader.vert"));
            GL.CompileShader(vertexShader);

            Console.WriteLine(GL.GetShaderInfoLog(vertexShader));

            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText("shader.frag"));
            GL.CompileShader(fragmentShader);

            Console.WriteLine(GL.GetShaderInfoLog(fragmentShader));

            program = GL.CreateProgram();

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.LinkProgram(program);
            GL.ValidateProgram(program);

            Console.WriteLine(GL.GetProgramInfoLog(program));

            GL.UseProgram(program);

            for (int i = 0; i < 32; i++)
            {
                GL.Uniform1(GL.GetUniformLocation(program, $"u_textures[{i}]"), i);
            }

            texA = LoadTextureFromFile("default.png");
            texB = LoadTextureFromFile("wood.png");
            texC = LoadTextureFromFile("jedi.jpg");
            texD = LoadTextureFromFile("cheese.jpg");

            spriteBatch = new SpriteBatch(program);

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.ClearColor(System.Drawing.Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
            {
                y -= speed;
            }
            else if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
            {
                y += speed;
            }
            else if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
            {
                x += speed;
            }
            else if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
            {
                x -= speed;
            }

            Matrix4 projection = Matrix4.CreateOrthographic(ClientSize.X, ClientSize.Y, -1, 1);
            GL.UniformMatrix4(GL.GetUniformLocation(program, "u_projection"), false, ref projection);

            Matrix4 view = Matrix4.CreateTranslation(x, y, 0);
            GL.UniformMatrix4(GL.GetUniformLocation(program, "u_view"), false, ref view);

            // Resets the vertices and textures that have been batched.
            spriteBatch.Begin();

            // Draw the same sprite ten times, to reach the max batch count. If you change the iterator to a value lower than 10 the two sprites draw correctly.
            for (int i = 0; i < 10; i++)
            {
                spriteBatch.Batch(texA, Color.White, Vector2.Zero, Vector2.Zero, 0, new Vector2(256, 256));
            }

            // Here is the problem, 'texB' texture is not drawn here, the 'texA' texture is drawn :(
            spriteBatch.Batch(texB, Color.White, Vector2.Zero, new Vector2(256, 0), 0, new Vector2(256, 256));

            //spriteBatch.Batch(texC, Color.White, Vector2.Zero, new Vector2(512, 0), 0, new Vector2(256, 256));
            //spriteBatch.Batch(texD, Color.White, Vector2.Zero, new Vector2(768, 0), 0, new Vector2(256, 256));

            // Updates the vertex buffer and draws the contents to the screen.
            spriteBatch.End();

            SwapBuffers();

            base.OnRenderFrame(args);
        }

        private static void Main()
        {
            new Program().Run();
        }

        private int LoadTextureFromFile(string filePath)
        {
            FileStream? stream = File.OpenRead(filePath);

            using (Image<Rgba32> image = Image.Load<Rgba32>(stream))
            {
                int width = image.Width;
                int height = image.Height;

                int[] data = new int[width * height];

                // Just simple bit manipulation to convert RGBA to ABGR.
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Rgba32 color = image[x, y];
                        data[(y * width) + x] = (color.A << 24) | (color.B << 16) | (color.G << 8) | (color.R << 0);
                    }
                }

                GL.CreateTextures(TextureTarget.Texture2D, 1, out int result);

                GL.TextureStorage2D(result, 1, SizedInternalFormat.Rgba8, width, height);
                GL.TextureParameter(result, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TextureParameter(result, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TextureParameter(result, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TextureParameter(result, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.TextureSubImage2D(result, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

                return result;
            }
        }
    }
}