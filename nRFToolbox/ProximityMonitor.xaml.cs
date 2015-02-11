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
using nRFToolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Phone.UI.Input;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using nRFToolbox.UI;
using nRFToolbox.Service.GattService;
using nRFToolbox.DataModel;

namespace nRFToolbox
{
	public sealed partial class ProximityMonitor : Page
	{
		public NavigationHelper navigationHelper { get; set; }
		public ProximityMonitor()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
				frame.GoBack();
				e.Handled = true;
			}
		}

		private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			ResolvePageHeader(e.NavigationParameter.ToString());
			this.proximityViewModel = ProximityViewModel.GetInstance();
			InitialControls();
			if (await proximityViewModel.TryRetrieveServices())
				ChangeScanToStopButton();
			this.DataContext = proximityViewModel;
		}

		private void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			this.deviceSelectionFlyout.Hide();
			var chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			this.proximityViewModel.DeviceName = chosenBLEDevice.CompleteDeviceName;
			this.proximityViewModel.StartServicesOnDevice(chosenBLEDevice);
			chosenBLEDevice.Device.ConnectionStatusChanged += Device_ConnectionStatusChanged;
			if(chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
				ChangeScanToStopButton();
		}

	   void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
		{
			if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				 ChangeScanToStopButton();
			}
		}

		async void ChangeScanToStopButton() 
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.scanButton.Visibility = Visibility.Collapsed;
				this.disconnectButton.Visibility = Visibility.Visible;
				HideProximityUnlockPicture();
			});
		}

		void HideProximityUnlockPicture() 
		{
			this.ProximityUnlocked.Visibility = Visibility.Collapsed;
			this.Proximitylocked.Visibility = Visibility.Visible;
		}

		void ShowProximityUnlockPicture() 
		{
			this.ProximityUnlocked.Visibility = Visibility.Visible;
			this.Proximitylocked.Visibility = Visibility.Collapsed;
		}

		async void scanButton_Click(object sender, RoutedEventArgs e)
		{
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Visible;
			SetDeviceSelectionFlyout();
			await proximityViewModel.UpdateAvailableDevice();
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
		}

		private async void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			this.proximityViewModel.StopServices();
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.scanButton.Visibility = Visibility.Visible;
				this.disconnectButton.Visibility = Visibility.Collapsed;
				this.proximityViewModel.DeviceName = ProximityViewModel.DEFAULT_DEVICE;
				ShowProximityUnlockPicture();
			});
		}

		private void SetDeviceSelectionFlyout()
		{
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		#region Page Initialization
		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event receivedBytes that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}

		public ProximityViewModel proximityViewModel { get; set; }

		public AppBarControl appBarPage { get; set; }

		public DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }

		public ErrorMessageFlyout errorMessageFlyout { get; set; }

		public AppBarButton scanButton { get; set; }

		public AppBarButton disconnectButton { get; set; }

		public Flyout deviceSelectionFlyout { get; set; }

		public ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }

		async void RegisterDeviceSelectionControl()
		{
			this.BottomAppBar = appBarPage.BottomAppBar;
			this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
			scanButton = (AppBarButton)appBarPage.FindName(AppBarControl.SCAN_DEVICE);
			scanButton.Click += scanButton_Click;
			scanButton.Visibility = Visibility.Visible;
			disconnectButton = (AppBarButton)appBarPage.FindName(AppBarControl.DISCONNECT);
			disconnectButton.Click += DisconnectButton_Clicked;
			AboutSettingButton = (AppBarButton)appBarPage.FindName(AppBarControl.TOOLBOX_SETTING);
			AboutSettingButton.Click += AboutButton_Click;
			//resolve elements for device selection control
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			Searching = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.SEARCHING) as TextBlock;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = proximityViewModel.DeviceSelectionViewModel.BleDevices;
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			if(!Frame.Navigate(typeof(Settings), proximityViewModel.PageId))
			{
			}
		}

		private void ResolvePageHeader(string pageName)
		{
			var header = this.PageHeader.FindName(ToolboxHeaderLayout.PROFILE_HEADER) as TextBlock;
			header.Text = pageName;
		}

		private void InitialControls()
		{
			appBarPage = new AppBarControl();
			deviceSelectionControl = new DeviceSelectionFlyoutControl();
			errorMessageFlyout = new ErrorMessageFlyout();
			RegisterDeviceSelectionControl();
		}
		#endregion

		public TextBlock Searching { get; set; }

		public AppBarButton AboutSettingButton { get; set; }
	}
}
