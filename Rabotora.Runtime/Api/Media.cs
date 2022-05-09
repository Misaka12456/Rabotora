using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System.Enhance.SharpDX.MediaFoundation;

namespace Rabotora.Runtime.Api
{
	/// <summary>
	/// Presents Media(Audio, Video and so on) Api for Rabotora Game Project which is in Runtime Environment.
	/// </summary>
	public static class Media
	{
		/// <summary>
		/// Interval between two audio playing procedures when the audio is loop playing (in seconds).
		/// </summary>
		public const float LoopInterval = 0.5f;

		/// <summary>
		/// List of registered media session(s).
		/// </summary>
		private static Dictionary<int, MediaSession> _sessions = new();

		/// <summary>
		/// List of the running media session id(s).
		/// </summary>
		public static List<int> _runningSessions = new();

		/// <summary>
		/// Play audio instantly.
		/// </summary>
		/// <param name="audioData">Audio data.</param>
		/// <param name="sessionId">If audio starts playing successfully, this will return the session id of the audio play session; otherwise, <see langword="null" /> .</param>
		/// <param name="isLoop">Should we loop playing the audio(play the audio after it ends)?</param>
		/// <param name="isStartupMediaManager">Should we startup the media manager before playing the audio?</param>
		/// <param name="finishPlayingCallback">The action which will be called after <b>natually finished</b> playing the <b>non-loop-play</b> audio. Can be <see langword="null" /> .</param>
		/// <returns>Did the audio started playing successfully?</returns>
		public static bool PlayAudio(byte[] audioData, out int? sessionId, bool isLoop = false, bool isStartupMediaManager = false, Action? finishPlayingCallback = null)
		{
			bool r = MediaHelper.CreateMediaSession(audioData, out var session, null, isStartupMediaManager);
			if (r && session != null)
			{
				sessionId = _sessions.Count - 1;
				_sessions.Add(sessionId.Value, session);
				session.Start(null, new Variant()
				{
					ElementType = VariantElementType.Float,
					Type = VariantType.Default,
					Value = 0.0f
				});
				_runningSessions.Add(sessionId.Value);
				new Thread((object? idWrapper) =>
				{
					int sessionId = (int)idWrapper!;
					for (; ; )
					{
						var @event = session.GetEvent(true);
						if (@event.TypeInfo == MediaEventTypes.SessionStopped || @event.TypeInfo == MediaEventTypes.SessionClosed || @event.TypeInfo == MediaEventTypes.SessionEnded)
						{
							if (isLoop && _runningSessions.Contains(sessionId))
							{
								session.Start(null, new Variant()
								{
									ElementType = VariantElementType.Float,
									Type = VariantType.Default,
									Value = 0.0f
								});
								Thread.Sleep((int)(LoopInterval * 1000));
							}
							else
							{
								break;
							}
						}
					}
					if (_runningSessions.Contains(sessionId))
					{
						DisposeSession(sessionId);
						finishPlayingCallback?.Invoke();
					}
				})
				{ IsBackground = true }.Start(sessionId.Value);
				return true;
			}
			else
			{
				sessionId = null;
				return false;
			}
		}

