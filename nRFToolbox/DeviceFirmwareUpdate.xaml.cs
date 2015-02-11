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
using nRFToolbox.Service;

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
			RegisterFramworkElements();
			//Check Bluetooth Setting
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
			//resolve elements for device selection control
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = deviceFirmwareUpdateViewModel.DeviceSelectionViewModel.BleDevices;
			//resolve elements for error message control
		}
		private async void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.scanButton.Visibility = Visibility.Visible;
				this.disconnectButton.Visibility = Visibility.Collapsed;
			});
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{

		}

		private async void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			this.deviceSelectionFlyout.Hide();
			chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			this.DeviceName.Text = chosenBLEDevice.CompleteDeviceName;
			deviceFirmwareUpdateViewModel.StartDeviceFirmwareUpdate(chosenBLEDevice);
			if (chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					this.scanButton.Visibility = Visibility.Collapsed;
					this.disconnectButton.Visibility = Visibility.Visible;
				});
			}
		}

		private async void AddLogText(string text)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.PageContent.RowDefinitions.Insert(logEventCount, new RowDefinition() { Height = GridLength.Auto});
				TextBlock commandSwitch = new TextBlock() { Text = DateTime.Now.Millisecond.ToString() + " : " + text, Foreground = new SolidColorBrush(Colors.Blue), TextWrapping = TextWrapping.Wrap};
				Grid.SetRow(commandSwitch, logEventCount++);
				this.PageContent.Children.Add(commandSwitch);
			});
		}

		void scanButton_Click(object sender, RoutedEventArgs e)
		{
			SetDeviceSelectionFlyout();
			deviceFirmwareUpdateViewModel.UpdateAvailableDevice();
		}

		private void SetDeviceSelectionFlyout() 
		{
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
		}

		public void ShowBluetoothIsOffMessage()
		{
			var messageDialog = new MessageDialog(deviceFirmwareUpdateViewModel.BluetoothIsOffMessageContent, deviceFirmwareUpdateViewModel.BluetoothIsOffMessageTitle);
			messageDialog.Commands.Add(new UICommand("Go to settings", new UICommandInvokedHandler(GoToBluetoothSettingPage), 0));
			messageDialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1));
			messageDialog.DefaultCommandIndex = 0;
			messageDialog.CancelCommandIndex = 1;
			messageDialog.ShowAsync();
		}

		private void CloseBluetoothIsOffMessage(IUICommand command)
		{ }

		private async void GoToBluetoothSettingPage(IUICommand command)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				await Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
			});
		}

		public ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }

		public Flyout deviceSelectionFlyout { get; set; }

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

		#endregion

		public AppBarButton scanButton { get; set; }

		public AppBarButton disconnectButton { get; set; }

		private int logEventCount = 1;

		public DeviceFirmwareUpdateViewModel deviceFirmwareUpdateViewModel { get; set; }

		public TextBlock flyoutHeaderEnabled { get; set; }

		public Button goToSetting { get; set; }

		public TextBlock flyoutHeaderDisabled { get; set; }

		public AppBarControl appBarPage { get; set; }

		public DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }

		public ErrorMessageFlyout errorMessageFlyout { get; set; }

		public AppBarButton bluetoothSettingButton { get; set; }

		public Task<bool> IsBluetoothOn { get; set; }

		public DeviceInformationItem chosenBLEDevice { get; set; }

		public GattCharacteristic controlPoint { get; set; }

		public GattCharacteristic paket { get; set; }

		public GattCharacteristic DFUVersion { get; set; }

		public int receivedBytes { get; set; }

		public int sendedBytes { get; set; }

		public byte[][] trunks { get; set; }

		public int sendFullPackThisTimes { get; set; }

		public int sendPartialPackThisTimes { get; set; }

		public int sendedTimes = 0;

		public int PiecesPerTime = 10;

		public bool IsFileTransferFinished = false;

		public long startAt { get; set; }

		public long stopAt { get; set; }
	}
}
