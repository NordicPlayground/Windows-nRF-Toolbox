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
using Windows.UI;
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
	public sealed partial class NordicUartButtonControlPanel : Page
	{
		public NavigationHelper navigationHelper { get; set; }
		public NordicUartButtonControlPanel()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			// register event for the appbar and flyout
			ResolveHeader(e.NavigationParameter.ToString());
			//Register controls used in this page
			RegisterControls();
			//Register framwork elements used in this page
			nordicUartControlButtonViewModel = NordicUartControlButtonViewModel.GetNordicUartControlButtonViewModel();
			RegisterFramworkElements();
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			this.DataContext = nordicUartControlButtonViewModel;
			if(this.nordicUartControlButtonViewModel.Dialog.Count != 0)
			{
				var lastDialog = this.nordicUartControlButtonViewModel.Dialog.Last();
				DialogUpdate(lastDialog);
			}
			this.nordicUartControlButtonViewModel.InvalidateDialog += nordicUARTViewModel_InvalidateDialog;
			if (nordicUartControlButtonViewModel.chosenDevice != null &&
				nordicUartControlButtonViewModel.chosenDevice.ConnectionStatus == BluetoothConnectionStatus.Connected &&
				nordicUartControlButtonViewModel.uartService.IsServiceStarted)
			{
				StartUartModelUI();
			}
		}

		void StartUartModelUI()
		{
			this.scanButton.Visibility = Visibility.Collapsed;
			this.UserInput.Text = string.Empty;
			this.UserInput.IsEnabled = true;
			this.editButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			if (this.nordicUartControlButtonViewModel.IsOnEditModel)
			{
				this.editButton.Icon = new SymbolIcon(Symbol.Save);
				this.clearButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			}
			else
			{
				this.editButton.Icon = new SymbolIcon(Symbol.Edit);
				this.disconnectButton.Visibility = Visibility.Visible;
			}
		}

		void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
		{
			if(sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
			{
				this.nordicUartControlButtonViewModel.StopUartService();
				StopUartModelUI();
			}
		}

		private void DialogUpdate(UartDialogViewModel newItem)
		{
			this.CommunicationLog.UpdateLayout();
			this.CommunicationLog.ScrollIntoView(newItem);
		}

		void nordicUARTViewModel_InvalidateDialog(UartDialogViewModel message)
		{
			DialogUpdate(message);
		}

		private void ResolveHeader(string pageID)
		{
			//var header = this.PageHeader.FindName(ToolboxHeaderLayout.PROFILE_HEADER) as TextBlock;
			//header.Text = pageID;
		}

		private void RegisterControls()
		{
			appBarPage = new AppBarControl();
			deviceSelectionControl = new DeviceSelectionFlyoutControl();
			buttonSelectionControl = new ControlButtonSelectionFlyoutControl();
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
			speakButton = (AppBarButton)appBarPage.FindName(AppBarControl.SPEAK);
			speakButton.Click += VoiceIconTapped;
			editButton = (AppBarButton)appBarPage.FindName(AppBarControl.EDIT);
			editButton.Click += editButton_Click;
			settingButton = (AppBarButton)appBarPage.FindName(AppBarControl.TOOLBOX_SETTING);
			settingButton.Click += SettingButton_Clicked;			//resolve elements for device selection control
			//clear
			clearButton = (AppBarButton)appBarPage.FindName(AppBarControl.CLEAR);
			clearButton.Click += clearButton_Click;
			Searching = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.SEARCHING) as TextBlock;
			deviceSelectionFlyout = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.DEVICE_SELECTION_FLYOUT) as Flyout;
			availableDevicesListInDeviceSelectionFlyoutContent = deviceSelectionControl.FindName(DeviceSelectionFlyoutControl.AVAILABLE_DEVICE_LIST) as ListView;
			availableDevicesListInDeviceSelectionFlyoutContent.IsItemClickEnabled = true;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemClick += Device_Clicked;
			availableDevicesListInDeviceSelectionFlyoutContent.ItemsSource = this.nordicUartControlButtonViewModel.DeviceSelectionViewModel.BleDevices;
			//resolve elements for error messageType control
			buttonSelectionFlyout = buttonSelectionControl.FindName(ControlButtonSelectionFlyoutControl.BUTTON_SELECTION_FLYOUT) as Flyout;
			availableButtonsGridView = buttonSelectionControl.FindName(ControlButtonSelectionFlyoutControl.AVAILABLE_BUTTON) as GridView;
			availableButtonsGridView.IsItemClickEnabled = true;
			availableButtonsGridView.ItemClick += availableButtonsGridView_ItemClick;
			availableButtonsGridView.ItemsSource = this.nordicUartControlButtonViewModel.AllControlButtons;
			ChooseAvailableButton = buttonSelectionControl.FindName(ControlButtonSelectionFlyoutControl.OK_BUTTON) as Button;
			ChooseAvailableButton.Click += ChooseAvailableOkButton_Click;
			CommandTextbox = buttonSelectionControl.FindName(ControlButtonSelectionFlyoutControl.COMMAND_TEXTBOX) as TextBox;
		}

		void clearButton_Click(object sender, RoutedEventArgs e)
		{
			this.nordicUartControlButtonViewModel.ClearSelectedControlButtons();
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

		void ChooseAvailableOkButton_Click(object sender, RoutedEventArgs e)
		{
			this.buttonSelectionFlyout.Hide();
			this.editButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			if(this.nordicUartControlButtonViewModel.SelectedAvailableButton != null)
			{
				var userCommand = CommandTextbox.Text.Trim();
				this.nordicUartControlButtonViewModel.SelectedAvailableButton.Title = userCommand;
				this.nordicUartControlButtonViewModel.UpdateSelectedControlButton();
				ClearUserChoice();
			}
		}

		private void ClearUserChoice()
		{
			CommandTextbox.Text = string.Empty;
			this.nordicUartControlButtonViewModel.ClearUserChoice();
		}

		void availableButtonsGridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var selectedItem = e.ClickedItem as ControlButtonViewModel;
			this.nordicUartControlButtonViewModel.ChangeColorOnSelectedItem(selectedItem.ButtonId);
			this.nordicUartControlButtonViewModel.SelectedAvailableButton = selectedItem;
		}

		void editButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.nordicUartControlButtonViewModel.IsOnEditModel)
			{
				this.nordicUartControlButtonViewModel.TurnOnOffEditModel(false);
				this.disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
				this.clearButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				this.editButton.Icon = new SymbolIcon(Symbol.Edit);
			}
			else
			{
				this.nordicUartControlButtonViewModel.TurnOnOffEditModel(true);
				this.disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				this.clearButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
				this.editButton.Icon = new SymbolIcon(Symbol.Save);
			}
		}

		private void Device_Clicked(object sender, ItemClickEventArgs e)
		{
			this.deviceSelectionFlyout.Hide();
			var chosenBLEDevice = e.ClickedItem as DeviceInformationItem;
			this.nordicUartControlButtonViewModel.DeviceName = chosenBLEDevice.CompleteDeviceName;
			if (chosenBLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
			{
				StartUartModelUI();
				this.nordicUartControlButtonViewModel.InitializeDevice(chosenBLEDevice);
				this.nordicUartControlButtonViewModel.chosenDevice.Device.ConnectionStatusChanged += Device_ConnectionStatusChanged;
			}
			else
			{
				// re-connect the device
			}
		}

		private void SettingButton_Clicked(object sender, RoutedEventArgs e)
		{
			if (!Frame.Navigate(typeof(Settings), this.nordicUartControlButtonViewModel.PageId))
			{
			}
		}

		private void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			this.nordicUartControlButtonViewModel.StopUartService();
			StopUartModelUI();
		}

		async void StopUartModelUI()
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.nordicUartControlButtonViewModel.ClearConversation();
				this.scanButton.Visibility = Visibility.Visible;
				this.disconnectButton.Visibility = Visibility.Collapsed;
				this.speakButton.Visibility = Visibility.Collapsed;
				this.DeviceName.Text = NordicUartControlButtonViewModel.DEFAULT_DEVICE;
				this.UserInput.Text = NordicUartControlButtonViewModel.DISCONNECTED;
				this.UserInput.IsEnabled = false;
				this.editButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				this.editButton.Icon = new SymbolIcon(Symbol.Edit);
				this.clearButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				this.nordicUartControlButtonViewModel.TurnOnOffEditModel(false);
			});
		}

		private async void scanButton_Click(object sender, RoutedEventArgs e)
		{
			SetDeviceSelectionFlyout();
			await this.nordicUartControlButtonViewModel.UpdateAvailableDevice();
			this.Searching.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
		}

		private void SetDeviceSelectionFlyout()
		{
			FlyoutBase.SetAttachedFlyout(this.scanButton, deviceSelectionFlyout);
			FlyoutBase.ShowAttachedFlyout(this.scanButton);
		}

		private void ChooseIcon(object sender, TappedRoutedEventArgs e)
		{

		}

		#region NavigationHelper registration

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
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

		#endregion

		public NordicUartControlButtonViewModel nordicUartControlButtonViewModel { get; set; }

		public AppBarControl appBarPage { get; set; }
		public DeviceSelectionFlyoutControl deviceSelectionControl { get; set; }


		public AppBarButton scanButton { get; set; }

		public AppBarButton disconnectButton { get; set; }

		public AppBarButton settingButton { get; set; }

		public TextBlock Searching { get; set; }

		public Flyout deviceSelectionFlyout { get; set; }

		public ListView availableDevicesListInDeviceSelectionFlyoutContent { get; set; }

		public CoreDispatcher dispatcher { get; set; }

		public AppBarButton editButton { get; set; }

		private async void ControlButtonPanelClicked(object sender, ItemClickEventArgs e)
		{
			var clickedItem = e.ClickedItem as ControlButtonViewModel;
			this.nordicUartControlButtonViewModel.ButtonInPanel = clickedItem;
			if(this.nordicUartControlButtonViewModel.IsOnEditModel)
			{
				// flyout choose button and edit
				FlyoutBase.SetAttachedFlyout(this.editButton, buttonSelectionFlyout);
				FlyoutBase.ShowAttachedFlyout(this.editButton);
				this.editButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				this.disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				this.CommandTextbox.Text = clickedItem.Title;
				if(clickedItem.ButtonId != string.Empty)
				{
					this.nordicUartControlButtonViewModel.ChangeColorOnSelectedItem(clickedItem.ButtonId);
					this.nordicUartControlButtonViewModel.SelectedAvailableButton = clickedItem;
				}
			}
			else
			{
				//send the commands
				var item = await this.nordicUartControlButtonViewModel.TalkTo(clickedItem.Title.Trim());
				if (item != null)
					DialogUpdate(item);
			}
		}

		public ControlButtonSelectionFlyoutControl buttonSelectionControl { get; set; }

		public Flyout buttonSelectionFlyout { get; set; }

		public GridView availableButtonsGridView { get; set; }

		public Button ChooseAvailableButton { get; set; }

		public TextBox CommandTextbox { get; set; }

		private void ButtonControlPanelLoaded(object sender, RoutedEventArgs e)
		{

		}

		private void VoiceControlPanelLoaded(object sender, RoutedEventArgs e)
		{

		}

		private async void SendButtonClicked(object sender, RoutedEventArgs e)
		{
			var item = await this.nordicUartControlButtonViewModel.TalkTo(UserInput.Text.Trim());
			this.UserInput.Text = string.Empty;
			if (item != null)
				DialogUpdate(item);
		}

		public AppBarButton speakButton { get; set; }

		private void PivotPageChanged(object sender, SelectionChangedEventArgs e)
		{
			var pivot = sender as Pivot;
			if(pivot.SelectedIndex == 1)
			{
				this.LogFileHeader.Foreground = new SolidColorBrush(Colors.Black);
				this.ButtonControlPanelHeader.Foreground = new SolidColorBrush(Colors.LightGray);
				this.speakButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			}
			if (pivot.SelectedIndex == 0)
			{
				this.LogFileHeader.Foreground = new SolidColorBrush(Colors.LightGray);
				this.ButtonControlPanelHeader.Foreground = new SolidColorBrush(Colors.Black);
				this.speakButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
		}

		public AppBarButton clearButton { get; set; }
	}
}
