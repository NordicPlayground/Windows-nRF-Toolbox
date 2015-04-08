using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace nRFToolbox
{
	public sealed partial class ErrorMessageFlyout : UserControl
	{
		public const string ERROR_MESSAGE_FLYOUT = "ErrorMessageFlyout";
		public const string CANNOT_SCAN_DEVICE = "Can't scan devices";
		public const string GO_TO_SETTING = "GoToSettingButton";
		public ErrorMessageFlyout()
		{
			this.InitializeComponent();
		}
	}
}
