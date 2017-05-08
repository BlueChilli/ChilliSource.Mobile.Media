#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using CoreMedia;
using System.Collections.Generic;
using ChilliSource.Mobile.Core;
using System.IO;
using Xamarin.Forms;
using ChilliSource.Mobile.Media;
using CoreVideo;
using System.Drawing;
using ChilliSource.Mobile.UI.Core;

[assembly: Dependency(typeof(VideoService))]

namespace ChilliSource.Mobile.Media
{
	public class VideoService : IVideoService
	{

		private static readonly Lazy<AVPlayer> _player = new Lazy<AVPlayer>(() => new AVPlayer());

		public static AVPlayer Player { get { return _player.Value; } }


		#region Playback

		public OperationResult PlayVideo(string videoPath, bool showInLandscape = false)
		{
			if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
			{
				return OperationResult.AsFailure("Invalid video file path specified");
			}
			var url = NSUrl.CreateFileUrl(videoPath, false, null);
			var item = new AVPlayerItem(url);
			Player.Pause();
			Player.ReplaceCurrentItemWithPlayerItem(item);
			var playerController = new ExtendedAVPlayerViewController();

			playerController.AllowAutorotation = true;
			playerController.PreferredOrientation = showInLandscape ? UIInterfaceOrientation.LandscapeLeft : UIInterfaceOrientation.Portrait;
			playerController.Player = Player;
			playerController.ShowsPlaybackControls = true;

			NavigationService.GetActiveViewController().PresentViewController(playerController, true, () => playerController.Player.Play());
			return OperationResult.AsSuccess();
		}

		#endregion

		#region Video Info

		public OperationResult<double> GetVideoDuration(string filePath)
		{
			var url = NSUrl.CreateFileUrl(filePath, false, null);
			var asset = AVAsset.FromUrl(url);
			return OperationResult<double>.AsSuccess(asset.Duration.Seconds);
		}

		public Orientation GetVideoOrientation(string filePath)
		{
			var url = NSUrl.CreateFileUrl(filePath, false, null);
			var asset = AVAsset.FromUrl(url);
			var tracks = asset.TracksWithMediaType(AVMediaType.Video);
			if (tracks.Length == 0)
			{
				return Orientation.Unknown;
			}

			var track = tracks[0];
			return GetVideoOrientation(track.PreferredTransform);
		}

		#endregion

		#region Video Trimming

		public Task<OperationResult<string>> PresentVideoEditor(string videoPath, double maxDuration)
		{
			var tcs = new TaskCompletionSource<OperationResult<string>>();

			var videoEditor = new UIVideoEditorController();

			videoEditor.VideoPath = videoPath;
			videoEditor.VideoMaximumDuration = maxDuration;
			videoEditor.VideoQuality = UIImagePickerControllerQualityType.High;
			videoEditor.Failed += (object sender, NSErrorEventArgs e) =>
			{
				videoEditor.DismissModalViewController(true);
				tcs.SetResult(OperationResult<string>.AsFailure(e.Error.LocalizedDescription));
			};
			videoEditor.UserCancelled += (object sender, EventArgs e) =>
			{
				videoEditor.DismissModalViewController(true);
				tcs.SetResult(OperationResult<string>.AsCancel());
			};
			videoEditor.Saved += (object sender, UIPathEventArgs e) =>
			{
				tcs.SetResult(OperationResult<string>.AsSuccess(e.Path));
				videoEditor.DismissModalViewController(true);
			};

			NavigationService.GetActiveViewController().PresentViewController(videoEditor, true, null);

			return tcs.Task;
		}

		public async Task<OperationResult> TrimVideo(string sourcePath, string destinationPath, double startTime, double endTime)
		{
			if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
			{
				return OperationResult.AsFailure("Invalid video file path specified");
			}

			var url = NSUrl.CreateFileUrl(sourcePath, false, null);
			var asset = AVAsset.FromUrl(url);

			var session = new AVAssetExportSession(asset, AVAssetExportSession.PresetPassthrough);

			session.OutputUrl = NSUrl.FromFilename(destinationPath);
			session.OutputFileType = AVFileType.Mpeg4;

			var cmStartTime = CMTime.FromSeconds(startTime, asset.Duration.TimeScale);
			var duration = CMTime.FromSeconds(endTime - startTime, asset.Duration.TimeScale);

			var range = new CMTimeRange();
			range.Start = cmStartTime;
			range.Duration = duration;
			session.TimeRange = range;

			await session.ExportTaskAsync();

			if (session.Status == AVAssetExportSessionStatus.Cancelled)
			{
				return OperationResult.AsCancel();
			}
			else if (session.Status == AVAssetExportSessionStatus.Failed)
			{
				return OperationResult.AsFailure(session.Error.LocalizedDescription);
			}
			else
			{
				return OperationResult.AsSuccess();
			}
		}

