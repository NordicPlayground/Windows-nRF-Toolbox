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
	public sealed partial class NordicUART : Page
	{
		public NavigationHelper navigationHelper { get; set; }

		public NordicUART()
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
			nordicUARTViewModel = NordicUARTViewModel.GetNordicUartViewModel();
			RegisterFramworkElements();
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			this.DataContext = nordicUARTViewModel;
			var lastDialog = nordicUARTViewModel.Dialog.Last();
			DialogUpdate(lastDialog);
			nordicUARTViewModel.InvalidateDialog += nordicUARTViewModel_InvalidateDialog;
			if(nordicUARTViewModel.chosenDevice != null && 
				nordicUARTViewModel.chosenDevice.ConnectionStatus == BluetoothConnectionStatus.Connected && 
				nordicUARTViewModel.uartService.IsServiceStarted)
			{
				StartUartModelUI();
			}
		}

		void nordicUARTViewModel_InvalidateDialog(UartDialogViewModel message)
		{
			DialogUpdate(message);
		}

		private void DialogUpdate(UartDialogViewModel newItem) 
		{
			this.CommunicationLog.UpdateLayout();
			this.CommunicationLog.ScrollIntoView(newItem);
		}

		void RegisterFramworkElements()
		{
			// resolve element in appBar control
			this.BottomAppBar = appBarPage.BottomAppBar;
			this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
			sendButton = (AppBarButton)appBarPage.FindName(AppBarControl.SEND);
			sendButton.Click += SendClicked;
			speakButton = (AppBarButton)appBarPage.FindName(AppBarControl.SPEAK);
			speakButton.Click += VoiceIconTapped;
			scanButton = (AppBarButton)appBarPage.FindName(AppBarControl.SCAN_DEVICE);
			scanButton.Click += scanButton_Click;
			scanButton.Visibility = Visibility.Visible;
			disconnectButton = (AppBarButton)appBarPage.FindName(AppBarControl.DISCONNECT);
			disconnectButton.Click += DisconnectButton_Clicked;
			settingButton = (AppBarButton)appBarPage.FindName(AppBarControl.TOOLBOX_SETTING);
			settingButton.Click += SettingButton_Clicked;			//resolve elements for device selection control
			Searching = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.SEARCHING) as TextBlock;
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = nordicUARTViewModel.DeviceSelectionViewModel.BleDevices;
			//resolve elements for error messageType control
		}

		async void scanButton_Click(object sender, RoutedEventArgs e)
		{
			SetDeviceSelectionFlyout();
			await nordicUARTViewModel.UpdateAvailableDevice();
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
		}

		private void SettingButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (!Frame.Navigate(typeof(Settings), nordicUARTViewModel.PageId))
			{
			}
		}

		private void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			this.deviceSelectionFlyout.Hide();
			var chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			this.nordicUARTViewModel.DeviceName = chosenBLEDevice.CompleteDeviceName;
			if (chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				StartUartModelUI();
				nordicUARTViewModel.InitializeDevice(chosenBLEDevice);
			}
			else
			{
				// re-connect the device
			}
		}

		void StartUartModelUI()
		{
			this.scanButton.Visibility = Visibility.Collapsed;
			this.disconnectButton.Visibility = Visibility.Visible;
			this.sendButton.Visibility = Visibility.Visible;
			this.speakButton.Visibility = Visibility.Visible;
		}

		void StopUartModelUI() 
		{
			this.scanButton.Visibility = Visibility.Visible;
			this.disconnectButton.Visibility = Visibility.Collapsed;
			this.sendButton.Visibility = Visibility.Collapsed;
			this.speakButton.Visibility = Visibility.Collapsed;
		}

		private void SetDeviceSelectionFlyout()
		{
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
		}

		private void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			this.nordicUARTViewModel.StopUartService();
			nordicUARTViewModel.ClearConversation();
			StopUartModelUI();
		}

		private void ResolveHeader(string pageID)
		{
			var header = this.PageHeader.FindName(ToolboxHeaderLayout.PROFILE_HEADER) as TextBlock;
			header.Text = pageID;
		}

		private void RegisterControls()
		{
			appBarPage = new AppBarControl();
			deviceSelectionControl = new DeviceSelectionFlyoutControl();
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{

		}

		private async void SendClicked(object sender, RoutedEventArgs e)
		{
			var item = await nordicUARTViewModel.TalkTo(UserInput.Text.Trim());
			this.UserInput.Text = string.Empty;
			if (item != null)
				DialogUpdate(item);
		}

		private void UserInputTextboxTapped(object sender, TappedRoutedEventArgs e)
		{
			
		}

		#region NavigationHelper registration

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			//if (e.NavigationMode == NavigationMode.New)
			//{
			//	await RegisterUartVoiceCommand();
			//}
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}

		private async Task RegisterUartVoiceCommand() 
		{
			var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///UartVoiceCommand.xml"));
			await Windows.Media.SpeechRecognition.VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(storageFile);
		}

		private async void VoiceIconTapped(object sender, RoutedEventArgs e)
		{
			// Create an instance of SpeechRecognizer.
			var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

			// Compile the dictation grammar by default.
			await speechRecognizer.CompileConstraintsAsync();

			// Start recognition.
			Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

			// Do something with the recognition result.
			//var messageDialog = new Windows.UI.Popups.MessageDialog(speechRecognitionResult.Text, "Text spoken");
			//await messageDialog.ShowAsync();
			this.UserInput.Text = speechRecognitionResult.Text;
		}

		#endregion

		public AppBarControl appBarPage { get; set; }

		public DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }

		public NordicUARTViewModel nordicUARTViewModel { get; set; }

		public AppBarButton scanButton { get; set; }

		public AppBarButton disconnectButton { get; set; }

		public AppBarButton settingButton { get; set; }

		public Flyout deviceSelectionFlyout { get; set; }

		public ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }

		public CoreDispatcher dispatcher { get; set; }

		public AppBarButton sendButton { get; set; }

		public AppBarButton speakButton { get; set; }

		public TextBlock Searching { get; set; }
	}
}
