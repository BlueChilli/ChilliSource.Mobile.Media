#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System.Threading.Tasks;
using AVFoundation;
using ChilliSource.Mobile.Media;

[assembly: Xamarin.Forms.Dependency(typeof(CameraService))]
namespace ChilliSource.Mobile.Media
{
	public class CameraService : ICameraService
	{
		public bool HasCameraAccess
		{
			get
			{
				return AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) == AVAuthorizationStatus.Authorized;
			}
		}

		public async Task<bool> RequestCameraAccess()
		{
			var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

			if (status == AVAuthorizationStatus.Authorized)
			{
				return true;
			}
			else if (status == AVAuthorizationStatus.NotDetermined)
			{
				return await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
			}
			else
			{
				return false;
			}
		}

		public int GetNumberOfVideoCameras()
		{
			return AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video).Length;
		}
	}
}
