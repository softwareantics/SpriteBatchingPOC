namespace SpriteBatchingPOC
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public static readonly int SizeInBytes = Marshal.SizeOf<Vertex>();

        public Vertex(Vector2 position, Vector4 color, Vector2 textureCoordinate, float textureIdentifier)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
            TextureIdentifier = textureIdentifier;
        }

        public Vector2 Position { get; set; }

        public Vector4 Color { get; set; }

        public Vector2 TextureCoordinate { get; set; }

        public float TextureIdentifier { get; set; }
    }
}