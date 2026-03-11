using Vortice.Direct3D11;
using CullMode = RabotoraX.Core.Graphics.CullMode;

namespace RabotoraX.Interop.Direct3D11.Utility;

public static class DXStateExtension
{
	extension(RasterizerDescription desc)
	{
		public CullMode ToManagedCullMode()
		{
			return desc.CullMode switch
			{
				Vortice.Direct3D11.CullMode.None => CullMode.None,
				Vortice.Direct3D11.CullMode.Front => CullMode.Front,
				Vortice.Direct3D11.CullMode.Back => CullMode.Back,
				_ => throw new ArgumentOutOfRangeException(nameof(desc.CullMode), "Invalid cull mode.")
			};
		}

		public Vortice.Direct3D11.CullMode ToNativeCullMode()
		{
			return desc.CullMode;
		}
	}
	
	extension(ID3D11RasterizerState rasterizerState)
	{
		public CullMode GetManagedCullMode() => rasterizerState.Description.ToManagedCullMode();
		public Vortice.Direct3D11.CullMode GetNativeCullMode() => rasterizerState.Description.ToNativeCullMode();
	}
}