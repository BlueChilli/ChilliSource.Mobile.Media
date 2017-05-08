#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using AVFoundation;
using ChilliSource.Mobile.Core;

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Produces AVCaptureDevice instances
	/// </summary>
	public static class AVCaptureDeviceFactory
	{
		public static OperationResult<AVCaptureDevice> CreateDevice(string mediaType, AVCaptureDevicePosition position)
		{
			AVCaptureDevice[] devices = AVCaptureDevice.DevicesWithMediaType(mediaType);
			if (devices.Length > 0)
			{
				AVCaptureDevice captureDevice = devices[0];
				foreach (var device in devices)
				{
					if (device.Position == position)
					{
						captureDevice = device;
						break;
					}
				}
				return OperationResult<AVCaptureDevice>.AsSuccess(captureDevice);
			}
			else
			{
				return OperationResult<AVCaptureDevice>.AsFailure("No devices found for provided media type");
			}
		}

	}
}