		#endregion

		#region Video Editing

		public Task<OperationResult> CombineVideoFiles(List<string> filePaths, string outputFilePath,
													   VideoExportQuality quality = VideoExportQuality.High,
													   VideoExportSize exportSizeType = VideoExportSize.R720p,
													   bool exportInLandscape = false, bool exportAudio = false)
		{

			var tcs = new TaskCompletionSource<OperationResult>();

			if (filePaths == null || filePaths.Count == 0 || string.IsNullOrEmpty(outputFilePath))
			{
				tcs.SetResult(OperationResult.AsFailure("Invalid input parameters provided"));
				return tcs.Task;
			}

			CGSize exportSize;
			switch (exportSizeType)
			{
				default:
				case VideoExportSize.R720p:
					{
						exportSize = exportInLandscape ? new CGSize(1280, 720) : new CGSize(720, 1280);
						break;
					}
				case VideoExportSize.R1080p:
					{
						exportSize = exportInLandscape ? new CGSize(1920, 1080) : new CGSize(1080, 1920);
						break;
					}
				case VideoExportSize.R4K:
					{
						exportSize = exportInLandscape ? new CGSize(3840, 2160) : new CGSize(2160, 3840);
						break;
					}
			}


			var composition = AVMutableComposition.Create();
			var videoCompositionTrack = composition.AddMutableTrack(AVMediaType.Video, 0);

			AVMutableCompositionTrack audioCompositionTrack = null;
			if (exportAudio)
			{
				audioCompositionTrack = composition.AddMutableTrack(AVMediaType.Audio, 0);
			}

			CGSize size = CGSize.Empty;
			CMTime videoTime = CMTime.Zero;
			CMTime audioTime = CMTime.Zero;

			var instructions = new List<AVMutableVideoCompositionInstruction>();

			var firstTransform = new CGAffineTransform();

			int index = 0;
			foreach (var filePath in filePaths)
			{
				var url = NSUrl.CreateFileUrl(filePath, false, null);
				var asset = AVAsset.FromUrl(url);

				Console.WriteLine(Path.GetFileName(filePath));
				var tracks = asset.TracksWithMediaType(AVMediaType.Video);
				if (tracks == null || tracks.Length == 0)
				{
					continue;
				}

				var videoAssetTrack = tracks.First();
				var audioAssetTrack = asset.TracksWithMediaType(AVMediaType.Audio).FirstOrDefault();

				//get info from first clip
				if (index == 0)
				{
					firstTransform = videoAssetTrack.PreferredTransform;
					size = videoAssetTrack.NaturalSize;
				}

				var range = new CMTimeRange
				{
					Start = CMTime.Zero,
					Duration = videoAssetTrack.TimeRange.Duration
				};

				NSError error = null;
				videoCompositionTrack.InsertTimeRange(range, videoAssetTrack, videoTime, out error);


				if (error != null)
				{
					Console.WriteLine("Error adding video composition track: " + error.LocalizedDescription);
				}

				if (exportAudio && audioAssetTrack != null)
				{
					audioCompositionTrack.InsertTimeRange(range, audioAssetTrack, audioTime, out error);

					if (error != null)
					{
						Console.WriteLine("Error adding audio composition track: " + error.LocalizedDescription);
					}
				}

				var instruction = AVMutableVideoCompositionInstruction.Create() as AVMutableVideoCompositionInstruction;
				instruction.TimeRange = new CMTimeRange
				{
					Start = videoTime,
					Duration = videoAssetTrack.TimeRange.Duration
				};


				var layer = AVMutableVideoCompositionLayerInstruction.FromAssetTrack(videoCompositionTrack);

				//apply the orientation transform to the layer
				layer.SetTransform(firstTransform, CMTime.Zero);
#if DEBUG
				Console.WriteLine("Transform: " + videoAssetTrack.PreferredTransform.ToString());
				Console.WriteLine("NaturalSize: " + videoAssetTrack.NaturalSize.ToString());
				Console.WriteLine("NaturalTimeScale: " + videoAssetTrack.NaturalTimeScale.ToString());
				Console.WriteLine("Duration: " + videoAssetTrack.TimeRange.Duration.ToString());
#endif
				var trackTransform = videoAssetTrack.PreferredTransform;
				var orientation = GetVideoOrientation(trackTransform);
				var transformedSize = trackTransform.TransformSize(videoAssetTrack.NaturalSize);

#if DEBUG
				Console.WriteLine("Transformed Size: " + transformedSize.ToString());
				Console.WriteLine("==================");
#endif
				transformedSize.Width = (nfloat)Math.Abs(transformedSize.Width);
				transformedSize.Height = (nfloat)Math.Abs(transformedSize.Height);

				CGAffineTransform.MakeIdentity();


				if (orientation == Orientation.Portrait)
				{
					trackTransform.x0 = videoAssetTrack.NaturalSize.Height;
					trackTransform.y0 = 0;
				}
				else if (orientation == Orientation.PortraitUpsideDown)
				{
					trackTransform.x0 = 0;
					trackTransform.y0 = videoAssetTrack.NaturalSize.Width;
				}
				else if (orientation == Orientation.LandscapeLeft)
				{
					trackTransform.x0 = videoAssetTrack.NaturalSize.Width;
					trackTransform.y0 = videoAssetTrack.NaturalSize.Height;
				}
				else if (orientation == Orientation.LandscapeRight)
				{
					trackTransform.x0 = 0;
					trackTransform.y0 = 0;
				}

				if (!exportSize.Equals(transformedSize))
				{
					trackTransform = CGAffineTransform.Scale(trackTransform, exportSize.Width / transformedSize.Width, exportSize.Height / transformedSize.Height);

					if (orientation == Orientation.Portrait)
					{
						trackTransform = CGAffineTransform.Translate(trackTransform, 0, transformedSize.Width / 2);
					}
					else if (orientation == Orientation.LandscapeLeft)
					{
						trackTransform = CGAffineTransform.Translate(trackTransform, transformedSize.Width / 2, transformedSize.Height / 2);
					}
				}

				layer.SetTransform(trackTransform, CMTime.Zero);

				instruction.LayerInstructions = new[] { layer };
				instructions.Add(instruction);


				videoTime = CMTime.Add(videoTime, videoAssetTrack.TimeRange.Duration);

				//don't count the Folktale end tag towards the length of the audio track
				if (index < filePaths.Count - 1)
				{
					audioTime = CMTime.Add(audioTime, audioAssetTrack.TimeRange.Duration);
				}
				index++;
			}

			var videoComposition = AVMutableVideoComposition.Create();
			videoComposition.Instructions = instructions.ToArray();
			videoComposition.FrameDuration = new CMTime(1, 30);

			videoComposition.RenderSize = exportSize;

			string qualitySetting = string.Empty;

			switch (quality)
			{
				default:
				case VideoExportQuality.High:
					{
						qualitySetting = AVAssetExportSession.PresetHighestQuality;
						break;
					}
				case VideoExportQuality.Medium:
					{
						qualitySetting = AVAssetExportSession.PresetMediumQuality;
						break;
					}
				case VideoExportQuality.Low:
					{
						qualitySetting = AVAssetExportSession.PresetLowQuality;
						break;
					}
			}

			var session = new AVAssetExportSession(composition, qualitySetting);
			session.VideoComposition = videoComposition;
			session.OutputUrl = NSUrl.FromFilename(outputFilePath);
			session.OutputFileType = AVFileType.Mpeg4;

			session.ExportAsynchronously(() =>
			{
				if (session.Status == AVAssetExportSessionStatus.Failed)
				{

					Console.WriteLine("Session export error: " + session.Error.LocalizedDescription);
					tcs.SetResult(OperationResult.AsFailure(session.Error.LocalizedDescription));
				}
				else
				{
					tcs.SetResult(OperationResult.AsSuccess());
				}
			});

			return tcs.Task;
		}

