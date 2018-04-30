#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ChilliSource.Mobile.Core;

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Provides methods for working with videos
	/// </summary>
	public interface IVideoService
	{
		/// <summary>
		/// Opens a native video player screen and plays the video at the specified path
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="videoPath">Video path.</param>
		/// <param name="showInLandscape">If set to <c>true</c> show the video player screen in landscape orientation.</param>
		OperationResult PlayVideo(string videoPath, bool showInLandscape = false);

		/// <summary>
		/// Returns the duration of the video at the specified file path
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation
		///  and holding the duration</returns>
		/// <param name="filePath">File path.</param>
		OperationResult<double> GetVideoDuration(string filePath);

		/// <summary>
		/// Calculates the orientation based on the transform of the first video track in the video file at the specified path
		/// </summary>
		/// <returns>The video orientation.</returns>
		/// <param name="filePath">File path.</param>
		Orientation GetVideoOrientation(string filePath);

        /// <summary>
		/// Returns a Stream representing the image data of the first frame of the video at <paramref name="filePath"/>.  
		/// </summary>
		/// <example>
		/// Usage example
		/// <code>
		/// var stream = GetFirstVideoFrame(videoFilePath).Result;
		/// var image = await ImageSource.FromStream(() => stream);				
		/// </code>
		/// </example>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation and 
		/// holding the image stream</returns>
		/// <param name="filePath">File path.</param>
		OperationResult<Stream> GetFirstVideoFrame(string filePath);

        /// <summary>
        /// Presents the native video editor page and loads the video at the specified path
        /// </summary>
        /// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation
        ///  and holding the path to the edited output file</returns>
        /// <param name="videoPath">Video path.</param>
        /// <param name="maxDuration">The maximum length that the video can be edited to</param>
        Task<OperationResult<string>> PresentVideoEditor(string videoPath, double maxDuration);


		/// <summary>
		/// Trims the video at the specified source path according to the <paramref name="startTime"/> and <paramref name="endTime"/> and 
		/// saves it to the destination path.
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="sourcePath">Source path.</param>
		/// <param name="destinationPath">Destination path.</param>
		/// <param name="startTime">Start time.</param>
		/// <param name="endTime">End time.</param>
		Task<OperationResult> TrimVideo(string sourcePath, string destinationPath, double startTime, double endTime);

		
		/// <summary>
		/// Combines the video and audio tracks of the provided files into a single video file. The audio track of the 
		/// source video file will be ignored.
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="videoFilePath">Video file path.</param>
		/// <param name="audioFilePath">Audio file path.</param>
		/// <param name="outputFilePath">Output file path.</param>
		/// <param name="volume">Volume.</param>
		/// <param name="fadeOutDuration">Fade out duration.</param>
		Task<OperationResult> AddAudioToVideoTrack(string videoFilePath, string audioFilePath, string outputFilePath,
												   float volume = 1, float fadeOutDuration = 0);

		
	}
}
