namespace SpriteBatchingPOC
{
    using OpenTK.Graphics.OpenGL4;
    using OpenTK.Mathematics;
    using OpenTK.Windowing.Common;
    using OpenTK.Windowing.Desktop;
    using System;
    using System.Drawing;
    using System.IO;
    using Vector2 = System.Numerics.Vector2;

    internal sealed class Program : GameWindow
    {
        private int fragmentShader;

        private int program;

        private SpriteBatch spriteBatch;

        private int vao;

        private int vertexShader;

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

            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText("shader.frag"));
            GL.CompileShader(fragmentShader);

            program = GL.CreateProgram();

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.LinkProgram(program);
            GL.ValidateProgram(program);

            GL.UseProgram(program);

            spriteBatch = new SpriteBatch();

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Matrix4 projection = Matrix4.CreateOrthographic(ClientSize.X, ClientSize.Y, -1, 1);
            GL.UniformMatrix4(GL.GetUniformLocation(program, "u_projection"), false, ref projection);

            Matrix4 view = Matrix4.CreateTranslation(Vector3.Zero);
            GL.UniformMatrix4(GL.GetUniformLocation(program, "u_view"), false, ref view);

            spriteBatch.Begin();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    spriteBatch.Batch(0, Color.Red, new Vector2(0, 0), new Vector2(i * 64, j * 64), 0, new Vector2(32, 32));
                }
            }

            spriteBatch.End();

            SwapBuffers();

            base.OnRenderFrame(args);
        }

        private static void Main()
        {
            new Program().Run();
        }
    }
}