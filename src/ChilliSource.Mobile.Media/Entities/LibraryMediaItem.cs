#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Holds identifier information about a media item located in the native media library
	/// </summary>
	public class LibraryMediaItem
	{
		public LibraryMediaItem(string libraryId, string libraryPath)
		{
			LibraryId = libraryId;
			LibraryPath = libraryPath;
		}

		/// <summary>
		/// The unique local media library identifier for the media item
		/// </summary>
		public string LibraryId { get; set; }

		/// <summary>
		/// The media library url to the media asset
		/// </summary>
		public string LibraryPath { get; set; }
	}
}
