using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace nRFToolbox
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ExtentedSplashScreen : Page
	{
		internal Rect splashImageRect; // Rect to store splash screen image coordinates.
		private ExtentedSplashScreen splash; // Variable to hold the splash screen object.
		internal bool dismissed = false; // Variable to track splash screen dismissal status.
		internal Frame rootFrame;

		public ExtentedSplashScreen(SplashScreen splashscreen, bool loadState)
		{
			//this.InitializeComponent();
			//Window.Current.SizeChanged += new WindowSizeChangedEventHandler(ExtendedSplash_OnResize);

			//splash = splashscreen;
			//if (splash != null)
			//{
			//	// Register an event handler to be executed when the splash screen has been dismissed.
			//	splash.Dismissed += new TypedEventHandler<SplashScreen, Object>(DismissedEventHandler);

			//	// Retrieve the window coordinates of the splash screen image.
			//	splashImageRect = splash.ImageLocation;
			//	PositionImage();

			//	// If applicable, include a method for positioning a progress control.
			//	PositionRing();
			//}

			//// Create a Frame to act as the navigation context 
			//rootFrame = new Frame();

			//// Restore the saved session state if necessary
			//RestoreStateAsync(loadState);
		}

		//void PositionImage()
		//{
		//	extendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
		//	extendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
		//	extendedSplashImage.Height = splashImageRect.Height;
		//	extendedSplashImage.Width = splashImageRect.Width;
		//}

		//void DismissedEventHandler(SplashScreen sender, object e)
		//{
		//	dismissed = true;

		//	// Complete app setup operations here...
		//}

		//void DismissExtendedSplash()
		//{
		//	// Navigate to mainpage
		//	rootFrame.Navigate(typeof(MainPage));
		//	// Place the frame in the current Window
		//	Window.Current.Content = rootFrame;
		//}

		//void ExtendedSplash_OnResize(Object sender, WindowSizeChangedEventArgs e)
		//{
		//	// Safely update the extended splash screen image coordinates. This function will be executed when a user resizes the window.
		//	if (splash != null)
		//	{
		//		// Update the coordinates of the splash screen image.
		//		splashImageRect = splash.ImageLocation;
		//		PositionImage();

		//		// If applicable, include a method for positioning a progress control.
		//		// PositionRing();
		//	}
		//}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
		}
	}
}
