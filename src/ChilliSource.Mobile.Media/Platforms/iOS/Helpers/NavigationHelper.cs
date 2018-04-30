using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace ChilliSource.Mobile.Media
{
    public static partial class NavigationHelper
    {
        /// <summary>
        /// Returns the top-most presented view controller in the navigation stack
        /// </summary>
        /// <returns>The active view controller.</returns>
        public static UIViewController GetActiveViewController()
        {
            var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }
            return viewController;
        }

    }
}
