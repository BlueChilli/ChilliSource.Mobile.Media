#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using ChilliSource.Mobile.Core;

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Provides methods for native audio recording
	/// </summary>
	public interface IAudioRecordingService
	{
		/// <summary>
		/// Initializes the service to create a new audio file at the specified path
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="audioFilePath">Output audio file path.</param>
		/// <param name="sampleRate">Sample rate.</param>
		/// <param name="channels">Channels.</param>
		/// <param name="bitDepth">Bit depth.</param>
		OperationResult Initialize(string audioFilePath, float sampleRate = 44100, int channels = 2, int bitDepth = 16);

		/// <summary>
		/// Starts recording to audio file
		/// </summary>
		void Record();

		/// <summary>
		/// Pauses recording. Call the Record() method to resume.
		/// </summary>
		void Pause();

		/// <summary>
		/// Stops recording and closes the audio file. Calling Record() will start a new recording and overwrite any previous ones.
		/// </summary>
		void Stop();

		/// <summary>
		/// Stops and deletes the recording.
		/// </summary>
		void Clear();

		/// <summary>
		/// Releases all resource used by the <see cref="T:ChilliSource.Mobile.Media.IAudioRecordingService"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="T:ChilliSource.Mobile.Media.IAudioRecordingService"/>. The <see cref="Dispose"/> method leaves the
		/// <see cref="T:ChilliSource.Mobile.Media.IAudioRecordingService"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the
		/// <see cref="T:ChilliSource.Mobile.Media.IAudioRecordingService"/> so the garbage collector can reclaim the memory
		/// that the <see cref="T:ChilliSource.Mobile.Media.IAudioRecordingService"/> was occupying.</remarks>
		void Dispose();

	}
}

