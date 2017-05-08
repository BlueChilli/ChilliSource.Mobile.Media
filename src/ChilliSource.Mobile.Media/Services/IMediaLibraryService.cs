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

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Provides methods to save and retrieve media items to and from the media library
	/// </summary>
	public interface IMediaLibraryService
	{
		/// <summary>
		/// Opens the native screen that prompts the user to select a video from the device's media library
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="destinationPath">File path of where the selected video should be saved</param>
		Task<OperationResult> PickVideo(string destinationPath);

		/// <summary>
		/// Opens the native screen that prompts the user to select an image from the device's media library
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="destinationPath">File path of where the selected image should be saved</param>
		Task<OperationResult> PickImage(string destinationPath);

		/// <summary>
		/// Opens the native camera screen that allows the user to capture a photo
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="destinationPath">File path of where the captured photo should be saved</param>
		Task<OperationResult> CapturePhoto(string destinationPath);

		/// <summary>
		/// Opens the native camera screen that allows the user to capture a video
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="destinationPath">File path of where the captured video should be saved.</param>
		Task<OperationResult> CaptureVideo(string destinationPath);

		/// <summary>
		/// Saves the video at the specified path to the native media library
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation
		///  and holding a unique native identifier for the saved video</returns>
		/// <param name="videoPath">Source video path.</param>
		Task<OperationResult<string>> SaveVideo(string videoPath);

		/// <summary>
		/// Saves the video at the specified path to the native media library. This method uses a legacy iOS approach to 
		/// save media files to the media library, and is currently
		/// still required for Facebook integration.
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation
		///  and holding a unique native identifier for the saved video</returns>
		/// <param name="videoPath">Source video path.</param>
		Task<OperationResult<string>> LegacySaveVideo(string videoPath);

		/// <summary>
		/// Saves the image at the specified path to the native media library
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation
		///  and holding a unique native identifier for the saved image</returns>
		/// <param name="imagePath">Source image path.</param>
		Task<OperationResult<string>> SaveImage(string imagePath);

		/// <summary>
		/// Returns a LibraryMediaItem instance holding identifier information about the video that was saved 
		/// to the media library most recently
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation
		///  and holding a LibraryMediaItem instance</returns>
		Task<OperationResult<LibraryMediaItem>> GetLastSavedVideoInfo();

		/// <summary>
		/// Returns a LibraryMediaItem instance holding identifier information about the image that was saved to the 
		/// media library most recently
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation 
		/// and holding a LibraryMediaItem instance</returns>
		Task<OperationResult<LibraryMediaItem>> GetLastSavedImageInfo();

	}
}
