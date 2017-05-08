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
using CoreGraphics;
using Foundation;

namespace ChilliSource.Mobile.Media
{
	public static class AVCaptureDeviceInputExtensions
	{
		/// <summary>
		/// Set the device's focus settings
		/// </summary>
		/// <returns><see cref="T:ChilliSource.Mobile.Core.OperationResult"/> instance indicating the outcome of the operation</returns>
		/// <param name="videoDeviceInput">Video device input.</param>
		/// <param name="focusMode">Focus mode.</param>
		/// <param name="exposureMode">Exposure mode.</param>
		/// <param name="pointOfInterest">Point of interest</param>
		/// <param name="monitorSubjectAreaChange">If set to <c>true</c> monitor subject area change.</param>
		public static OperationResult UpdateFocus(this AVCaptureDeviceInput videoDeviceInput, AVCaptureFocusMode focusMode,
												  AVCaptureExposureMode exposureMode, CGPoint pointOfInterest, bool monitorSubjectAreaChange)
		{

			if (videoDeviceInput == null)
			{
				return OperationResult.AsFailure("device input is null");
			}

			AVCaptureDevice device = videoDeviceInput.Device;
			NSError error;
			if (device.LockForConfiguration(out error))
			{
				if (device.FocusPointOfInterestSupported && device.IsFocusModeSupported(focusMode))
				{
					device.FocusPointOfInterest = pointOfInterest;
					device.FocusMode = focusMode;
				}
				if (device.ExposurePointOfInterestSupported && device.IsExposureModeSupported(exposureMode))
				{
					device.ExposurePointOfInterest = pointOfInterest;
					device.ExposureMode = exposureMode;
				}
				device.SubjectAreaChangeMonitoringEnabled = monitorSubjectAreaChange;
				device.UnlockForConfiguration();
				return OperationResult.AsSuccess();
			}
			else
			{
				return OperationResult.AsFailure(string.Format("Could not lock device for configuration: {0}", error));
			}

		}

	}
}