		public Task<OperationResult> AddAudioToVideoTrack(string videoFilePath, string audioFilePath, string outputFilePath,
														float volume = 1, float fadeOutDuration = 0)
		{
			var tcs = new TaskCompletionSource<OperationResult>();

			var composition = AVMutableComposition.Create();
			var videoCompositionTrack = composition.AddMutableTrack(AVMediaType.Video, 0);
			var audioCompositionTrack = composition.AddMutableTrack(AVMediaType.Audio, 0);

			var videoUrl = NSUrl.FromFilename(videoFilePath);
			var videoAsset = AVAsset.FromUrl(videoUrl);
			var videoAssetTrack = videoAsset.TracksWithMediaType(AVMediaType.Video).First();

			var audioUrl = NSUrl.FromFilename(audioFilePath);
			var audioAsset = AVAsset.FromUrl(audioUrl);
			var audioAssetTrack = audioAsset.TracksWithMediaType(AVMediaType.Audio).First();

			CGSize size = videoAssetTrack.NaturalSize;
			CMTime time = CMTime.Zero;

			var range = new CMTimeRange
			{
				Start = CMTime.Zero,
				Duration = videoAssetTrack.TimeRange.Duration
			};

			NSError error = null;
			videoCompositionTrack.InsertTimeRange(range, videoAssetTrack, time, out error);
			if (error != null)
			{
				Console.WriteLine("Error adding video composition track: " + error.LocalizedDescription);
			}

			error = null;
			audioCompositionTrack.InsertTimeRange(range, audioAssetTrack, time, out error);
			if (error != null)
			{
				Console.WriteLine("Error adding audio composition track: " + error.LocalizedDescription);
			}


			var audioMix = AVMutableAudioMix.Create();
			var audioInputParams = AVMutableAudioMixInputParameters.FromTrack(audioCompositionTrack);
			audioInputParams.SetVolume(volume, CMTime.Zero);

			if (fadeOutDuration > 0)
			{
				var fadeOutStartTime = CMTime.Subtract(videoAssetTrack.TimeRange.Duration, CMTime.FromSeconds(fadeOutDuration, audioAssetTrack.NaturalTimeScale));
				var fadeOutRange = new CMTimeRange
				{
					Start = fadeOutStartTime,
					Duration = CMTime.FromSeconds(fadeOutDuration, audioAssetTrack.NaturalTimeScale)
				};

				audioInputParams.SetVolumeRamp(volume, 0.0f, fadeOutRange);
			}

			audioMix.InputParameters = new[] { audioInputParams };

			var session = new AVAssetExportSession(composition, AVAssetExportSession.PresetHighestQuality);
			session.OutputUrl = NSUrl.FromFilename(outputFilePath);
			session.OutputFileType = AVFileType.Mpeg4;
			session.AudioMix = audioMix;

			session.ExportAsynchronously(() =>
			{
				if (session.Status == AVAssetExportSessionStatus.Failed)
				{
					Console.WriteLine("Session export error: " + session.Error.LocalizedDescription);
					tcs.SetResult(OperationResult.AsFailure(session.Error.LocalizedDescription));
				}
				else
				{
					tcs.SetResult(OperationResult.AsSuccess());
				}
			});

			return tcs.Task;
		}

