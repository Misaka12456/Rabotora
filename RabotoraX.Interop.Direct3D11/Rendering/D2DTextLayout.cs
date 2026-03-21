using System;
using System.Numerics;
using RabotoraX.Core.UI;
using Vortice.DirectWrite;

namespace RabotoraX.Interop.Direct3D11.Rendering;

public sealed class D2DTextLayout : INativeTextLayout
{
	public IDWriteTextLayout InternalLayout => _layout;
	public Vector2 Size => new(_layout.Metrics.Width, _layout.Metrics.Height);
	private readonly IDWriteTextLayout _layout;
	
	public D2DTextLayout(IDWriteTextLayout layout)
	{
		_layout = layout;
	}

	public void Dispose()
	{
		_layout.Dispose();
		GC.SuppressFinalize(this);
	}
}