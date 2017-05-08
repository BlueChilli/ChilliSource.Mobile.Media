#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using AVFoundation;
using Foundation;
using System.IO;
using ChilliSource.Mobile.Media;
using Xamarin.Forms;
using ChilliSource.Mobile.Core;

[assembly: Dependency(typeof(AudioPlaybackService))]

namespace ChilliSource.Mobile.Media
{
	public class AudioPlaybackService : IAudioPlaybackService
	{
		private AVAudioPlayer _player;

		public event EventHandler OnPlaybackCompleted;

		public OperationResult<double> Initialize(string audioFilePath)
		{

			if (string.IsNullOrEmpty(audioFilePath) || !File.Exists(audioFilePath))
			{
				return OperationResult<double>.AsFailure("Invalid audio file path specified");
			}

			var audioSession = AVAudioSession.SharedInstance();
			var error = audioSession.SetCategory(AVAudioSessionCategory.Playback);
			if (error != null)
			{
				return OperationResult<double>.AsFailure(error.LocalizedDescription);
			}
			error = audioSession.SetActive(true);
			if (error != null)
			{
				return OperationResult<double>.AsFailure(error.LocalizedDescription);
			}

			NSUrl audioUrl = NSUrl.FromFilename(audioFilePath);

			if (_player != null)
			{
				_player.FinishedPlaying -= DidFinishPlaying;
				_player.Dispose();
				_player = null;
			}

			_player = AVAudioPlayer.FromUrl(audioUrl);
			_player.NumberOfLoops = 0;
			_player.FinishedPlaying += DidFinishPlaying;

			return OperationResult<double>.AsSuccess(_player.Duration);
		}

		public void Dispose()
		{
			if (_player != null)
			{
				_player.FinishedPlaying -= DidFinishPlaying;
				_player.Dispose();
				_player = null;
			}
		}

		public void Play()
		{
			_player?.PrepareToPlay();
			_player?.Play();
		}

		public void Pause()
		{
			_player?.Pause();
		}

		public void Resume()
		{
			_player?.Play();
		}

		public void Stop()
		{
			if (_player != null && _player.Playing)
			{
				_player.Stop();
			}
		}

		public void Rewind()
		{
			if (_player != null)
			{
				if (_player.Playing)
				{
					_player.Stop();
					_player.CurrentTime = 0;
					_player.Play();
				}
				else
				{
					_player.Stop();
					_player.CurrentTime = 0;
				}
			}
		}

		private void DidFinishPlaying(object sender, AVStatusEventArgs e)
		{
			if (OnPlaybackCompleted != null)
			{
				OnPlaybackCompleted.Invoke(sender, e);
			}
		}
	}
}

