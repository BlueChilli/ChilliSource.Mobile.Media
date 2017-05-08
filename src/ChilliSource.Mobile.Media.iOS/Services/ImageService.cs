#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System.IO;
using ChilliSource.Mobile.Core;
using ChilliSource.Mobile.Media;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(ImageService))]
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

			var targetFileExtension = Path.GetExtension(targetPath).ToLower();

			if (targetFileExtension.Equals("png") || targetFileExtension.Equals("jpg") || targetFileExtension.Equals("jpeg"))
			{
				return OperationResult.AsFailure("Invalid extension of target file");
			}

			if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
			}

			UIImage sourceImage = UIImage.FromFile(sourcePath);

			var resultImage = sourceImage.Resize(maxWidth, maxHeight);

			bool success = false;
			if (targetFileExtension.Equals("png"))
			{
				success = resultImage.AsPNG().Save(targetPath, true);
			}
			else if (targetFileExtension.Equals("jpg") || targetFileExtension.Equals("jpeg"))
			{
				success = resultImage.AsJPEG().Save(targetPath, true);
			}

			if (success)
			{
				return OperationResult.AsSuccess();
			}
			else
			{
				return OperationResult.AsFailure("Failed to save image to target path");
			}
		}
	}
}
