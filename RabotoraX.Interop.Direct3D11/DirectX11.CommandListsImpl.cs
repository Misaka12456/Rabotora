using System;
using System.Diagnostics;
using JetBrains.Annotations;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Threading;
using RabotoraX.Core.Utility;
using RabotoraX.Interop.Direct3D11.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using CullMode = RabotoraX.Core.Graphics.CullMode;

namespace RabotoraX.Interop.Direct3D11;

public partial class DirectX11
{
	public INativeCommandList CreateCommandList()
	{
		return new D3D11CommandList(this);
	}

	public void Submit(INativeCommandList commandList)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(Submit));

		if (commandList is not D3D11CommandList dxCmdList)
		{
			throw new ArgumentException("[DirectX11] CommandList backend mismatch.", nameof(commandList));
		}

		lock (RenderLock)
		{
			dxCmdList.Execute();
		}
	}

	internal void ApplySetRenderTarget(INativeRenderTexture? renderTarget)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetRenderTarget));

		_currentRenderTarget = renderTarget;

		if (renderTarget == null)
		{
			_context!.OMSetRenderTargets(MainRenderTexture!.RTV, _depthStencilView);
		}
		else if (renderTarget is D3D11RenderTexture dxRT)
		{
			_context!.OMSetRenderTargets(dxRT.RTV, _depthStencilView);
		}
		else
		{
			throw new ArgumentException("Unsupported render target type for DX11 backend.");
		}
	}
	
	internal void ApplyClear(float r, float g, float b, float a)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplyClear));
		var target = _currentRenderTarget switch
		{
			null => MainRenderTexture,
			D3D11RenderTexture dxRT => dxRT,
			_ => throw new ArgumentException("Unsupported render target type for DX11 backend.")
		};
		
		_context!.ClearRenderTargetView(target!.RTV, new Color4(r, g, b, a));
		_context!.ClearDepthStencilView(_depthStencilView!, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
	}

	internal void ApplySetViewport(float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetViewport));
		_context!.RSSetViewport(x, y, width, height, minDepth, maxDepth);
	}

	internal void ApplySetShader(INativeShader shader)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetShader));
		switch (shader)
		{
			case DX11Shader {Type: ShaderType.VertexShader} dxShader:
			{
				_context!.VSSetShader((ID3D11VertexShader)dxShader.NativeShader);
				if (dxShader.InputLayout != null)
				{
					_context!.IASetInputLayout(dxShader.InputLayout);
				}

				break;
			}
			case DX11Shader {Type: ShaderType.FragmentShader} dxShader:
			{
				_context!.PSSetShader((ID3D11PixelShader)dxShader.NativeShader);
				break;
			}
			case DX11ShaderProgram {Type: ShaderType.VertexFragment} program:
			{
				_context!.VSSetShader(program.VertexShader);
				_context!.PSSetShader(program.FragmentShader);
				if (program.InputLayout != null)
				{
					_context!.IASetInputLayout(program.InputLayout);
				}
				break;
			}
		}
	}

	internal void ApplySetVertexBuffer(IGpuBuffer buffer, int stride, int offset = 0)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetVertexBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			_context!.IASetVertexBuffer(0, dxBuffer.NativeBuffer, (uint)stride, (uint)offset); // Slot 0
		}
	}

	internal void ApplySetIndexBuffer(IGpuBuffer buffer)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetIndexBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			_context!.IASetIndexBuffer(dxBuffer.NativeBuffer, Format.R32_UInt, 0);
		}
	}

	internal void ApplySetConstantBuffer(int slot, IGpuBuffer buffer, ShaderType stage)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetConstantBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			if (stage == ShaderType.VertexShader)
			{
				_context!.VSSetConstantBuffer((uint)slot, dxBuffer.NativeBuffer);
			}
			else if (stage == ShaderType.FragmentShader)
			{
				_context!.PSSetConstantBuffer((uint)slot, dxBuffer.NativeBuffer);
			}
		}
	}
	
	[MustDisposeResource]
	internal IDisposable ApplySetCullMode(CullMode mode)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetCullMode));
		var lastCullMode = _cullModeStack.Peek();
		if (lastCullMode == mode) return new AutoScope(); // No change needed
		
		Debug.Assert(_cullModeStack.Count < 32, "History Cull Modes Count >= 32: too many nested SetCullMode calls without ResumeCullMode. Has any code logic causing circular Cull Mode changes?");
		
		_cullModeStack.Push(mode);
		var state = mode switch
		{
			CullMode.Back => _rasterizerStateCullBack,
			CullMode.Front => _rasterizerStateCullFront,
			CullMode.None => _rasterizerStateCullNone,
			_ => _rasterizerStateCullBack
		};
		
		_context!.RSSetState(state);
		
		return new AutoScope(onExit: ResumeCullMode);
	}

	internal void ResumeCullMode()
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ResumeCullMode));
		if (_cullModeStack.Count <= 1) return;
		_cullModeStack.Pop(); // ignore the popped value because we will peek the new current mode
		var currentMode = _cullModeStack.Peek();
		var state = currentMode switch
		{
			CullMode.Back => _rasterizerStateCullBack,
			CullMode.Front => _rasterizerStateCullFront,
			CullMode.None => _rasterizerStateCullNone,
			_ => _rasterizerStateCullBack
		};
			
		_context!.RSSetState(state);
	}

	[MustDisposeResource]
	internal IDisposable ApplySetBlendState(BlendState state)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetBlendState));
		
		var lastState = _blendStateStack.Peek();
		if (lastState.Equals(state)) return new AutoScope(); // No change needed
		
		Debug.Assert(_blendStateStack.Count < 32, "History Blend States Count >= 32: too many nested SetBlendState calls without ResumeBlendState. Has any code logic causing circular Blend State changes?");
		
		_blendStateStack.Push(state);
		ApplyBlendState(state);
		
		return new AutoScope(onExit: ResumeBlendState);
	}

	internal void ResumeBlendState()
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ResumeBlendState));
		if (_blendStateStack.Count <= 1) return;
		_blendStateStack.Pop(); // ignore the popped value because we will peek the new current state
		ApplyBlendState(_blendStateStack.Peek());
	}

	private void ApplyBlendState(BlendState state)
	{
		if (!_blendStateCache.TryGetValue(state, out var nativeState))
		{
			var desc = new BlendDescription()
			{
				AlphaToCoverageEnable = false,
				IndependentBlendEnable = false
			};

			desc.RenderTarget[0] = new RenderTargetBlendDescription()
			{
				BlendEnable = state.EnableBlending,
				SourceBlend = MapBlend(state.SrcColor),
				DestinationBlend = MapBlend(state.DstColor),
				BlendOperation = MapOp(state.ColorOp),
				SourceBlendAlpha = MapBlend(state.SrcAlpha),
				DestinationBlendAlpha = MapBlend(state.DstAlpha),
				BlendOperationAlpha = MapOp(state.AlphaOp),
				RenderTargetWriteMask = ColorWriteEnable.All
			};
			
			nativeState = _device!.CreateBlendState(desc);
			_blendStateCache[state] = nativeState;
		}

		_context!.OMSetBlendState(nativeState);
	}

	internal void ApplySetDepthEnabled(bool enabled, bool writeEnabled = true)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplySetDepthEnabled));
		if (!enabled)
		{
			_context!.OMSetDepthStencilState(_depthStateNone);
		}
		else
		{
			_context!.OMSetDepthStencilState(writeEnabled ? _depthStateDefault : _depthStateReadOnly);
		}
	}

	internal void ApplyDraw(int vertexCount, int startVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplyDraw));
		_context!.IASetPrimitiveTopology(ToDxTopology(topology));
		_context!.Draw((uint)vertexCount, (uint)startVertexLocation);
	}

	internal void ApplyDrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ApplyDrawIndexed));
		_context!.IASetPrimitiveTopology(ToDxTopology(topology));
		_context!.DrawIndexed((uint)indexCount, (uint)startIndexLocation, baseVertexLocation);
	}
}