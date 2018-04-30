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
	/// Provides methods for manipulating images
	/// </summary>
	public interface IImageService
	{
		/// <summary>
		/// Resizes the image at the specified source path so that it fits in a rectangle with <paramref name="maxWidth"/> and <paramref name="maxHeight"/> dimensions, 
		/// and saves the resized image to the specified target path. Only support Png and Jpeg file formats.
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="sourcePath">Source path.</param>
		/// <param name="targetPath">Target path.</param>
		/// <param name="maxWidth">Max width.</param>
		/// <param name="maxHeight">Max height.</param>
		OperationResult ResizeImage(string sourcePath, string targetPath, float maxWidth, float maxHeight);

        /// <summary>
        /// Resizes the image at the specified source path so that it fits in a rectangle with <paramref name="maxWidth"/> and <paramref name="maxHeight"/> dimensions
        /// and returns a byte array with the image data.
        /// </summary>
        /// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="maxWidth">Max width.</param>
        /// <param name="maxHeight">Max height.</param>
        OperationResult<byte[]> ResizeImage(string sourcePath, float maxWidth, float maxHeight);

    }
}
