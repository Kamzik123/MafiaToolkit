using Buffer = SharpDX.Direct3D11.Buffer;
using Utils.Extensions;
using SharpDX.Direct3D11;

namespace Rendering.Graphics.Utils
{
    public class RenderUtils
    {
        public static Buffer ConstructIndexBuffer(Device D3D, ulong Hash, uint[] Indices)
        {
            if(RenderStorageSingleton.Instance.IndexBuffers.ContainsKey(Hash))
            {
                // IndexBuffer already exists
                return RenderStorageSingleton.Instance.IndexBuffers[Hash];
            }

            Buffer NewIndexBuffer = Buffer.Create(D3D, BindFlags.IndexBuffer, Indices);
            RenderStorageSingleton.Instance.IndexBuffers.TryAdd(Hash, NewIndexBuffer);
            return NewIndexBuffer;
        }

        public static Buffer ConstructVertexBuffer<T>(Device D3D, ulong Hash, T[] Vertices) where T : struct
        {
            if (RenderStorageSingleton.Instance.VertexBuffers.ContainsKey(Hash))
            {
                // VertexBuffer already exists
                return RenderStorageSingleton.Instance.VertexBuffers[Hash];
            }

            Buffer NewVertexBuffer = Buffer.Create(D3D, BindFlags.VertexBuffer, Vertices);
            RenderStorageSingleton.Instance.VertexBuffers.TryAdd(Hash, NewVertexBuffer);
            return NewVertexBuffer;
        }
    }
}
