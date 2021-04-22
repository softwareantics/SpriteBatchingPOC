namespace SpriteBatchingPOC
{
    using OpenTK.Graphics.OpenGL4;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Numerics;

    public class SpriteBatch
    {
        private readonly int ebo;

        private readonly int vbo;

        private readonly IList<Vertex> vertices;

        public SpriteBatch(int maxCapacity = 1000)
        {
            if (maxCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCapacity), $"The specified {nameof(maxCapacity)} parameter must be greater than zero.");
            }

            MaxVertexCount = maxCapacity * 4;
            MaxIndexCount = maxCapacity * 6;

            vertices = new List<Vertex>();

            // Position
            GL.VertexAttribFormat(0, 2, VertexAttribType.Float, false, 0);
            GL.VertexAttribBinding(0, 0);
            GL.EnableVertexAttribArray(0);

            // Color
            GL.VertexAttribFormat(1, 4, VertexAttribType.Float, false, 2 * sizeof(float));
            GL.VertexAttribBinding(1, 0);
            GL.EnableVertexAttribArray(1);

            // Texture coordinate
            GL.VertexAttribFormat(2, 2, VertexAttribType.Float, false, 6 * sizeof(float));
            GL.VertexAttribBinding(2, 0);
            GL.EnableVertexAttribArray(2);

            // Texture identifier
            GL.VertexAttribFormat(3, 1, VertexAttribType.Float, false, 8 * sizeof(float));
            GL.VertexAttribBinding(3, 0);
            GL.EnableVertexAttribArray(3);

            GL.CreateBuffers(1, out vbo);
            GL.CreateBuffers(1, out ebo);

            // Empty VBO of vertices
            GL.NamedBufferData(vbo, MaxVertexCount * Vertex.SizeInBytes, Array.Empty<Vertex>(), BufferUsageHint.DynamicDraw);

            int[] indices = new int[MaxIndexCount];

            int offset = 0;

            for (int i = 0; i < MaxIndexCount; i += 6)
            {
                indices[i] = offset;
                indices[i + 1] = 1 + offset;
                indices[i + 2] = 2 + offset;

                indices[i + 3] = 2 + offset;
                indices[i + 4] = 3 + offset;
                indices[i + 5] = 0 + offset;

                offset += 4;
            }

            // Fill indices buffer
            GL.NamedBufferData(ebo, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);
        }

        public int MaxIndexCount { get; }

        public int MaxVertexCount { get; }

        public void Batch(int textureID, Color color, Vector2 origin, Vector2 position, float rotation, Vector2 scale)
        {
            if (vertices.Count >= MaxVertexCount)
            {
                End();
                Begin();
            }

            float x = position.X;
            float y = position.Y;

            float dx = -origin.X;
            float dy = -origin.Y;

            float w = scale.X;
            float h = scale.Y;

            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);

            Vector4 vecColor = new Vector4(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f);

            // Top right
            vertices.Add(new Vertex()
            {
                Position = new Vector2(x + ((dx + w) * cos) - (dy * sin), y + ((dx + w) * sin) + (dy * cos)),
                Color = vecColor,
                TextureCoordinate = new Vector2(1, 1),
                TextureIdentifier = textureID,
            });

            // Top left
            vertices.Add(new Vertex()
            {
                Position = new Vector2(x + (dx * cos) - (dy * sin), y + (dx * sin) + (dy * cos)),
                Color = vecColor,
                TextureCoordinate = new Vector2(0, 1),
                TextureIdentifier = textureID,
            });

            // Bottom left
            vertices.Add(new Vertex()
            {
                Position = new Vector2(x + (dx * cos) - ((dy + h) * sin), y + (dx * sin) + ((dy + h) * cos)),
                Color = vecColor,
                TextureCoordinate = new Vector2(0, 0),
                TextureIdentifier = textureID,
            });

            // Bottom right
            vertices.Add(new Vertex()
            {
                Position = new Vector2(x + ((dx + w) * cos) - ((dy + h) * sin), y + ((dx + w) * sin) + ((dy + h) * cos)),
                Color = vecColor,
                TextureCoordinate = new Vector2(1, 0),
                TextureIdentifier = textureID,
            });
        }

        public void Begin()
        {
            vertices.Clear();
        }

        public void End()
        {
            GL.NamedBufferSubData(vbo, IntPtr.Zero, vertices.Count * Vertex.SizeInBytes, vertices.ToArray());
            GL.BindVertexBuffer(0, vbo, IntPtr.Zero, Vertex.SizeInBytes);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.DrawElements(PrimitiveType.Triangles, MaxIndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}