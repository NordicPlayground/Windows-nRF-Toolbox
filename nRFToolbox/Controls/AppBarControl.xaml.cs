using nRFToolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace nRFToolbox
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public partial class AppBarControl : Page
	{
		public const string SCAN_DEVICE = "ScanDevices";
		public const string DISCONNECT = "Disconnect";
		public const string ABOUT_BUTTON = "About";
		//public const string DEVICE_SELECTION_FLYOUT = "DeviceSelectionFlyout";
		public const string TOOLBOX_SETTING = "Setting";
		//public const string FLYOUT_HEADER_ENABLED = "FlyoutHeaderBluetoothEnabled";
		//public const string FLYOUT_HEADER_DISENABLED = "FlyoutHeaderBluetoothEnabled";

		public AppBarControl()
		{
			this.InitializeComponent();
		}
	}
}