		/// <summary>
		/// Play video instantly.
		/// </summary>
		/// <param name="videoData">Video data.</param>
		/// <param name="hVideoWnd">The handle of which window we should play the video on.</param>
		/// <param name="sessionId">If video starts playing successfully, this will return the session id of the video play session; otherwise, <see langword="null" /> .</param>
		/// <param name="isStartupMediaManager">Should we startup the media manager before playing the video?</param>
		/// <param name="finishPlayingCallback">The action which will be called after <b>natually finished</b> playing the video. Can be <see langword="null" /> .</param>
		/// <returns>Did the video started playing successfully?</returns>
		public static bool PlayVideo(byte[] videoData, IntPtr hVideoWnd, out int? sessionId, bool isStartupMediaManager = false, Action? finishPlayingCallback = null)
		{
			bool r = MediaHelper.CreateMediaSession(videoData, out var session, hVideoWnd, isStartupMediaManager);
			if (r && session != null)
			{
				sessionId = _sessions.Count - 1;
				_sessions.Add(sessionId.Value, session);
				session.Start(null, new Variant()
				{
					ElementType = VariantElementType.Float,
					Type = VariantType.Default,
					Value = 0.0f
				});
				new Thread((object? idWrapper) =>
				{
					int sessionId = (int)idWrapper!;
					for (; ; )
					{
						var @event = session.GetEvent(true);
						if (@event.TypeInfo == MediaEventTypes.SessionStopped || @event.TypeInfo == MediaEventTypes.SessionClosed || @event.TypeInfo == MediaEventTypes.SessionEnded)
						{
							break;
						}
					}
					if (_runningSessions.Contains(sessionId))
					{
						DisposeSession(sessionId);
						finishPlayingCallback?.Invoke();
					}
				})
				{ IsBackground = true }.Start(sessionId.Value);
				return true;
			}
			else
			{
				sessionId = null;
				return false;
			}
		}

		/// <summary>
		/// Stop the audio which is playing instantly.
		/// <para>
		/// If the audio is not loop playing, it will automatically stop when naturally ended.<br />
		/// Call this method for the audio which is stopped and not loop played will <b>do nothing</b>.
		/// </para>
		/// </summary>
		/// <param name="sessionId">The id of the target audio session.</param>
		/// <param name="isShutdownMediaManager">Should we shut down the media manager after stopping the audio?</param>
		/// <returns>Did the audio stopped playing successfully?</returns>
		public static bool StopAudio(int sessionId, bool isShutdownMediaManager = false)
		{
			var r = StopMedia(sessionId);
			if (isShutdownMediaManager)
			{
				MediaManager.Shutdown();
			}
			return r;
		}

		/// <summary>
		/// Stop the video which is playing instantly.
		/// <para>
		/// Call this method for the video which is stopped will <b>do nothing</b>.
		/// </para>
		/// </summary>
		/// <param name="sessionId">The id of the target video session.</param>
		/// <param name="isShutdownMediaManager">Should we shut down the media manager after stopping the video?</param>
		/// <param name="renderTargetRecreation">Which action will we called to recreate(repaint) render target after stopping the video? (Default is <see langword="null" />.)</param>
		/// <returns>Did the video stopped playing successfully?</returns>
		public static bool StopVideo(int sessionId, bool isShutdownMediaManager = false, Action? renderTargetRecreation = null)
		{
			var r = StopMedia(sessionId);
			if (r)
			{
				if (isShutdownMediaManager)
				{
					MediaManager.Shutdown();
				}
				renderTargetRecreation?.Invoke();
			}
			return r;
		}

		/// <summary>
		/// Stop the media wich is playing instantly. <b>You should not call this method directly.</b>
		/// </summary>
		/// <param name="sessionId">The id of the target media session.</param>
		/// <returns>Did the media stopped playing successfully?</returns>
		private static bool StopMedia(int sessionId)
		{
			if (_runningSessions.Contains(sessionId))
			{
				if (_sessions.TryGetValue(sessionId, out var session))
				{
					_runningSessions.Remove(sessionId);
					session.Stop();
					DisposeSession(sessionId);
					return true;
				}
				else
				{
					DisposeSession(sessionId, true);
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Dispose the media session. <b>You should not call this method directly.</b>
		/// </summary>
		/// <param name="sessionId">The id of the media session which is ready to be disposed.</param>
		/// <param name="isRemoveRunningSessionList">Should we remove the session id from the running session list( <see cref="_runningSessions"/> )?</param>
		private static void DisposeSession(int sessionId, bool isRemoveRunningSessionList = false)
		{
			if (isRemoveRunningSessionList)
			{
				_runningSessions.Remove(sessionId);
			}
			if (_sessions.TryGetValue(sessionId, out var session))
			{
				session?.Dispose();
				_sessions.Remove(sessionId);
			}
		}
	}
}
