using SharpDX;
using SharpDX.Direct3D11;

namespace Rendering.Graphics
{
    public class SelectEntryParams
    {
        public int RefID { private set; get; }

        public SelectEntryParams(int InId)
        {
            RefID = InId;
        }

        public static SelectEntryParams Construct(int InId)
        {
            return new SelectEntryParams(InId);
        }
    }

    public abstract class IRenderer
    {
        protected BaseShader shader;
        protected bool isUpdatedNeeded;
        protected Buffer indexBuffer;
        protected Buffer vertexBuffer;

        public bool DoRender { get; set; }
        public Matrix Transform { get; protected set; }
        public BoundingBox BoundingBox { get; protected set; }

        public abstract void Select(SelectEntryParams SelectParams);
        public abstract void Unselect();
        public abstract void InitBuffers(Device d3d, DeviceContext deviceContext);
        public abstract void SetTransform(Matrix matrix);
        public abstract void UpdateBuffers(Device device, DeviceContext deviceContext);
        public abstract void Render(Device device, DeviceContext deviceContext, Camera camera);
        public abstract void Shutdown();

    }
}
