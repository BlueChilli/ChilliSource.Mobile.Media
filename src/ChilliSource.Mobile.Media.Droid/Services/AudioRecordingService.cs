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

[assembly: Dependency(typeof(AudioRecordingService))]

namespace ChilliSource.Mobile.Media
{
	public class AudioRecordingService : IAudioRecordingService
	{
		private MediaRecorder _recorder;
		private string _audioFilePath;
		private bool _isRecording;

		public OperationResult Initialize(string audioFilePath, float sampleRate = 44100, int channels = 2, int bitDepth = 16)
		{
			if (string.IsNullOrEmpty(audioFilePath))
			{
				return OperationResult<double>.AsFailure("Invalid audio file path specified");
			}

			_audioFilePath = audioFilePath;

			if (_recorder == null)
			{
				_recorder = new MediaRecorder();
				_recorder.SetAudioSource(AudioSource.Mic);
				_recorder.SetOutputFormat(OutputFormat.ThreeGpp);
				_recorder.SetAudioEncoder(AudioEncoder.AmrNb);
			}
			else
			{
				if (File.Exists(_audioFilePath))
				{
					File.Delete(_audioFilePath);
				}
			}

			File.Create(audioFilePath);
			_recorder.SetOutputFile(audioFilePath);
			_recorder.Prepare();

			return OperationResult.AsSuccess();

		}

		public void Dispose()
		{
			Clear();
			if (_recorder != null)
			{
				_recorder.Dispose();
				_recorder = null;
			}
		}

		public void Record()
		{
			if (_recorder != null && !_isRecording)
			{
				_recorder.Start();
				_isRecording = true;
			}
		}

		public void Pause()
		{
			if (_recorder != null && _isRecording)
			{
				_recorder.Stop();
				_isRecording = false;
			}
		}

		public void Stop()
		{
			if (_recorder != null && _isRecording)
			{
				_recorder.Stop();
				_recorder.Reset();
				_isRecording = false;
			}

		}

		public void Clear()
		{
			Stop();
			if (File.Exists(_audioFilePath))
			{
				File.Delete(_audioFilePath);
			}
		}
	}
}

