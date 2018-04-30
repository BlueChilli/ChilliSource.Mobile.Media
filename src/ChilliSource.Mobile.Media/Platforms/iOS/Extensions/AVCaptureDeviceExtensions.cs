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
using Foundation;

namespace ChilliSource.Mobile.Media
{
	public static class AVCaptureDeviceExtensions
	{
		/// <summary>
		/// Sets the device's frame rate
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="device">Device.</param>
		/// <param name="frameRate">Frame rate.</param>
		/// <param name="timeValue">Time value.</param>
		public static OperationResult SetFrameRate(this AVCaptureDevice device, int frameRate, int timeValue = 1)
		{
			NSError error = null;
			if (!device.LockForConfiguration(out error))
			{
				return OperationResult.AsFailure(error?.LocalizedDescription ?? "Could not lock configuration");
			}

			double epsilon = 0.00000001;

			bool frameRateSet = false;
			foreach (var range in device.ActiveFormat.VideoSupportedFrameRateRanges)
			{
				if (range.MinFrameRate <= frameRate + epsilon && range.MaxFrameRate >= frameRate - epsilon)
				{
					device.ActiveVideoMaxFrameDuration = new CoreMedia.CMTime(timeValue, frameRate);
					device.ActiveVideoMinFrameDuration = new CoreMedia.CMTime(timeValue, frameRate);
					frameRateSet = true;
					break;
				}
			}

			device.UnlockForConfiguration();
			return frameRateSet ? OperationResult.AsSuccess() : OperationResult.AsFailure("Frame rate is not supported");
		}

		/// <summary>
		/// Sets the device's flash settings
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="device">Device.</param>
		/// <param name="flashMode">Flash mode.</param>
		public static OperationResult SetFlashMode(this AVCaptureDevice device, AVCaptureFlashMode flashMode)
		{
			if (device.HasFlash && device.IsFlashModeSupported(flashMode))
			{
				NSError error;
				if (device.LockForConfiguration(out error))
				{
					device.FlashMode = flashMode;
					device.UnlockForConfiguration();
					return OperationResult.AsSuccess();
				}
				else
				{
					return OperationResult.AsFailure(string.Format("Could not lock device for configuration: {0}", error));
				}
			}
			else
			{
				return OperationResult.AsSuccess();
			}
		}

	}
}