		public OperationResult<Stream> GetFirstVideoFrame(string filePath)
		{
			var url = NSUrl.CreateFileUrl(filePath, false, null);
			var asset = new AVUrlAsset(url);
			var imageGenerator = new AVAssetImageGenerator(asset);
			imageGenerator.AppliesPreferredTrackTransform = true;

			CMTime actualTime;
			NSError error;
			var cgImage = imageGenerator.CopyCGImageAtTime(new CMTime(0, 1), out actualTime, out error);

			if (error != null)
			{
				return OperationResult<Stream>.AsFailure(error.ToString());
			}

			if (cgImage != null)
			{
				return OperationResult<Stream>.AsSuccess(new UIImage(cgImage).AsJPEG().AsStream());
			}
			else
			{
				return OperationResult<Stream>.AsFailure("Image generation failed");
			}
		}

		#endregion

		#region Helpers

		private Orientation GetVideoOrientation(CGAffineTransform t)
		{
			if (t.xx == 0 && t.yx == 1 && t.xy == -1 && t.yy == 0)
			{
				return Orientation.Portrait;
			}
			if (t.xx == 0 && t.yx == -1 && t.xy == 1 && t.yy == 0)
			{
				return Orientation.PortraitUpsideDown;
			}
			if (t.xx == 1 && t.yx == 0 && t.xy == 0 && t.yy == 1)
			{
				return Orientation.LandscapeRight;
			}
			if (t.xx == -1 && t.yx == 0 && t.xy == 0 && t.yy == -1)
			{
				return Orientation.LandscapeLeft;
			}

			return Orientation.Unknown;
		}


		#endregion
	}
}

