using SharpDX.MediaFoundation;
using SharpDX;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Enhance.SharpDX.MediaFoundation
{
	public static class MediaHelper
	{
		public static bool CreateMediaSession(byte[] mediaData, out MediaSession? session, IntPtr? windowHandle = null, bool isStartUpMediaManager = false)
		{
			PresentationDescriptor? pd = null;
			Topology? topology = null;
			try
			{
				if (isStartUpMediaManager)
				{
					MediaManager.Startup();
				}
				var attributes = new MediaAttributes(mediaData.Length);
				MediaFactory.CreateMediaSession(attributes, out session);
				var resolver = new SourceResolver();
				var byteStream = new ByteStream(mediaData);
				resolver.CreateObjectFromByteStream(byteStream, null, (int)SourceResolverFlags.ByteStream, null, out var objType, out var videoObject);
				GetMediaSource(videoObject, out var source);
				if (source != null)
				{
					MediaFactory.CreateTopology(out topology);
					source.CreatePresentationDescriptor(out pd);
					var r1 = CreatePlaybackTopology(source, pd, windowHandle.HasValue ? windowHandle.Value : IntPtr.Zero, out topology);
					if (r1.Success)
					{
						session.SetTopology(0, topology);
						return true;
					}
					else
					{
						session = null;
						topology = null;
						return false;
					}
				}
				else
				{
					session = null;
					topology = null;
					return false;
				}
			}
			catch (SharpDXException ex)
			{
				Debug.Print(ex.ToString());
				session = null;
				return false;
			}
			finally
			{
				pd?.Dispose();
				topology?.Dispose();
			}
		}

		#region DirectX(Windows Media Foundation) 核心代码部分
		private static Result GetMediaSource(IUnknown mediaObject, out MediaSource? source)
		{
			var sourceGuid = new Guid("279a808d-aec7-40c8-9c6b-a6b492c78a66");
			var r = mediaObject.QueryInterface(ref sourceGuid, out var sourceWrapper);
			if (r.Success)
			{
				source = new MediaSource(sourceWrapper);
			}
			else
			{
				source = null;
			}
			return r;
		}

		private static Result CreatePlaybackTopology(MediaSource source, PresentationDescriptor descriptor, IntPtr hVideoWnd, out Topology? topology)
		{
			Topology? _topology = null;
			try
			{
				MediaFactory.CreateTopology(out _topology);
				int index = 0;
				bool status = true;
				do
				{
					status = AddBranchToPartialTopology(_topology, source, descriptor, index, hVideoWnd);
					index++;
				} while (status);
				topology = _topology;
				return Result.Ok;
			}
			catch (SharpDXException ex)
			{
				topology = null;
				return ex.ResultCode;
			}
			finally
			{
				_topology?.Dispose();
			}
		}

		private static bool AddBranchToPartialTopology(Topology topology, MediaSource source, PresentationDescriptor descriptor, int streamIndex, IntPtr hVideoWnd)
		{
			StreamDescriptor? streamDescriptor = null;
			Activate? activate = null;
			TopologyNode? sourceNode = null, outputNode = null;
			try
			{
				streamDescriptor = descriptor.GetStreamDescriptorByIndex(streamIndex, out var isSelected);
				if (isSelected)
				{
					CreateMediaSinkActivate(descriptor, streamDescriptor, streamIndex, hVideoWnd, out activate);
					if (activate != null)
					{

						var sourceNodePtr = Marshal.AllocHGlobal(Marshal.SizeOf(source));
						MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);
						sourceNode.Object = source;
						topology.AddNode(sourceNode);
						MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);
						outputNode.Object = activate;
						topology.AddNode(outputNode);
						sourceNode.ConnectOutput(0, outputNode, 0);
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			catch (SharpDXException ex)
			{
				Debug.Print(ex.ToString());
				return false;
			}
			finally
			{
				streamDescriptor?.Dispose();
activate?.Dispose();
sourceNode?.Dispose();
outputNode?.Dispose();
}
}

		private static void CreateMediaSinkActivate(PresentationDescriptor pd, StreamDescriptor sd, int streamIndex, IntPtr hVideoWnd, out Activate? activate)
		{
			var handler = sd.MediaTypeHandler;
			var majorType = handler.MajorType;
			if (majorType == MediaTypeGuids.Audio)
			{
				MediaFactory.CreateAudioRendererActivate(out activate);
			}
			else if (majorType == MediaTypeGuids.Video)
			{
				MediaFactory.CreateVideoRendererActivate(hVideoWnd, out activate);
			}
			else
			{
				pd.DeselectStream(streamIndex);
				activate = null;
			}
		}
		#endregion
	}
}