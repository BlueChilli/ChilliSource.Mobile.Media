#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVFoundation;
using ChilliSource.Mobile.Core;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Use this class to add a camera view anywhere in your app
	/// </summary>
	/// <example>
	/// Usage example
	/// <code>
	/// var cameraView = new CameraView();
	/// View.AddSubview(cameraView);
	/// await cameraView.Setup(enableAudioRecording: true, enableStillImageCapture: false, 
	/// 	orientation: _preferredOrientation, numberOfCameras: numberOfCameras);
	/// cameraView.OnRecordingFinished += CameraView_OnRecordingFinished;
	/// await cameraView.StartRecording(filePath);
	/// cameraView.StopRecording();
	/// cameraView.RemoveFromSuperview();
	/// cameraView.Dispose();
	/// cameraView.OnRecordingFinished -= CameraView_OnRecordingFinished;
	/// </code>
	/// </example>
	public class CameraNativeView : UIView, IAVCaptureFileOutputRecordingDelegate
	{
		private enum CameraSetupResult
		{
			Success,
			CameraNotAuthorized,
			SessionConfigurationFailed
		}

		#region Fields

		private AVCaptureSession _session;
		private AVCaptureDeviceInput _videoDeviceInput;
		private AVCaptureMovieFileOutput _movieFileOutput;
		private AVCaptureStillImageOutput _stillImageOutput;

		private NSObject _subjectSubscriber;
		private IDisposable _runningObserver;
		private IDisposable _recordingObserver;
		private IDisposable _capturingStillObserver;
		private NSObject _runtimeErrorObserver;
		private NSObject _interuptionObserver;
		private NSObject _interuptionEndedObserver;

		private bool _enableAudioRecording;
		private bool _enableStillImageCapture;
		private nint _backgroundRecordingID;
		private CameraSetupResult _setupResult;
		private static Class _layerClass;

		#endregion

		#region Lifecycle

		public CameraNativeView() { }
		public CameraNativeView(CGRect frame) : base(frame: frame) { }

		protected override void Dispose(bool disposing)
		{			
			if (disposing)
			{
				RemoveObservers();
				if (_session != null)
				{
					if (_session.Running)
					{
						_session.StopRunning();
					}

					PreviewLayer.Session = null;

					if (_videoDeviceInput != null)
					{
						_session.RemoveInput(_videoDeviceInput);
						_videoDeviceInput.Dispose();
						_videoDeviceInput = null;
					}

					if (_movieFileOutput != null)
					{
						_session.RemoveOutput(_movieFileOutput);
						_movieFileOutput.Dispose();
						_movieFileOutput = null;
					}

					if (_stillImageOutput != null)
					{
						_session.RemoveOutput(_stillImageOutput);
						_stillImageOutput.Dispose();
						_stillImageOutput = null;
					}

					_session.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Properties


		public bool IsSessionRunning { get; private set; }
		public int NumberOfCameras { get; private set; }

		public bool IsRecording
		{
			get
			{
				return _movieFileOutput != null && _movieFileOutput.Recording;
			}
		}

		//This property in conjunction with the PreviewLayer property makes the AV layer the main layer of this view, thereby allowing the layer
		//to automatically resize when the view is rotated and therefore providing the smoothest transition.
		public static Class LayerClass
		{
			[Export("layerClass")]
			get
			{
				return _layerClass = _layerClass ?? new Class(typeof(AVCaptureVideoPreviewLayer));
			}
		}

		private AVCaptureVideoPreviewLayer PreviewLayer
		{
			get
			{
				return (AVCaptureVideoPreviewLayer)Layer;
			}
		}

		#endregion

		#region Public Events

		public delegate void RecordingStatusChangedEventHandler(bool isRecording);
		public event RecordingStatusChangedEventHandler OnRecordingStatusChanged;

		public delegate void SessionRunningChangedEventHandler(bool isRunning);
		public event SessionRunningChangedEventHandler OnSessionRunningChanged;

		public delegate void SessionErrorOccuredEventHandler(string errorMessage);
		public event SessionErrorOccuredEventHandler OnSessionErrorOccured;

		public delegate void SessionWasInterruptedEventHandler(bool resumePossible, bool cameraAvailable);
		public event SessionWasInterruptedEventHandler OnSessionWasInterrupted;

		public delegate void SessionInterruptionEndedEventHandler();
		public event SessionInterruptionEndedEventHandler OnSessionInterruptionEnded;

		public delegate void RecordingFinishedEventHandler(NSUrl outputFileUrl, Action completionCallback);
		public event RecordingFinishedEventHandler OnRecordingFinished;

		public delegate void ImageSnappedEventHandler(NSData imageData);
		public event ImageSnappedEventHandler OnImageSnapped;

		#endregion

		#region Public Methods

		public OperationResult StartCapture()
		{
			if (_setupResult != CameraSetupResult.Success)
			{
				return OperationResult.AsFailure("Setup has not completed successfully");
			}

			if (!_session.Running)
			{
				_session.StartRunning();
			}
			return OperationResult.AsSuccess();
		}

		public Task<OperationResult> Setup(bool enableAudioRecording, bool enableStillImageCapture = false, UIInterfaceOrientation orientation = UIInterfaceOrientation.Portrait, int numberOfCameras = 1)
		{

			TaskCompletionSource<OperationResult> tcs = new TaskCompletionSource<OperationResult>();
			var warnings = new List<string>();

			NumberOfCameras = numberOfCameras;

			_enableAudioRecording = enableAudioRecording;
			_enableStillImageCapture = enableStillImageCapture;
			_session = new AVCaptureSession();

			_backgroundRecordingID = -1;
			NSError error;
			var result = AVCaptureDeviceFactory.CreateDevice(AVMediaType.Video, AVCaptureDevicePosition.Back);

			if (!result.IsSuccessful)
			{
				_setupResult = CameraSetupResult.SessionConfigurationFailed;
				tcs.SetResult(OperationResult.AsFailure("No video devices found, probably running in the simulator"));
				return tcs.Task;
			}

			_videoDeviceInput = AVCaptureDeviceInput.FromDevice(result.Result, out error);

			if (_videoDeviceInput == null)
			{
				_setupResult = CameraSetupResult.SessionConfigurationFailed;
				tcs.SetResult(OperationResult.AsFailure(@"Could not create video device input: {error}"));
				return tcs.Task;
			}

			_session.BeginConfiguration();
			if (_session.CanAddInput(_videoDeviceInput))
			{
				_session.AddInput(_videoDeviceInput);

				var initialVideoOrientation = (AVCaptureVideoOrientation)(long)orientation;
				PreviewLayer.Session = _session;
				PreviewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
				PreviewLayer.Connection.VideoOrientation = initialVideoOrientation;
			}
			else
			{
				_setupResult = CameraSetupResult.SessionConfigurationFailed;
				tcs.SetResult(OperationResult.AsFailure("Could not add video device input to the session"));
				return tcs.Task;
			}

			if (_enableAudioRecording)
			{
				AVCaptureDevice audioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Audio);
				AVCaptureDeviceInput audioDeviceInput = AVCaptureDeviceInput.FromDevice(audioDevice, out error);
				if (audioDeviceInput == null)
				{
					warnings.Add(@"Could not create audio device input: {error}");
				}
				else
				{
					if (_session.CanAddInput(audioDeviceInput))
					{
						_session.AddInput(audioDeviceInput);
					}
					else
					{
						warnings.Add("Could not add audio device input to the session");
					}
				}
			}

			_movieFileOutput = new AVCaptureMovieFileOutput();
			if (_session.CanAddOutput(_movieFileOutput))
			{
				_session.AddOutput(_movieFileOutput);
				AVCaptureConnection connection = _movieFileOutput.ConnectionFromMediaType(AVMediaType.Video);
				if (connection.SupportsVideoStabilization)
				{
					connection.PreferredVideoStabilizationMode = AVCaptureVideoStabilizationMode.Auto;
				}
			}
			else
			{
				warnings.Add("Could not add movie file output to the session");
				_setupResult = CameraSetupResult.SessionConfigurationFailed;
			}

			if (_enableStillImageCapture)
			{
				_stillImageOutput = new AVCaptureStillImageOutput();
				if (_session.CanAddOutput(_stillImageOutput))
				{
					_stillImageOutput.CompressedVideoSetting = new AVVideoSettingsCompressed
					{
						Codec = AVVideoCodec.JPEG
					};
					_session.AddOutput(_stillImageOutput);
				}
				else
				{
					warnings.Add("Could not add still image output to the session");
					_setupResult = CameraSetupResult.SessionConfigurationFailed;
				}
			}

			_session.CommitConfiguration();

			_setupResult = CameraSetupResult.Success;
			tcs.SetResult(OperationResult.AsSuccess(string.Empty, warnings));

			AddObservers();

			return tcs.Task;
		}

		public Task<OperationResult> StartRecording(string filePath)
		{
			TaskCompletionSource<OperationResult> tcs = new TaskCompletionSource<OperationResult>();

			if (_movieFileOutput.Recording)
			{
				tcs.SetResult(OperationResult.AsFailure("Recording already in progress"));
			}
			else
			{
				if (UIDevice.CurrentDevice.IsMultitaskingSupported)
				{
					// Setup background task. This is needed because the IAVCaptureFileOutputRecordingDelegate.FinishedRecording
					// callback is not received until AVCam returns to the foreground unless you request background execution time.
					// This also ensures that there will be time to write the file to the photo library when AVCam is backgrounded.
					// To conclude this background execution, UIApplication.SharedApplication.EndBackgroundTask is called in
					// IAVCaptureFileOutputRecordingDelegate.FinishedRecording after the recorded file has been saved.
					_backgroundRecordingID = UIApplication.SharedApplication.BeginBackgroundTask(null);
				}

				// Update the orientation on the movie file output video connection before starting recording.
				AVCaptureConnection connection = _movieFileOutput.ConnectionFromMediaType(AVMediaType.Video);
				connection.VideoOrientation = PreviewLayer.Connection.VideoOrientation;

				// Turn OFF flash for video recording.
				_videoDeviceInput.Device.SetFlashMode(AVCaptureFlashMode.Off);

				// Start recording to a temporary file.
				_movieFileOutput.StartRecordingToOutputFile(new NSUrl(filePath, false), this);

				tcs.SetResult(OperationResult.AsSuccess());
			}

			return tcs.Task;
		}

		public Task<OperationResult> StopRecording()
		{
			TaskCompletionSource<OperationResult> tcs = new TaskCompletionSource<OperationResult>();

			if (_movieFileOutput.Recording)
			{
				_movieFileOutput.StopRecording();
				tcs.SetResult(OperationResult.AsSuccess());
			}
			else
			{
				tcs.SetResult(OperationResult.AsFailure("Recording is not in progress"));
			}

			return tcs.Task;
		}

		public Task<OperationResult> ResumeInterruptedSession()
		{
			TaskCompletionSource<OperationResult> tcs = new TaskCompletionSource<OperationResult>();


			// The session might fail to start running, e.g., if a phone or FaceTime call is still using audio or video.
			// A failure to start the session running will be communicated via a session runtime error notification.
			// To avoid repeatedly failing to start the session running, we only try to restart the session running in the
			// session runtime error handler if we aren't trying to resume the session running.

			_session.StartRunning();
			IsSessionRunning = _session.Running;

			if (_session.Running)
			{
				tcs.SetResult(OperationResult.AsSuccess());
			}
			else
			{
				tcs.SetResult(OperationResult.AsFailure("Session is not running"));
			}

			return tcs.Task;
		}

		public Task<OperationResult> ChanageCamera()
		{

			TaskCompletionSource<OperationResult> tcs = new TaskCompletionSource<OperationResult>();

			AVCaptureDevice currentVideoDevice = _videoDeviceInput.Device;
			AVCaptureDevicePosition preferredPosition = AVCaptureDevicePosition.Unspecified;
			AVCaptureDevicePosition currentPosition = currentVideoDevice.Position;

			switch (currentPosition)
			{
				case AVCaptureDevicePosition.Unspecified:
				case AVCaptureDevicePosition.Front:
					{
						preferredPosition = AVCaptureDevicePosition.Back;
						break;
					}
				case AVCaptureDevicePosition.Back:
					{
						preferredPosition = AVCaptureDevicePosition.Front;
						break;
					}
			}
			var result = AVCaptureDeviceFactory.CreateDevice(AVMediaType.Video, preferredPosition);
			if (result.IsSuccessful)
			{
				AVCaptureDeviceInput videoDeviceInput = AVCaptureDeviceInput.FromDevice(result.Result);

				if (_videoDeviceInput == null)
				{
					tcs.SetResult(OperationResult.AsFailure(@"Could not create video device input: {error}"));
					return tcs.Task;
				}

				_session.BeginConfiguration();

				// Remove the existing device input first, since using the front and back camera simultaneously is not supported.
				_session.RemoveInput(_videoDeviceInput);

				if (_session.CanAddInput(videoDeviceInput))
				{
					if (_subjectSubscriber != null)
					{
						_subjectSubscriber.Dispose();
					}

					result.Result.SetFlashMode(AVCaptureFlashMode.Auto);
					_subjectSubscriber = NSNotificationCenter.DefaultCenter.AddObserver(AVCaptureDevice.SubjectAreaDidChangeNotification, OnSubjectAreaChangedHandler, result.Result);

					_session.AddInput(videoDeviceInput);
					_videoDeviceInput = videoDeviceInput;
				}
				else
				{
					_session.AddInput(_videoDeviceInput);
				}

				AVCaptureConnection connection = _movieFileOutput.ConnectionFromMediaType(AVMediaType.Video);
				if (connection.SupportsVideoStabilization)
				{
					connection.PreferredVideoStabilizationMode = AVCaptureVideoStabilizationMode.Auto;
				}

				_session.CommitConfiguration();

				tcs.SetResult(OperationResult.AsSuccess());
			}
			else
			{
				tcs.SetResult(OperationResult.AsFailure("Failed to create video device: " + result.Message));
			}

			return tcs.Task;
		}

		public void UpdateOrientation()
		{
			if (PreviewLayer != null)
			{
				var statusBarOrientation = UIApplication.SharedApplication.StatusBarOrientation;
				var avOrientation = (AVCaptureVideoOrientation)(long)statusBarOrientation;

				PreviewLayer.Connection.VideoOrientation = avOrientation;
			}
		}

		public async Task<OperationResult> SnapStillImage()
		{

			AVCaptureConnection connection = _stillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
			var previewLayer = (AVCaptureVideoPreviewLayer)PreviewLayer;

			// Update the orientation on the still image output video connection before capturing.
			connection.VideoOrientation = previewLayer.Connection.VideoOrientation;

			// Flash set to Auto for Still Capture.
			_videoDeviceInput.Device.SetFlashMode(AVCaptureFlashMode.Auto);

			// Capture a still image.
			try
			{
				var imageDataSampleBuffer = await _stillImageOutput.CaptureStillImageTaskAsync(connection);

				// The sample buffer is not retained. Create image data before saving the still image to the photo library asynchronously.
				NSData imageData = AVCaptureStillImageOutput.JpegStillToNSData(imageDataSampleBuffer);

				if (OnImageSnapped != null)
				{
					OnImageSnapped(imageData);
				}

				return OperationResult.AsSuccess();

			}
			catch (NSErrorException ex)
			{
				return OperationResult.AsFailure(ex);
			}
		}

		#endregion

		#region IAVCaptureFileOutputRecordingDelegate

		public void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError error)
		{
			// Note that currentBackgroundRecordingID is used to end the background task associated with this recording.
			// This allows a new recording to be started, associated with a new UIBackgroundTaskIdentifier, once the movie file output's isRecording property
			// is back to NO — which happens sometime after this method returns.
			// Note: Since we use a unique file path for each recording, a new recording will not overwrite a recording currently being saved.

			var currentBackgroundRecordingID = _backgroundRecordingID;
			_backgroundRecordingID = -1;

			Action endBackgroundOperation = () =>
			{
				if (currentBackgroundRecordingID != -1)
				{
					UIApplication.SharedApplication.EndBackgroundTask(currentBackgroundRecordingID);
				}
			};

			bool success = true;
			if (error != null)
			{
				Console.WriteLine("FinishedRecording error: {0}", error);

				var userInfoMessage = error.UserInfo[AVErrorKeys.RecordingSuccessfullyFinished];
				if (userInfoMessage != null)
				{
					success = ((NSNumber)userInfoMessage).BoolValue;
				}
			}

			if (!success)
			{
				endBackgroundOperation();
				return;
			}

			if (OnRecordingFinished != null)
			{
				OnRecordingFinished(outputFileUrl, () => endBackgroundOperation());
			}
		}

		#endregion

		#region AV Events

		private void AddObservers()
		{
			_runningObserver = _session.AddObserver("running", NSKeyValueObservingOptions.New, OnSessionRunningChangedHandler);

			if (_stillImageOutput != null)
			{
				_capturingStillObserver = _stillImageOutput.AddObserver("capturingStillImage", NSKeyValueObservingOptions.New, OnCapturingStillImageChangedHandler);
			}

			_recordingObserver = _movieFileOutput.AddObserver("recording", NSKeyValueObservingOptions.New, OnRecordingChangedHandler);


			_subjectSubscriber = NSNotificationCenter.DefaultCenter.AddObserver(AVCaptureDevice.SubjectAreaDidChangeNotification, OnSubjectAreaChangedHandler, _videoDeviceInput.Device);
			_runtimeErrorObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVCaptureSession.RuntimeErrorNotification, OnSessionRuntimeErrorOccuredHandler, _session);

			// A session can only run when the app is full screen. It will be interrupted in a multi-app layout, introduced in iOS 9.
			// Add observers to handle these session interruptions
			// and show a preview is paused message. See the documentation of AVCaptureSession.WasInterruptedNotification for other
			// interruption reasons.
			_interuptionObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVCaptureSession.WasInterruptedNotification, OnSessionWasInterruptedHandler, _session);
			_interuptionEndedObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVCaptureSession.InterruptionEndedNotification, OnSessionInterruptionEndedHandler, _session);

		}

		private void RemoveObservers()
		{
			_runningObserver?.Dispose();
			_recordingObserver?.Dispose();
			_capturingStillObserver?.Dispose();


			if (_subjectSubscriber != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_subjectSubscriber);
				_subjectSubscriber.Dispose();
			}

			if (_runtimeErrorObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_runtimeErrorObserver);
				_runtimeErrorObserver.Dispose();
			}

			if (_interuptionObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_interuptionObserver);
				_interuptionObserver.Dispose();
			}

			if (_interuptionEndedObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_interuptionEndedObserver);
				_interuptionEndedObserver.Dispose();
			}
		}

		private void OnCapturingStillImageChangedHandler(NSObservedChange change)
		{
			bool isCapturingStillImage = ((NSNumber)change.NewValue).BoolValue;
			if (isCapturingStillImage)
			{
				PreviewLayer.Opacity = 0;
				UIView.Animate(0.25, () =>
				{
					PreviewLayer.Opacity = 1;
				});
			}
		}

		private void OnRecordingChangedHandler(NSObservedChange change)
		{
			bool isRecording = ((NSNumber)change.NewValue).BoolValue;			
			OnRecordingStatusChanged?.Invoke(isRecording);
		}

		private void OnSessionRunningChangedHandler(NSObservedChange change)
		{
			IsSessionRunning = ((NSNumber)change.NewValue).BoolValue;			
			OnSessionRunningChanged?.Invoke(IsSessionRunning);

		}

		private void OnSessionRuntimeErrorOccuredHandler(NSNotification notification)
		{
			var error = (NSError)notification.UserInfo[AVCaptureSession.ErrorKey];
			
			if (!_session.Running)
			{
				_session.StartRunning();
				IsSessionRunning = _session.Running;
			}

			OnSessionErrorOccured?.Invoke(error.Description);
		}

		private void OnSessionWasInterruptedHandler(NSNotification notification)
		{
			var taskId = UIApplication.SharedApplication.BeginBackgroundTask(null);
			
			// In some scenarios we want to enable the user to resume the session running.
			// For example, if music playback is initiated via control center while using AVCam,
			// then the user can let AVCam resume the session running, which will stop music playback.
			// Note that stopping music playback in control center will not automatically resume the session running.
			// Also note that it is not always possible to resume, see ResumeInterruptedSession.
			bool resumePossible = false;
			bool cameraAvailable = true;

			var reason = (AVCaptureSessionInterruptionReason)((NSNumber)notification.UserInfo[AVCaptureSession.InterruptionReasonKey]).Int32Value;

			Console.WriteLine("Capture session was interrupted with reason {0}", reason);

			if (reason == AVCaptureSessionInterruptionReason.AudioDeviceInUseByAnotherClient || reason == AVCaptureSessionInterruptionReason.VideoDeviceInUseByAnotherClient)
			{
				resumePossible = true;
			}
			else if (reason == AVCaptureSessionInterruptionReason.VideoDeviceNotAvailableWithMultipleForegroundApps)
			{
				cameraAvailable = false;
			}


			if (OnSessionWasInterrupted != null)
			{
				OnSessionWasInterrupted(resumePossible, cameraAvailable);
			}

			UIApplication.SharedApplication.EndBackgroundTask(taskId);
		}

		private void OnSessionInterruptionEndedHandler(NSNotification notification)
		{			
			if (OnSessionInterruptionEnded != null)
			{
				OnSessionInterruptionEnded();
			}
		}

		private void OnSubjectAreaChangedHandler(NSNotification notification)
		{
			var devicePoint = new CGPoint(0.5, 0.5);
			_videoDeviceInput.UpdateFocus(AVCaptureFocusMode.ContinuousAutoFocus, AVCaptureExposureMode.ContinuousAutoExposure, devicePoint, false);
		}


		#endregion

	}
}

