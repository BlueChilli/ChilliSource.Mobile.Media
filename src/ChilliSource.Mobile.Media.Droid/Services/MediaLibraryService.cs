#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using System.Threading.Tasks;
using ChilliSource.Mobile.Core;
using ChilliSource.Mobile.Media;
using Xamarin.Forms;


[assembly: Dependency(typeof(MediaLibraryService))]

namespace ChilliSource.Mobile.Media
{
	public class MediaLibraryService : IMediaLibraryService
	{
		public Task<OperationResult> CapturePhoto(string destinationPath)
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult> CaptureVideo(string destinationPath)
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult> PickImage(string destinationPath)
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult> PickVideo(string destinationPath)
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult<string>> SaveImage(string imagePath)
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult<string>> LegacySaveVideo(string videoPath)
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult<string>> SaveVideo(string videoPath)
		{
			throw new NotImplementedException();
		}

		public OperationResult<string> GetLastSavedVideoId()
		{
			throw new NotImplementedException();
		}

		public OperationResult<string> GetLastSavedImageId()
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult<LibraryMediaItem>> GetLastSavedVideoInfo()
		{
			throw new NotImplementedException();
		}

		public Task<OperationResult<LibraryMediaItem>> GetLastSavedImageInfo()
		{
			throw new NotImplementedException();
		}

	}
}

