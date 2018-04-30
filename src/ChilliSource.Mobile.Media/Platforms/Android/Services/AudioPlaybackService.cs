#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using Xamarin.Forms;
using Android.Media;
using System.IO;
using ChilliSource.Mobile.Media;
using ChilliSource.Mobile.Core;

[assembly: Dependency(typeof(AudioPlaybackService))]

namespace ChilliSource.Mobile.Media
{
	public class AudioPlaybackService : IAudioPlaybackService
	{
		private MediaPlayer _player;

		public event EventHandler OnPlaybackCompleted;

		public OperationResult<double> Initialize(string audioFilePath)
		{
			if (string.IsNullOrEmpty(audioFilePath) || !File.Exists(audioFilePath))
			{
				return OperationResult<double>.AsFailure("Invalid audio file path specified");
			}

			_player = new MediaPlayer();
			_player.SetDataSource(audioFilePath);
			_player.Prepare();
			_player.Completion += delegate
			{
				if (OnPlaybackCompleted != null)
				{
					OnPlaybackCompleted.Invoke(_player, null);
				}
			};
			return OperationResult<double>.AsSuccess(_player.Duration / 1000);
		}

		public void Play()
		{
			if (_player != null)
			{
				_player.Start();
			}
		}

		public void Pause()
		{
			if (_player != null)
			{
				_player.Pause();
			}
		}

		public void Resume()
		{
			if (_player != null)
			{
				_player.Start();
			}
		}

		public void Rewind()
		{
			if (_player != null)
			{
				if (_player.IsPlaying)
				{
					_player.Stop();
					_player.Reset();
					_player.Start();
				}
				else
				{
					_player.Stop();
					_player.Reset();
				}
			}
		}

		public void Stop()
		{
			if (_player != null && _player.IsPlaying)
			{
				_player.Stop();
			}
		}

		public void Dispose()
		{
			if (_player != null)
			{
				_player.Stop();
				_player.Dispose();
				_player = null;
			}
		}
	}
}

