#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using CoreGraphics;
using ChilliSource.Mobile.Core;
using Foundation;
using UIKit;

namespace ChilliSource.Mobile.Media
{
	public static class UIImageExtensions
	{
		/// <summary>
		/// Saves image in JPEG to the specified path
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="image">Image.</param>
		/// <param name="filePath">File path.</param>
		public static OperationResult SaveAsJpeg(this UIImage image, string filePath)
		{
			if (image == null)
			{
				return OperationResult.AsFailure("provided image is null");
			}

			using (NSData imageData = image.AsJPEG())
			{
				NSError err;
				if (!imageData.Save(filePath, true, out err))
				{
					return OperationResult.AsFailure("Saving of file failed: " + err.Description);
				}
				else
				{
					return OperationResult.AsSuccess();
				}
			}
		}

		/// <summary>
		/// Saves image in PNG to the specified path
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="image">Image.</param>
		/// <param name="filePath">File path.</param>
		public static OperationResult SaveAsPng(this UIImage image, string filePath)
		{
			if (image == null)
			{
				return OperationResult.AsFailure("provided image is null");
			}

			using (NSData imageData = image.AsPNG())
			{
				NSError err;
				if (!imageData.Save(filePath, true, out err))
				{
					return OperationResult.AsFailure("Saving of file failed: " + err.Description);
				}
				else
				{
					return OperationResult.AsSuccess();
				}
			}
		}

		/// <summary>
		/// Creates a 1x1 UIImage that has the specified background color
		/// </summary>
		/// <returns>The color.</returns>
		/// <param name="color">Color.</param>
		public static UIImage FromColor(UIColor color)
		{
			var rect = new CGRect(0, 0, 1, 1);
			UIGraphics.BeginImageContext(rect.Size);

			using (var context = UIGraphics.GetCurrentContext())
			{
				context.SetFillColor(color.CGColor);
				context.FillRect(rect);
				return UIGraphics.GetImageFromCurrentImageContext();
			}
		}

		/// <summary>
		/// Returns a resized copy of <paramref name="sourceImage"/> that it fits 
		/// in a rectangle with <paramref name="maxWidth"/> and <paramref name="maxHeight"/> dimensions
		/// </summary>
		/// <returns>The resized image</returns>
		/// <param name="maxWidth">Max width.</param>
		/// <param name="maxHeight">Max height.</param>
		public static UIImage Resize(this UIImage sourceImage, float maxWidth, float maxHeight)
		{
			var sourceSize = sourceImage.Size;
			var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			var width = maxResizeFactor * sourceSize.Width;
			var height = maxResizeFactor * sourceSize.Height;

			UIGraphics.BeginImageContextWithOptions(new CGSize((float)width, (float)height), true, 1.0f);

			sourceImage.Draw(new CGRect(0, 0, (float)width, (float)height));

			var resultImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return resultImage;
		}

	}
}
