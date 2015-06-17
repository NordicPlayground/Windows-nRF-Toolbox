using nRFToolbox.DataModel;
using nRFToolbox.UI;
using nRFToolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
	public sealed partial class BloodPressureMonitor : Page
	{
		public BloodPressureMonitor()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			ResolvePageHeader(e.NavigationParameter.ToString());
			InitialControls();
		}

		private void InitialControls()
		{
			appBarPage = new AppBarControl();
			deviceSelectionControl = new DeviceSelectionFlyoutControl();
			errorMessageFlyout = new ErrorMessageFlyout();
			this.BloodPressureMonitorViewModel = BloodPressureMonitorViewModel.GetBloodPressureMonitorViewModel();
			this.DataContext = BloodPressureMonitorViewModel;
			RegisterDeviceSelectionControl();
			var chosenDevice = BloodPressureMonitorViewModel.chosenDevice;
			if (chosenDevice != null
				&& chosenDevice.ConnectionStatus == BluetoothConnectionStatus.Connected
				&& BloodPressureMonitorViewModel.IsServiceStarted)
			{
				StartedModelUI();
			}
			//if (chosenDevice != null
			//	&& chosenDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected
			//	&& BloodPressureMonitorViewModel.IsServiceStarted)
			//{
			//	//GlucoseMonitorViewModel.StopServices();
			//}
		}

		private void StartedModelUI()
		{
			this.scanButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			this.disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
		}

		void RegisterDeviceSelectionControl()
		{
			this.BottomAppBar = appBarPage.BottomAppBar;
			this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
			scanButton = (AppBarButton)appBarPage.FindName(AppBarControl.SCAN_DEVICE);
			scanButton.Click += scanButton_Click;
			scanButton.Visibility = Visibility.Visible;
			disconnectButton = (AppBarButton)appBarPage.FindName(AppBarControl.DISCONNECT);
			disconnectButton.Click += DisconnectButton_Clicked;
			setting = (AppBarButton)appBarPage.FindName(AppBarControl.TOOLBOX_SETTING);
			setting.Click += SettingButton_Clicked;
			clearButton = (AppBarButton)appBarPage.FindName(AppBarControl.CLEAR);
			clearButton.Click += clearButton_Click;
			
			//resolve elements for device selection control
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			Searching = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.SEARCHING) as TextBlock;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = BloodPressureMonitorViewModel.DeviceSelectionViewModel.BleDevices;
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
		}

		void clearButton_Click(object sender, RoutedEventArgs e)
		{
			this.BloodPressureMonitorViewModel.ClearData();
		}

		private async void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			HideFlyout();
			var chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			this.BloodPressureMonitorViewModel.DeviceName = chosenBLEDevice.CompleteDeviceName;
			this.BloodPressureMonitorViewModel.chosenDevice = chosenBLEDevice;
			if (chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				chosenBLEDevice.Device.ConnectionStatusChanged += Device_ConnectionStatusChanged;
				if (await this.BloodPressureMonitorViewModel.Start())
				{
					//await BloodPressureMonitorViewModel.EnableBloodPressureMeasurementIndication();
					StartedModelUI();
				}
			}
			else
			{
				
			}
		}

		async void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
		{
			
		}

		private void HideFlyout()
		{
			this.deviceSelectionFlyout.Hide();
		}

		private void SettingButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (!Frame.Navigate(typeof(Settings), this.BloodPressureMonitorViewModel.PageId))
			{

			}
		}

		private async void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			await this.BloodPressureMonitorViewModel.StopService();
			this.disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			this.scanButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
		}

		private async void scanButton_Click(object sender, RoutedEventArgs e)
		{
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Visible;
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
			await this.BloodPressureMonitorViewModel.UpdateAvailableDevice();
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
		}

		private void ResolvePageHeader(string pageName)
		{
			var header = this.PageHeader.FindName(ToolboxHeaderLayout.PROFILE_HEADER) as TextBlock;
			header.Text = pageName;
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
			
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}

		public NavigationHelper navigationHelper { get; set; }

		public AppBarControl appBarPage { get; set; }

		public DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }

		public ErrorMessageFlyout errorMessageFlyout { get; set; }

		public BloodPressureMonitorViewModel BloodPressureMonitorViewModel { get; set; }

		public AppBarButton scanButton { get; set; }

		public AppBarButton disconnectButton { get; set; }

		public AppBarButton setting { get; set; }

		public Flyout deviceSelectionFlyout { get; set; }

		public ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }

		public TextBlock Searching { get; set; }

		public AppBarButton clearButton { get; set; }
	}
}
