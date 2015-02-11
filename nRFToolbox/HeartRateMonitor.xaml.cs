/*Copyright (c) 2015, Nordic Semiconductor ASA
 *
 *Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *
 *1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *
 *2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other 
 *materials provided with the distribution.
 *
 *3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific 
 *prior written permission.
 *
 *THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 *PURPOSE ARE DISCLAIMED. *IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF *SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, *DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 *ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED *OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using nRFToolbox.DataModel;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using Windows.UI.Popups;
using Windows.System;
using Windows.Phone.UI.Input;
using Windows.ApplicationModel.Background;
using nRFToolbox.ViewModels;
using nRFToolbox.UI;
using nRFToolbox.Base;
using nRFToolbox.Service.GattService;
using nRFToolbox.Service;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace nRFToolbox
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class HeartRateMonitor : Page
	{
		private NavigationHelper navigationHelper;
		HeartRateMonitorViewModel heartRateMonitorViewModel { get; set; }

		public HeartRateMonitor()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
			Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);
		}

		/// <summary>
		/// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
		/// </summary>
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
		}

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="sender">
		/// The source of the event; typically <see cref="NavigationHelper"/>
		/// </param>
		/// <param name="e">Event receivedBytes that provides both the navigation parameter passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
		/// a dictionary of state preserved by this page during an earlier
		/// session.  The state will be null the first time a page is visited.</param>
		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			ResolvePageHeader(e.NavigationParameter.ToString());
			this.heartRateMonitorViewModel = new HeartRateMonitorViewModel();
			this.DataContext = heartRateMonitorViewModel;
			InitialControls();
		}

		private void InitialControls()
		{
			appBarPage = new AppBarControl();
			deviceSelectionControl = new DeviceSelectionFlyoutControl();
			errorMessageFlyout = new ErrorMessageFlyout();
			RegisterDeviceSelectionControl();
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
			//resolve elements for device selection control
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			Searching = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.SEARCHING) as TextBlock;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = heartRateMonitorViewModel.DeviceSelectionViewModel.BleDevices;
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
		}

		private void SettingButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (!Frame.Navigate(typeof(Settings), heartRateMonitorViewModel.PageId))
			{
			}
		}

		private async void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			HideFlyout();
			var chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			heartRateMonitorViewModel.DeviceName = chosenBLEDevice.CompleteDeviceName;
			chosenBLEDevice.Device.ConnectionStatusChanged += Device_ConnectionStatusChanged;
			heartRateMonitorViewModel.StartHeartRateMonitor(chosenBLEDevice);
			if (chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					this.scanButton.Visibility = Visibility.Collapsed;
					this.disconnectButton.Visibility = Visibility.Visible;
				});
			}
			await Task.Delay(TimeSpan.FromSeconds(3));
			RegisterLineChartTimer();
		}

		void Device_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		void HideFlyout() 
		{
			this.deviceSelectionFlyout.Hide();
		}

	   async void scanButton_Click(object sender, RoutedEventArgs e)
		{
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Visible;
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
			await heartRateMonitorViewModel.UpdateAvailableDevice();
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
		}

		private async void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
		{
			if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					this.scanButton.Visibility = Visibility.Collapsed;
					this.disconnectButton.Visibility = Visibility.Visible;
				});
			}
		}

		private async void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			heartRateMonitorViewModel.StopServices();
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.scanButton.Visibility = Visibility.Visible;
				this.disconnectButton.Visibility = Visibility.Collapsed;
			});
		}

		#region Flyout component
		AppBarButton scanButton { get; set; }
	   AppBarButton disconnectButton { get; set; }
		Flyout deviceSelectionFlyout;
		ListView availableDevicesList { get; set; }
		Chart hearRatelineChart { get; set; }
		#endregion

		/// <summary>
		/// Preserves state associated with this page in case the application is suspended or the
		/// page is discarded from the navigation cache.  Values must conform to the serialization
		/// requirements of <see cref="SuspensionManager.SessionState"/>.
		/// </summary>
		/// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
		/// <param name="e">Event receivedBytes that provides an empty dictionary to be populated with
		/// serializable state.</param>
		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		#region NavigationHelper registration

		/// <summary>
		/// The methods provided in this section are simply used to allow
		/// NavigationHelper to respond to the page's navigation methods.
		/// <para>
		/// Page specific logic should be placed in event handlers for the  
		/// <see cref="NavigationHelper.LoadState"/>
		/// and <see cref="NavigationHelper.SaveState"/>.
		/// The navigation parameter is available in the LoadState method 
		/// in addition to page state preserved during an earlier session.
		/// </para>
		/// </summary>
		/// <param name="e">Provides receivedBytes for navigation methods and event
		/// handlers that cannot cancel the navigation request.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}

		private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			if (frame == null)
			{
				return;
			}

			if (frame.CanGoBack)
			{
				heartRateMonitorViewModel.StopServices();
				frame.GoBack();
				e.Handled = true;
			}
		}

		#endregion

		private void RegisterLineChartTimer()
		{
			dispatcherTimer = new SmartDispatcherTimer();
			dispatcherTimer.IsReentrant = false;
			dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
			dispatcherTimer.TickTask = () =>
				{
					UpdateGUI();
				};
		  dispatcherTimer.Start();
		}

		private void UpdateGUI()
		{
			HeartRateMeasurementCharacteristic data;
			if(this.heartRateMonitorViewModel.heartRateService.HeartRatioQueue.TryDequeue(out data))
			{
				this.heartRateMonitorViewModel.AddBMPValue(data.BeatPerMinute);
			}
		}

		public SmartDispatcherTimer dispatcherTimer { get; set; }

		public AppBarControl appBarPage { get; set; }

		public DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }

		public ErrorMessageFlyout errorMessageFlyout { get; set; }

		public ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }
	   void App_Suspending(
		 Object sender,
		 Windows.ApplicationModel.SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			deferral.Complete();
		}

		public TextBlock Searching { get; set; }

		public AppBarButton setting { get; set; }
	}
}
