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

			if (targetFileExtension.Contains("png") || targetFileExtension.Contains("jpg") || targetFileExtension.Contains("jpeg"))
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
			if (targetFileExtension.Contains("png"))
			{
				success = resultImage.AsPNG().Save(targetPath, true);
			}
			else if (targetFileExtension.Contains("jpg") || targetFileExtension.Contains("jpeg"))
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

        public OperationResult<byte[]> ResizeImage(string sourcePath, float maxWidth, float maxHeight)
        {
            if (!File.Exists(sourcePath))
            {
                return OperationResult<byte[]>.AsFailure("Source file not found");
            }

            var extension = Path.GetExtension(sourcePath).ToLower();
          
            UIImage sourceImage = UIImage.FromFile(sourcePath);
            var resultImage = sourceImage.Resize(maxWidth, maxHeight);
          
            if (extension.Contains("png"))
            {
                return OperationResult<byte[]>.AsSuccess(resultImage.AsPNG().ToArray());
            }
            else if (extension.Contains("jpg") || extension.Contains("jpeg"))
            {
                return OperationResult<byte[]>.AsSuccess(resultImage.AsJPEG().ToArray());
            }
            else
            {
                return OperationResult<byte[]>.AsFailure("Extension not supported");
            }
        }
    }
}
