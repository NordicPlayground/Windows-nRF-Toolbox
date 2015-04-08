using Common.Service;
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
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
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
	public sealed partial class GlucoseMonitor : Page
	{
		private NavigationHelper navigationHelper;
		public GlucoseMonitor()
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

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{

		}

		private async void InitialControls()
		{
			appBarPage = new AppBarControl();
			deviceSelectionControl = new DeviceSelectionFlyoutControl();
			errorMessageFlyout = new ErrorMessageFlyout();
			this.GlucoseMonitorViewModel = GlucoseMonitorViewModel.GetGlucoseMonitorViewModel();
			this.DataContext = GlucoseMonitorViewModel;
			RegisterDeviceSelectionControl();
			var chosenDevice = GlucoseMonitorViewModel.chosenDevice;
			if(chosenDevice != null
				&& chosenDevice.ConnectionStatus == BluetoothConnectionStatus.Connected
				&& GlucoseMonitorViewModel.IsServiceStarted)
			{
				await StartedModelUI();
			}
			if(chosenDevice != null 
				&& chosenDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected
				&& GlucoseMonitorViewModel.IsServiceStarted)
			{
				GlucoseMonitorViewModel.StopServices();
			}
		}

		private void ResolvePageHeader(string pageName)
		{
			var header = this.PageHeader.FindName(ToolboxHeaderLayout.PROFILE_HEADER) as TextBlock;
			header.Text = pageName;
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
			getAllRecordButton = (AppBarButton)appBarPage.FindName(AppBarControl.SAVE_LOCAL);
			getAllRecordButton.Click += GetAllButton_Clicked;
			clearButton = (AppBarButton)appBarPage.FindName(AppBarControl.CLEAR);
			clearButton.Click += clearButton_Clicked;
			//resolve elements for device selection control
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			Searching = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.SEARCHING) as TextBlock;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = GlucoseMonitorViewModel.DeviceSelectionViewModel.BleDevices;
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
		}

		private async void clearButton_Clicked(object sender, RoutedEventArgs e)
		{
			this.clearButton.IsEnabled = false;
			await this.GlucoseMonitorViewModel.DeleteAllStoredRecords();
			this.clearButton.IsEnabled = true;
		}

		private async void GetAllButton_Clicked(object sender, RoutedEventArgs e)
		{
			this.getAllRecordButton.IsEnabled = false;
			await this.GlucoseMonitorViewModel.GetAllStoredRecords();
			this.getAllRecordButton.IsEnabled = true;
		}

		private void SettingButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (!Frame.Navigate(typeof(Settings), this.GlucoseMonitorViewModel.PageId))
			{

			}
		}

		async void scanButton_Click(object sender, RoutedEventArgs e)
		{
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Visible;
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
			await this.GlucoseMonitorViewModel.UpdateAvailableDevice();
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
		}

		private async void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			await SwitchToDisconnectStatus();
		}

		private async Task SwitchToDisconnectStatus() 
		{
			this.GlucoseMonitorViewModel.StopServices();
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.scanButton.Visibility = Visibility.Visible;
				this.disconnectButton.Visibility = Visibility.Collapsed;
				this.clearButton.Visibility = Visibility.Collapsed;
				this.getAllRecordButton.Visibility = Visibility.Collapsed;
			});
		}

		private async void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			HideFlyout();
			var chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			this.GlucoseMonitorViewModel.DeviceName = chosenBLEDevice.CompleteDeviceName;
			this.GlucoseMonitorViewModel.chosenDevice = chosenBLEDevice;
			if (chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				chosenBLEDevice.Device.ConnectionStatusChanged += Device_ConnectionStatusChanged;
				if (await this.GlucoseMonitorViewModel.Start(chosenBLEDevice.Services))
					await StartedModelUI();
			}
			else
			{
				
			}
		}

		async void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
		{
			await ConnectionStatusChangedUI();
		}


		private async void GetFirstRecordClicked(object sender, RoutedEventArgs e)
		{
			this.GetFirstRecordButton.IsEnabled = false;
			await this.GlucoseMonitorViewModel.GetFirstRecords();
			this.GetFirstRecordButton.IsEnabled = true;
		}

		private async void GetLastRecordClicked(object sender, RoutedEventArgs e)
		{
			this.GetLastRecordButton.IsEnabled = false;
			await this.GlucoseMonitorViewModel.GetLastRecords();
			this.GetLastRecordButton.IsEnabled = true;
		}

		private async Task ConnectionStatusChangedUI() 
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.scanButton.Visibility = Visibility.Collapsed;
				this.disconnectButton.Visibility = Visibility.Visible;
				this.clearButton.Visibility = Visibility.Collapsed;
				this.getAllRecordButton.Visibility = Visibility.Collapsed;
			});
		}

		private async Task StartedModelUI() 
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.scanButton.Visibility = Visibility.Collapsed;
				this.disconnectButton.Visibility = Visibility.Visible;
				this.clearButton.Visibility = Visibility.Visible;
				this.getAllRecordButton.Visibility = Visibility.Visible;
			});
		}

		void HideFlyout()
		{
			this.deviceSelectionFlyout.Hide();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}


		private AppBarControl appBarPage { get; set; }

		private DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }

		private ErrorMessageFlyout errorMessageFlyout { get; set; }

		private AppBarButton scanButton { get; set; }

		private TextBlock Searching { get; set; }

		private AppBarButton disconnectButton { get; set; }

		private AppBarButton setting { get; set; }

		private Flyout deviceSelectionFlyout { get; set; }

		private ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }

		public GlucoseMonitorViewModel GlucoseMonitorViewModel { get; set; }

		public AppBarButton getAllRecordButton { get; set; }

		public AppBarButton clearButton { get; set; }
	}
}
