#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using System.Threading.Tasks;

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Provides methods for working with the device's camera
	/// </summary>
	public interface ICameraService
	{

		/// <summary>
		/// Determine if the app has permission from the user to access the camera
		/// </summary>
		/// <value><c>true</c> if has camera access; otherwise, <c>false</c>.</value>
		bool HasCameraAccess { get; }

		/// <summary>
		/// Performs iOS permission prompt for accessing the camera, but only if the user has not been prompted before
		/// </summary>
		/// <returns><c>true</c> if has camera access; otherwise, <c>false</c>.</returns>
		Task<bool> RequestCameraAccess();

		/// <summary>
		/// Returns the number of cameras on the device that support video recording
		/// </summary>
		/// <returns>number of cameras</returns>
		int GetNumberOfVideoCameras();
	}
}
