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
using System.Timers;
using Xamarin.Forms;
using ChilliSource.Mobile.Media;
using ChilliSource.Mobile.Core;

[assembly: Dependency(typeof(AudioRecordingService))]

namespace ChilliSource.Mobile.Media
{
	public class AudioRecordingService : IAudioRecordingService
	{

		private AVAudioRecorder _recorder;
		private NSDictionary _settings;
		private Timer _timer;

		public OperationResult Initialize(string audioFilePath, float sampleRate = 44100, int channels = 2, int bitDepth = 16)
		{
			if (string.IsNullOrEmpty(audioFilePath))
			{
				return OperationResult.AsFailure("Invalid audio file path specified");
			}

			var audioSession = AVAudioSession.SharedInstance();

			var error = audioSession.SetCategory(AVAudioSessionCategory.Record);
			if (error != null)
			{
				return OperationResult.AsFailure(error.LocalizedDescription);
			}
			error = audioSession.SetActive(true);
			if (error != null)
			{
				return OperationResult.AsFailure(error.LocalizedDescription);
			}

			var url = NSUrl.FromFilename(audioFilePath);

			NSObject[] values = new NSObject[]
			{
				NSNumber.FromFloat (sampleRate), //Sample Rate
				NSNumber.FromInt32 ((int)AudioToolbox.AudioFormatType.MPEG4AAC), //AVFormat
				NSNumber.FromInt32 (channels), //Channels
				NSNumber.FromInt32 (bitDepth), //PCMBitDepth
				NSNumber.FromBoolean (false), //IsBigEndianKey
				NSNumber.FromBoolean (false) //IsFloatKey
			};

			NSObject[] keys = new NSObject[]
			{
				AVAudioSettings.AVSampleRateKey,
				AVAudioSettings.AVFormatIDKey,
				AVAudioSettings.AVNumberOfChannelsKey,
				AVAudioSettings.AVLinearPCMBitDepthKey,
				AVAudioSettings.AVLinearPCMIsBigEndianKey,
				AVAudioSettings.AVLinearPCMIsFloatKey
			};

			_settings = NSDictionary.FromObjectsAndKeys(values, keys);
			_recorder = AVAudioRecorder.Create(url, new AudioSettings(_settings), out error);

			if (error != null)
			{
				return OperationResult.AsFailure("Error creating audio file: " + error.LocalizedDescription);
			}

			_recorder.MeteringEnabled = true;
			var success = _recorder.PrepareToRecord();
			if (success)
			{
				return OperationResult.AsSuccess();
			}
			else
			{
				return OperationResult.AsFailure("Could not initialize recorder");
			}
		}

		public void Record()
		{
			if (_recorder != null)
			{

				_recorder.Record();

				if (_timer == null)
				{
					_timer = new Timer(100);
					_timer.Elapsed += OnTimedEvent;
				}

				_timer.Start();
			}
		}

		public void Pause()
		{
			if (_recorder != null)
			{
				_recorder.Pause();
				_timer.Stop();
			}
		}

		public void Stop()
		{
			if (_recorder != null)
			{
				_recorder.Stop();
				ClearTimer();
			}
		}

		public void Clear()
		{
			if (_recorder != null)
			{
				if (_recorder.Recording)
				{
					_recorder.Stop();
					_timer.Stop();
				}
				_recorder.DeleteRecording();
				_recorder.Dispose();
				_recorder = null;

			}

			ClearTimer();
		}

		public void Dispose()
		{
			Clear();
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			_recorder.UpdateMeters();
		}

		private void ClearTimer()
		{
			if (_timer != null)
			{
				_timer.Elapsed -= OnTimedEvent;
				_timer.Dispose();
				_timer = null;
			}
		}
	}
}

