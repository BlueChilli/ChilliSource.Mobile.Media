#region License

/*
Licensed to Blue Chilli Technology Pty Ltd and the contributors under the MIT License (the "License").
You may not use this file except in compliance with the License.
See the LICENSE file in the project root for more information.
*/

#endregion

using System;
using AVKit;
using Foundation;
using UIKit;

namespace ChilliSource.Mobile.Media
{
	/// <summary>
	/// Use this class to customize the orientation and rotation options of the default AVPlayerViewController
	/// </summary>
	public class ExtendedAVPlayerViewController : AVPlayerViewController
	{

		public UIInterfaceOrientation PreferredOrientation { get; set; }
		public bool AllowAutorotation { get; set; }

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.Portrait), new NSString("orientation"));
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
		}

		public override bool ShouldAutorotate()
		{
			return AllowAutorotation;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
            return UIInterfaceOrientationMask.AllButUpsideDown;
		}

		public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
		{
			return PreferredOrientation;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Player?.CurrentItem.Dispose();
			}

			base.Dispose(disposing);
		}

	}
}
