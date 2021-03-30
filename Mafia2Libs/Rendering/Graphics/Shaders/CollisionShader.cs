using SharpDX.Direct3D11;

namespace Rendering.Graphics
{
    public class CollisionShader : BaseShader
    {
        public CollisionShader(Device Dx11Device, ShaderInitParams InitParams) : base(Dx11Device, InitParams) { }
    }
}