#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using AVFoundation;
using Foundation;
using ObjCRuntime;
using UIKit;
using CoreMedia;
using System.IO;

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Use this video to play videos anywhere in your app
	/// </summary>
	/// <example>
	/// Usage example
	/// <code>
	/// var videoPlayer = new VideoPlayerView(muted: false, repeat: true);
	/// videoPlayer.LoadFromPath(videoFilePath);
	/// videoPlayer.Play();
	/// videoPlayer.Pause();
	/// </code>
	/// </example>
	public class VideoPlayerNativeView : UIView
	{
		AVPlayerItem _item;
		AVAsset _asset;
		static Class _layerClass;
		NSObject _playbackEndedObserver;
		bool _isMuted;

		private static readonly Lazy<AVPlayer> _player = new Lazy<AVPlayer>(BuildPlayer);

		#region Lifecycle

		public VideoPlayerNativeView(bool muted = false, bool repeat = false)
		{
			IsMuted = muted;

			PlayerLayer.Player = Player;
			PlayerLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;

			ShouldRepeat = repeat;
			BackgroundColor = UIColor.Black;
			AddUserInteraction();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_playbackEndedObserver != null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_playbackEndedObserver);
					_playbackEndedObserver.Dispose();
				}

				Player?.Pause();
				PlayerLayer.Player = null;
				Player?.ReplaceCurrentItemWithPlayerItem(null);

				_item?.Dispose();
				_item = null;

				_asset?.Dispose();
				_asset = null;

			}
			base.Dispose(disposing);
		}

		#endregion

		#region Properties

		//This property in conjunction with the PlayerLayer property makes the AV layer the main layer of this view, thereby allowing the layer
		//to automatically resize when the view is rotated and therefore providing the smoothest transition.
		public static Class LayerClass
		{
			[Export("layerClass")]
			get
			{
				return _layerClass = _layerClass ?? new Class(typeof(AVPlayerLayer));
			}
		}

		AVPlayerLayer PlayerLayer
		{
			get
			{
				return (AVPlayerLayer)Layer;
			}
		}

		public static AVPlayer Player { get { return _player.Value; } }

		public nfloat VideoPlaybackPosition { get; private set; }

		public bool IsPlaying { get; private set; }

		public double PositionInSeconds
		{
			get
			{
				return Player.CurrentTime.Seconds;
			}
		}

		public bool ShouldRepeat { get; set; }

		public bool IsMuted
		{
			get
			{
				return _isMuted;
			}

			set
			{
				_isMuted = value;
				Player.Muted = _isMuted;
			}
		}

		#endregion

		#region Public Methods

		public void Play()
		{
			if (_player != null)
			{
				Player.Play();
				IsPlaying = !IsPlaying;
			}
		}

		public void Pause()
		{
			if (_player != null)
			{
				Player.Pause();
				IsPlaying = !IsPlaying;
			}
		}

		public void Reset()
		{
			if (_player != null)
			{
				Player.Seek(CMTime.Zero);
			}
		}

		public void SeekToTime(nfloat position)
		{
			if (_player != null)
			{
				CMTime time = new CMTime((long)position, 1);
				Player.Seek(time, toleranceBefore: CMTime.Zero, toleranceAfter: CMTime.Zero);
			}
		}

		public double GetDuration()
		{
			return _asset?.Duration.Seconds ?? 0;
		}

		public void LoadFromPath(string filePath)
		{
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				var url = NSUrl.CreateFileUrl(filePath, false, null);
				LoadFromAsset(AVAsset.FromUrl(url));
			}
		}

		public void LoadFromAsset(AVAsset asset)
		{
			_asset = asset;

			if (_item != null)
			{
				_playbackEndedObserver.Dispose();
				_item.Dispose();
			}

			_item = new AVPlayerItem(asset);
			_playbackEndedObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, HandlePlaybackCompleted, _item);

			Player.Pause();
			Player.ReplaceCurrentItemWithPlayerItem(_item);
		}

		public void LoadFromUrl(NSUrl url)
		{
			LoadFromAsset(AVAsset.FromUrl(url));
		}

		#endregion

		#region Event Handling

		public delegate void PlaybackCompletedEventHandler();
		public event PlaybackCompletedEventHandler OnPlaybackCompleted;

		public delegate void ViewTappedEventHandler();
		public event ViewTappedEventHandler OnViewTapped;

		private void HandlePlaybackCompleted(NSNotification notification)
		{

			if (ShouldRepeat)
			{
				Reset();
				Play();
			}

			OnPlaybackCompleted?.Invoke();
		}

		#endregion

		#region Helper Methods

		private static AVPlayer BuildPlayer()
		{
			var player = new AVPlayer();
			player.Muted = true;
			player.ActionAtItemEnd = AVPlayerActionAtItemEnd.None;
			return player;
		}

		private void AddUserInteraction()
		{
			var tapRecognizer = new UITapGestureRecognizer(() =>
		  {
			  OnViewTapped?.Invoke();
		  });
			tapRecognizer.NumberOfTapsRequired = 1;
			tapRecognizer.NumberOfTouchesRequired = 1;

			UserInteractionEnabled = true;

			AddGestureRecognizer(tapRecognizer);
		}

		#endregion
	}
}

