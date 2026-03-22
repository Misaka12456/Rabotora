using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Utility;

namespace RabotoraX.Interop.Direct3D11;

[SupportedOSPlatform("windows")]
internal sealed class D3D11CommandList : INativeCommandList
{
	private const int MaxRenderPassDepth = 16;
	
	private readonly DirectX11 _api;
	private readonly List<Action> _commands = [];

	private readonly Stack<CullMode> _cullModeStack = new([CullMode.Back]);
	private readonly Stack<BlendState> _blendStateStack = new([BlendState.AlphaBlend]);
	private readonly Stack<INativeRenderTexture?> _renderTargetStack = new();
	
	private bool _begun, _ended;

	internal D3D11CommandList(DirectX11 api)
	{
		_api = api;
	}

	public void Begin()
	{
		if (_begun && !_ended)
		{
			throw new InvalidOperationException("[D3D11CommandList] CommandList is already recording.");
		}
		
		_commands.Clear();
		
		_cullModeStack.Clear();
		_cullModeStack.Push(CullMode.Back);
		
		_blendStateStack.Clear();
		_blendStateStack.Push(BlendState.AlphaBlend);
		
		_begun = true;
		_ended = false;
	}

	public void End()
	{
		if (!_begun)
		{
			throw new InvalidOperationException("[D3D11CommandList] Call Begin() to begin recording commands first.");
		}
		if (_ended)
		{
			throw new InvalidOperationException("[D3D11CommandList] CommandList has already ended recording.");
		}
		
		_ended = true;
	}

	private void Enqueue(Action action)
	{
		if (!_begun || _ended)
		{
			throw new InvalidOperationException("[D3D11CommandList] CommandList is not recording.");
		}
		_commands.Add(action);
	}

	// public void SetRenderTarget(INativeRenderTexture? renderTexture)
	// {
	// 	Enqueue(() => _api.ApplySetRenderTarget(renderTexture));
	// }

	public void BeginRenderPass(INativeRenderTexture? renderTexture)
	{
		Enqueue(() =>
		{
			var last = _renderTargetStack.Count > 0 ? _renderTargetStack.Peek() : null;
			_renderTargetStack.Push(renderTexture);
			
			Debug.Assert(_renderTargetStack.Count < MaxRenderPassDepth, $"Render Pass Depth >= {MaxRenderPassDepth}: too many nested BeginRenderPass calls without EndRenderPass. Has any code logic causing circular render pass calls?");

			if (last != renderTexture)
			{
				_api.ApplySetRenderTarget(renderTexture);
			}
		});
	}

	public void EndRenderPass()
	{
		Enqueue(() =>
		{
			if (_renderTargetStack.Count == 0)
			{
				throw new InvalidOperationException("[D3D11CommandList] Mismatched EndRenderPass.");
			}

			_renderTargetStack.Pop();
			var current = _renderTargetStack.Count > 0 ? _renderTargetStack.Peek() : null;
			if (current != null)
			{
				_api.ApplySetRenderTarget(current);
			}
		});
	}

	public void Clear(float r, float g, float b, float a)
	{
		Enqueue(() => _api.ApplyClear(r, g, b, a));
	}

	public void SetViewport(float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1)
	{
		Enqueue(() => _api.ApplySetViewport(x, y, width, height, minDepth, maxDepth));
	}

	public void SetShader(INativeShader shader)
	{
		Enqueue(() => _api.ApplySetShader(shader));
	}

	public void SetVertexBuffer(IGpuBuffer buffer, int stride, int offset = 0)
	{
		Enqueue(() => _api.ApplySetVertexBuffer(buffer, stride, offset));
	}

	public void SetIndexBuffer(IGpuBuffer buffer)
	{
		Enqueue(() => _api.ApplySetIndexBuffer(buffer));
	}

	public void SetConstantBuffer(int slot, IGpuBuffer buffer, ShaderType stage)
	{
		Enqueue(() => _api.ApplySetConstantBuffer(slot, buffer, stage));
	}

	[MustDisposeResource]
	public IDisposable SetCullMode(CullMode mode)
	{
		var last = _cullModeStack.Peek();
		if (last == mode)
		{
			return new AutoScope();
		}

		_cullModeStack.Push(mode);
		Enqueue(() => _api.ApplySetCullMode(mode));
		return new AutoScope(onExit: ResumeCullMode);
	}

	public void ResumeCullMode()
	{
		if (_cullModeStack.Count <= 1)
			return;

		_cullModeStack.Pop();
		var current = _cullModeStack.Peek();
		Enqueue(() => _api.ApplySetCullMode(current));
	}

	[MustDisposeResource]
	public IDisposable SetBlendState(BlendState state)
	{
		var last = _blendStateStack.Peek();
		if (last.Equals(state))
		{
			return new AutoScope();
		}

		_blendStateStack.Push(state);
		Enqueue(() => _api.ApplySetBlendState(state));
		return new AutoScope(onExit: ResumeBlendState);
	}

	public void ResumeBlendState()
	{
		if (_blendStateStack.Count <= 1)
			return;

		_blendStateStack.Pop();
		var current = _blendStateStack.Peek();
		Enqueue(() => _api.ApplySetBlendState(current));
	}

	public void SetDepthEnabled(bool enabled, bool writeEnabled = true)
		=> Enqueue(() => _api.ApplySetDepthEnabled(enabled, writeEnabled));

	public void Draw(int vertexCount, int startVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
		=> Enqueue(() => _api.ApplyDraw(vertexCount, startVertexLocation, topology));

	public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
		=> Enqueue(() => _api.ApplyDrawIndexed(indexCount, startIndexLocation, baseVertexLocation, topology));

	internal void Execute()
	{
		if (!_begun || !_ended)
			throw new InvalidOperationException("Call Begin() and End() before Submit().");

		foreach (var command in _commands)
		{
			command();
		}

		_commands.Clear();
		_begun = false;
		_ended = false;
	}

	public void Dispose()
	{
		_commands.Clear();
	}
}