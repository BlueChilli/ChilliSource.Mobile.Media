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
	/// Provides methods for native audio file playback
	/// </summary>
	public interface IAudioPlaybackService
	{
		/// <summary>
		/// Initializes the service for playback of the audio file at the specified path
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome
		///  of the operation and holding the length of the audio file</returns>
		/// <param name="audioFilePath">Audio file path.</param>
		OperationResult<double> Initialize(string audioFilePath);

		/// <summary>
		/// Plays audio
		/// </summary>
		void Play();

		/// <summary>
		/// Pauses playback
		/// </summary>
		void Pause();

		/// <summary>
		/// Resumes paused playback
		/// </summary>
		void Resume();

		/// <summary>
		/// Stops playback and undoes setup needed for playback. The Play method needs to be invoked to resume playback.
		/// </summary>
		void Stop();

		/// <summary>
		/// Rewinds the playback to the beginning of the audio file. If the playback was in progress when this method 
		/// is called then the file will start playing automatically from the beginning.
		/// </summary>
		void Rewind();

		/// <summary>
		/// Releases all resource used by the <see cref="T:ChilliSource.Mobile.Media.IAudioPlaybackService"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="T:ChilliSource.Mobile.Media.IAudioPlaybackService"/>. The <see cref="Dispose"/> method leaves the
		/// <see cref="T:ChilliSource.Mobile.Media.IAudioPlaybackService"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the
		/// <see cref="T:ChilliSource.Mobile.Media.IAudioPlaybackService"/> so the garbage collector can reclaim the memory
		/// that the <see cref="T:ChilliSource.Mobile.Media.IAudioPlaybackService"/> was occupying.</remarks>
		void Dispose();

		/// <summary>
		/// Raised when audio playback has reached the end of the file
		/// </summary>
		event EventHandler OnPlaybackCompleted;

	}
}

