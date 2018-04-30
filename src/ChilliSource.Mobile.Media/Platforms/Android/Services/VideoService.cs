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
using Android.Content;
using System.IO;
using ChilliSource.Mobile.Media;
using ChilliSource.Mobile.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: Dependency(typeof(VideoService))]

namespace ChilliSource.Mobile.Media
{
	public class VideoService : IVideoService
	{
		public OperationResult PlayVideo(string videoPath, bool showInLandscape = false)
		{
			if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
			{
				return OperationResult.AsFailure("Invalid video file path specified");
			}

			var intent = new Intent(Intent.ActionView);
			intent.SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(videoPath)), "video/*");
			Forms.Context.StartActivity(Intent.CreateChooser(intent, "Complete action using"));

			return OperationResult.AsSuccess();
		}


		public Task<OperationResult> AddAudioToVideoTrack(string videoFilePath, string audioFilePath, string outputFilePath, float volume, float fadeOutDuration)
		{
			throw new NotImplementedException();
		}
		
		public OperationResult<System.IO.Stream> GetFirstVideoFrame(string filePath)
		{
			throw new NotImplementedException();
		}

		public OperationResult<double> GetVideoDuration(string filePath)
		{
			throw new NotImplementedException();
		}

		public Core.Orientation GetVideoOrientation(string filePath)
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult<string>> PresentVideoEditor(string videoPath, double maxDuration)
		{
			throw new NotImplementedException();
		}


		public Task<OperationResult> TrimVideo(string sourcePath, string destinationPath, double startTime, double endTime)
		{
			throw new NotImplementedException();
		}

	}
}

