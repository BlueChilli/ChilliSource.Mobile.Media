#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Util;
using ChilliSource.Mobile.Core;
using ChilliSource.Mobile.Media;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageService))]

namespace ChilliSource.Mobile.Media
{
	public class ImageService : IImageService
	{
		public OperationResult ResizeImage(string sourcePath, string targetPath, float maxWidth, float maxHeight)
		{

			if (!File.Exists(sourcePath))
			{
				return OperationResult.AsFailure("Source file not found");
			}

			if (File.Exists(targetPath))
			{
				return OperationResult.AsFailure("Target file already exists");
			}

			var targetFileExtension = System.IO.Path.GetExtension(targetPath).ToLower();

			if (targetFileExtension.Equals("png") || targetFileExtension.Equals("jpg") || targetFileExtension.Equals("jpeg"))
			{
				return OperationResult.AsFailure("Invalid extension of target file");
			}


			// First decode with inJustDecodeBounds=true to check dimensions
			var options = new BitmapFactory.Options()
			{
				InJustDecodeBounds = false,
				InPurgeable = true,
			};

			bool success = false;

			using (var image = BitmapFactory.DecodeFile(sourcePath, options))
			{
				if (image != null)
				{
					var sourceSize = new Android.Util.Size((int)image.GetBitmapInfo().Height, (int)image.GetBitmapInfo().Width);

					var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

					string targetDir = System.IO.Path.GetDirectoryName(targetPath);
					if (!Directory.Exists(targetDir))
					{
						Directory.CreateDirectory(targetDir);
					}

					if (maxResizeFactor > 0.9)
					{
						File.Copy(sourcePath, targetPath);
						success = true;
					}
					else
					{
						var width = (int)(maxResizeFactor * sourceSize.Width);
						var height = (int)(maxResizeFactor * sourceSize.Height);

						using (var bitmapScaled = Bitmap.CreateScaledBitmap(image, height, width, true))
						{
							using (Stream outStream = File.Create(targetPath))
							{

								if (targetFileExtension.Equals("png"))
								{
									success = bitmapScaled.Compress(Bitmap.CompressFormat.Png, 100, outStream);
								}
								else if (targetFileExtension.Equals("jpg") || targetFileExtension.Equals("jpeg"))
								{
									success = bitmapScaled.Compress(Bitmap.CompressFormat.Jpeg, 95, outStream);
								}
							}
							bitmapScaled.Recycle();
						}
					}

					image.Recycle();

					if (success)
					{
						return OperationResult.AsSuccess();
					}
					else
					{
						return OperationResult.AsFailure("Failed to save image to target path");
					}
				}
				else
				{
					return OperationResult.AsFailure("Image decoding failed");
				}

			}

		}
	}
}
