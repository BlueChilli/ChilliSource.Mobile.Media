#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using AssetsLibrary;
using AVFoundation;
using ChilliSource.Mobile.Core;
using ChilliSource.Mobile.Media;
using Foundation;
using Photos;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(MediaLibraryService))]

namespace ChilliSource.Mobile.Media
{
	public class MediaLibraryService : IMediaLibraryService
	{
		private enum MediaItemType
		{
			Video,
			Image
		}

		public Task<OperationResult> PickVideo(string destinationPath)
		{
			return GetMediaItem(destinationPath, MediaItemType.Video, UIImagePickerControllerSourceType.PhotoLibrary);
		}

		public Task<OperationResult> PickImage(string destinationPath)
		{
			return GetMediaItem(destinationPath, MediaItemType.Image, UIImagePickerControllerSourceType.PhotoLibrary);
		}

		public Task<OperationResult> CapturePhoto(string destinationPath)
		{
			return GetMediaItem(destinationPath, MediaItemType.Image, UIImagePickerControllerSourceType.Camera);
		}

		public Task<OperationResult> CaptureVideo(string destinationPath)
		{
			return GetMediaItem(destinationPath, MediaItemType.Video, UIImagePickerControllerSourceType.Camera);
		}

		public Task<OperationResult<string>> SaveVideo(string videoPath)
		{
			var tcs = new TaskCompletionSource<OperationResult<string>>();

			if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
			{
				tcs.SetResult(OperationResult<string>.AsFailure("Invalid video file path specified"));
				return tcs.Task;
			}

			var url = NSUrl.CreateFileUrl(videoPath, false, null);

			string localId = string.Empty;

			PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() =>
			{
				var request = PHAssetChangeRequest.FromVideo(url);
				localId = request?.PlaceholderForCreatedAsset?.LocalIdentifier;

			}, (bool success, NSError error) =>
			{
				if (!success && error != null)
				{
					tcs.SetResult(OperationResult<string>.AsFailure(error.LocalizedDescription));
				}
				else
				{
					tcs.SetResult(OperationResult<string>.AsSuccess(localId));
				}
			});

			return tcs.Task;
		}

		//needed for sharing to Facebook
		public Task<OperationResult<string>> LegacySaveVideo(string videoPath)
		{
			var tcs = new TaskCompletionSource<OperationResult<string>>();

			if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
			{
				tcs.SetResult(OperationResult<string>.AsFailure("Invalid video file path specified"));
				return tcs.Task;
			}


			var url = NSUrl.CreateFileUrl(videoPath, false, null);
			var library = new ALAssetsLibrary();
			library.WriteVideoToSavedPhotosAlbum(url, (resultUrl, error) =>
			 {
				 if (error == null)
				 {
					 tcs.SetResult(OperationResult<string>.AsSuccess(resultUrl.AbsoluteString));
				 }
				 else
				 {
					 tcs.SetResult(OperationResult<string>.AsFailure(error.LocalizedDescription));
				 }
			 });

			return tcs.Task;
		}

		public Task<OperationResult<string>> SaveImage(string imagePath)
		{
			var tcs = new TaskCompletionSource<OperationResult<string>>();

			if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
			{
				tcs.SetResult(OperationResult<string>.AsFailure("Invalid image file path specified"));
				return tcs.Task;
			}


			var url = NSUrl.CreateFileUrl(imagePath, false, null);

			string localId = string.Empty;

			PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() =>
			{
				var request = PHAssetChangeRequest.FromImage(url);
				localId = request?.PlaceholderForCreatedAsset?.LocalIdentifier;

			}, (bool success, NSError error) =>
			{
				if (!success && error != null)
				{
					tcs.SetResult(OperationResult<string>.AsFailure(error.LocalizedDescription));
				}
				else
				{
					tcs.SetResult(OperationResult<string>.AsSuccess(localId));
				}
			});

			return tcs.Task;
		}

		public Task<OperationResult<LibraryMediaItem>> GetLastSavedVideoInfo()
		{
			return GetLastSavedMediaItem(MediaItemType.Video);
		}

		public Task<OperationResult<LibraryMediaItem>> GetLastSavedImageInfo()
		{
			return GetLastSavedMediaItem(MediaItemType.Image);
		}

		private Task<OperationResult> GetMediaItem(string destinationPath, MediaItemType mediaItemType, UIImagePickerControllerSourceType sourceType)
		{
			var tcs = new TaskCompletionSource<OperationResult>();

			var picker = new ExtendedUIImagePickerController();
			picker.PreferredOrientation = UIApplication.SharedApplication.StatusBarOrientation;
			if (mediaItemType == MediaItemType.Video)
			{
				picker.VideoQuality = UIImagePickerControllerQualityType.At1280x720;
			}
			picker.SourceType = sourceType;
			var typeString = mediaItemType == MediaItemType.Video ? "public.movie" : "public.image";
			picker.MediaTypes = new string[] { typeString };

			picker.Canceled += (object sender, EventArgs e) =>
			{
				tcs.SetResult(OperationResult.AsCancel());
				picker.DismissViewController(true, null);

			};
			picker.FinishedPickingMedia += (object sender, UIImagePickerMediaPickedEventArgs e) =>
			{
				picker.DismissViewController(true, () =>
				 {
					 NSUrl mediaURl = null;

					 if (e.MediaUrl != null)
					 {
						 mediaURl = e.MediaUrl;
						 File.Copy(mediaURl.Path, destinationPath, true);
						 tcs.SetResult(OperationResult.AsSuccess());
					 }
					 else
					 {
						 var originalImage = e.OriginalImage;
						 var imageData = originalImage.AsJPEG();

						 NSError err = null;

						 if (imageData.Save(destinationPath, false, out err))
						 {
							 tcs.SetResult(OperationResult.AsSuccess());
						 }
						 else
						 {
							 tcs.SetResult(OperationResult.AsFailure(err.LocalizedDescription));
						 }
					 }
				 });
			};

			NavigationHelper.GetActiveViewController().PresentViewController(picker, true, null);

			return tcs.Task;
		}

		private Task<OperationResult<LibraryMediaItem>> GetLastSavedMediaItem(MediaItemType itemType)
		{
			var tcs = new TaskCompletionSource<OperationResult<LibraryMediaItem>>();

			var fetchOptions = new PHFetchOptions();
			fetchOptions.SortDescriptors = new NSSortDescriptor[] { new NSSortDescriptor("creationDate", false) };
			var fetchResult = PHAsset.FetchAssets(itemType == MediaItemType.Video ? PHAssetMediaType.Video : PHAssetMediaType.Image, fetchOptions);
			var phAsset = fetchResult?.firstObject as PHAsset;
			if (phAsset != null)
			{
				PHImageManager.DefaultManager.RequestAvAsset(phAsset, null, (asset, audioMix, info) =>
				{
					var urlAsset = asset as AVUrlAsset;
					tcs.SetResult(OperationResult<LibraryMediaItem>.AsSuccess(new LibraryMediaItem(phAsset.LocalIdentifier, urlAsset.Url.AbsoluteString)));
				});
			}
			else
			{
				tcs.SetResult(OperationResult<LibraryMediaItem>.AsFailure("Could not retrieve last asset"));
			}

			return tcs.Task;
		}
	}
}
