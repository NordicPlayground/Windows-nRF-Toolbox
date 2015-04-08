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
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using nRFToolbox.UI;
using nRFToolbox.DataModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace nRFToolbox
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class DeviceFirmwareUpdate : Page
	{
		public NavigationHelper navigationHelper { get; set; }

		public DeviceFirmwareUpdate()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			// register event for the appbar and flyout
			ResolveHeader(e.NavigationParameter.ToString());
			//Register controls used in this page
			RegisterControls();
			//Register framwork elements used in this page
			deviceFirmwareUpdateViewModel = new DeviceFirmwareUpdateViewModel();
			this.DataContext = deviceFirmwareUpdateViewModel;
			RegisterFramworkElements();
		}

		private void RegisterControls()
		{
			 appBarPage = new AppBarControl();
			 deviceSelectionControl = new DeviceSelectionFlyoutControl();
			 errorMessageFlyout = new ErrorMessageFlyout();
		}

		private void ResolveHeader(string pageID)
		{
			var header = this.PageHeader.FindName(ToolboxHeaderLayout.PROFILE_HEADER) as TextBlock;
			header.Text = pageID;
		}

		void RegisterFramworkElements()
		{
			// resolve element in appBar control
			this.BottomAppBar = appBarPage.BottomAppBar;
			this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
			scanButton = (AppBarButton)appBarPage.FindName(AppBarControl.SCAN_DEVICE);
			scanButton.Click += scanButton_Click;
			scanButton.Visibility = Visibility.Visible;
			disconnectButton = (AppBarButton)appBarPage.FindName(AppBarControl.DISCONNECT);
			disconnectButton.Click += DisconnectButton_Clicked;
			settingButton = (AppBarButton)appBarPage.FindName(AppBarControl.TOOLBOX_SETTING);
			settingButton.Click += SettingButton_Clicked;			//resolve elements for device selection control
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			Searching = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.SEARCHING) as TextBlock;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = deviceFirmwareUpdateViewModel.DeviceSelectionViewModel.BleDevices;
			//resolve elements for error messageType control
		}

		private void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			DFUOffModelUI();
			deviceFirmwareUpdateViewModel.StopDFUService();
			deviceFirmwareUpdateViewModel.ClearStatus();
		}

		private void SettingButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (!Frame.Navigate(typeof(Settings), deviceFirmwareUpdateViewModel.PageId))
			{
			}
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{

		}

		private async void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			this.deviceSelectionFlyout.Hide();
			var chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			this.deviceFirmwareUpdateViewModel.DeviceName = chosenBLEDevice.CompleteDeviceName;
			if (chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				DFUStartModelUI();
				if (await deviceFirmwareUpdateViewModel.StartDeviceFirmwareUpdate(chosenBLEDevice))
				{
					await deviceFirmwareUpdateViewModel.UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.START_DFU);
				}
			}
			else
			{
				await deviceFirmwareUpdateViewModel.UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.DEVICE_NOT_CONNECTED);
			}
		}

		void DFUStartModelUI() 
		{
			this.scanButton.Visibility = Visibility.Collapsed;
			this.disconnectButton.Visibility = Visibility.Visible;
			this.SendingProgressBar.Visibility = Visibility.Visible;
		}
		void DFUOffModelUI() 
		{
			this.scanButton.Visibility = Visibility.Visible;
			this.disconnectButton.Visibility = Visibility.Collapsed;
			this.SendingProgressBar.Visibility = Visibility.Collapsed;
		}


		async void scanButton_Click(object sender, RoutedEventArgs e)
		{
			if (!this.deviceFirmwareUpdateViewModel.IsImagesReadyToSend())
			{
				deviceFirmwareUpdateViewModel.ShowSettingErrorMessage();
			}
			else 
			{
				SetDeviceSelectionFlyout();
				await deviceFirmwareUpdateViewModel.UpdateAvailableDevice();
				this.Searching.Visibility = Windows.UI.Xaml.Visibility.Collapsed;			
			}
		}

		private void SetDeviceSelectionFlyout() 
		{
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
		}

		public ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }

		public Flyout deviceSelectionFlyout { get; set; }

		#region NavigationHelper registration

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}

		#endregion

		public AppBarButton scanButton { get; set; }

		public AppBarButton disconnectButton { get; set; }

		public DeviceFirmwareUpdateViewModel deviceFirmwareUpdateViewModel { get; set; }

		public AppBarControl appBarPage { get; set; }

		public DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }

		public ErrorMessageFlyout errorMessageFlyout { get; set; }

		private AppBarButton settingButton { get; set; }

		public TextBlock Searching { get; set; }
	}
}
