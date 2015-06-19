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
using Common.Service;
using Common.Service.GattService;
using nRFToolbox.DataModel;
using nRFToolbox.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace nRFToolbox.ViewModels
{
	public class NordicUartControlButtonViewModel : ViewModelBase
	{
		public ToolboxDeviceSelectionViewModel DeviceSelectionViewModel { get; set; }
		public CoreDispatcher dispatcher { get; set; }
		public DeviceInformationItem chosenDevice { get; set; }
		public IUARTService uartService { get; set; }
		public bool IsOnEditModel { get; set; }
		public ControlButtonViewModel ButtonInPanel { get; set; }
		public string EditedModeColor = "Red";
		public string NonEditModeColor = "#009CDE";

		public bool IsReadyToTalk = false;
		public uint MAXIMUN_BYTES = 20; // Current support
		public delegate void EchoMessageReceived(UartDialogViewModel message);
		public event EchoMessageReceived InvalidateDialog;

		private static NordicUartControlButtonViewModel managerInstance = null;
		public static NordicUartControlButtonViewModel GetNordicUartControlButtonViewModel()
		{
			if (managerInstance != null)
				return managerInstance;
			else
			{
				managerInstance = new NordicUartControlButtonViewModel();
				return managerInstance;
			}
		}
		public NordicUartControlButtonViewModel() 
		{
			uartService = GattServiceManager.GetGATTServiceManager().GetServiceForNordicUart().FirstOrDefault() as UARTService;
			IsOnEditModel = false;
			InitializeSelectedControlButton(IsOnEditModel);
			InitializeAllControlButton();
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
		}

		private GridViewItemCollection<ControlButtonViewModel> allControlButtons = new GridViewItemCollection<ControlButtonViewModel>();
		public GridViewItemCollection<ControlButtonViewModel> AllControlButtons
		{
			get
			{
				return this.allControlButtons;
			}
		}

		private GridViewItemCollection<ControlButtonViewModel> selectedControlButton = new GridViewItemCollection<ControlButtonViewModel>();
		public GridViewItemCollection<ControlButtonViewModel> SelectedControlButton
		{
			get
			{
				return this.selectedControlButton;
			}
		}

		public void InitializeSelectedControlButton(bool isOnEditModel) 
		{
			ControlButtonViewModel item;
			Uri _baseUri = new Uri("ms-appx:///Assets/image/ControlButton/");
			string background = null;
			if (isOnEditModel)
				background = EditedModeColor;
			else
				background = NonEditModeColor;

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "1";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "2";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "3";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "4";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "5";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "6";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "7";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "8";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

			item = new ControlButtonViewModel();
			item.Title = string.Empty;
			item.PositionId = "9";
			item.ButtonId = string.Empty;
			item.Background = background;
			item.SetLocalImage(_baseUri, "");
			selectedControlButton.Add(item);

		}

		public void InitializeAllControlButton() 
		{
			ControlButtonViewModel item;
			string background = EditedModeColor;
			Uri _baseUri = new Uri("ms-appx:///Assets/image/ControlButton/");

			item = new ControlButtonViewModel();
			item.Title = "1";
			item.ButtonId = "1";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_1.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "2";
			item.ButtonId = "2";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_2.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "3";
			item.ButtonId = "3";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_3.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "4";
			item.ButtonId = "4";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_4.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "5";
			item.ButtonId = "5";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_5.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "6";
			item.ButtonId = "6";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_6.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "7";
			item.ButtonId = "7";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_7.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "8";
			item.ButtonId = "8";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_8.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "9";
			item.ButtonId = "9";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_9.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "play";
			item.ButtonId = "play";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_play.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "pause";
			item.ButtonId = "pause";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_pause.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "rewind";
			item.ButtonId = "rewind";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_rewind.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "forward";
			item.ButtonId = "forward";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_forward.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "stop";
			item.ButtonId = "stop";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_stop.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "setting";
			item.ButtonId = "setting";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_settings.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "about";
			item.ButtonId = "about";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_about.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "down";
			item.ButtonId = "down";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_down.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "up";
			item.ButtonId = "up";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_up.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "right";
			item.ButtonId = "right";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_right.png");
			allControlButtons.Add(item);

			item = new ControlButtonViewModel();
			item.Title = "left";
			item.ButtonId = "left";
			item.Background = background;
			item.SetLocalImage(_baseUri, "ic_uart_left.png");
			allControlButtons.Add(item);
		}

		public async Task<bool> UpdateAvailableDevice()
		{
			try
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					DeviceSelectionViewModel.ClearCachedDevices();
				});
				var result = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(ToolboxIdentifications.GattServiceUuids.Nordic_UART));
				if (result.Count > 0)
				{
					foreach (var device in result)
					{
						var ble = await BluetoothLEDevice.FromIdAsync(device.Id) as BluetoothLEDevice;
						await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
						{
							DeviceSelectionViewModel.AddBLEDevice(ble);
						});
					}
				}
				else
				{
					if (!await DeviceSelectionViewModel.IsBluetoothSettingOn())
					{
						await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
						{
							ShowBluetoothOffErrorMessage();
						});
					}
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private ObservableCollection<UartDialogViewModel> dialog = new ObservableCollection<UartDialogViewModel>();
		public ObservableCollection<UartDialogViewModel> Dialog
		{
			get
			{
				return this.dialog;
			}
		}

		private string userInput = "Start";
		public string UserInput
		{
			get
			{
				return userInput;
			}
			set
			{
				if (this.userInput != value)
				{
					this.userInput = value;
					this.OnPropertyChanged("UserInput");
				}
			}
		}

		private string pageId = ToolboxIdentifications.PageId.NORDIC_UART;
		public string PageId
		{
			get
			{
				return pageId;
			}
		}

		private string deviceName = DEFAULT_DEVICE;
		public string DeviceName
		{
			get
			{
				return deviceName;
			}
			set
			{
				if (this.deviceName != value)
				{
					this.deviceName = value;
					this.OnPropertyChanged("DeviceName");
				}
			}
		}

		public async Task<UartDialogViewModel> TalkTo(string text)
		{
			try
			{
				if (IsReadyToTalk && !IsTextExceed(text) && chosenDevice != null && chosenDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
				{
					var item = RegisterDialog(text, true);
					await uartService.SendPeer(text);
					return item;
				}
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void InitializeDevice(DeviceInformationItem chosenUartDevice)
		{
			if (this.chosenDevice == null || this.chosenDevice.DeviceID != chosenUartDevice.DeviceID)
			{
				this.chosenDevice = chosenUartDevice;
				GattDeviceService service = null;
				if (chosenDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.NORDIC_UART, out service))
				{
					uartService.Start(service);
					uartService.EchoReceived += uartService_EchoReceived;
					IsReadyToTalk = true;
				}
			}
			else if (this.chosenDevice != null || uartService.IsServiceStarted)
			{
				IsReadyToTalk = true;
			}
			//give error message or success message
		}

		async void uartService_EchoReceived(string echo)
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				var item = RegisterDialog(echo, false);
				if (InvalidateDialog != null)
					InvalidateDialog(item);
			});
		}

		private bool IsTextExceed(string text)
		{
			var textBinary = GattServiceHelper.ToIBuffer(text);
			if (textBinary.Length > MAXIMUN_BYTES)
			{
				var item = RegisterDialog("Try with a shorter text." + Environment.NewLine + PageId, false);
				if (InvalidateDialog != null)
					InvalidateDialog(item);
				return true;
			}
			return false;
		}

		public UartDialogViewModel RegisterDialog(string text, bool iSSendByUser)
		{
			var diag = new UartDialogViewModel(text, iSSendByUser);
			this.Dialog.Add(diag);
			return diag;
		}

		internal void ClearConversation()
		{
			this.Dialog.Clear();
		}

		public void TurnOnOffEditModel(bool onEditModel) 
		{
			if (onEditModel == true)
			{
				ChangeColorTo(EditedModeColor);
				IsOnEditModel = true;
			}else
			{
				ChangeColorTo(NonEditModeColor);
				IsOnEditModel = false;
			}
		}

		public void ChangeColorOnSelectedItem(string Id) 
		{
			if (SelectedAvailableButton != null)
			{
				var result = this.allControlButtons.FirstOrDefault(x => x.ButtonId == SelectedAvailableButton.ButtonId) as ControlButtonViewModel;
				if (result != null)
					result.Background = EditedModeColor;
			}
			var selectedItem = this.allControlButtons.FirstOrDefault(x => x.ButtonId == Id) as ControlButtonViewModel;
			if (selectedItem != null)
				selectedItem.Background = NonEditModeColor;
		}

		private void ChangeColorTo(string rGBCode)
		{
			foreach(var button in this.selectedControlButton)
			{
				button.Background = rGBCode;
			}
		}

		public void StopUartService()
		{
			if (this.InvalidateDialog != null)
				uartService.EchoReceived -= uartService_EchoReceived;
			this.uartService.Stop();
			this.chosenDevice = null;
		}

		public void ClearUserChoice() 
		{
			foreach(var item in this.AllControlButtons)
			{
				item.Background = this.EditedModeColor;
			}
			this.SelectedAvailableButton = null;
		}

		#region ErrorMessage
		public string BluetoothIsOffMessageTitle = "Can't scan devices";
		public string BluetoothIsOffMessageContent = "Bluetooth is turned off";

		public void ShowBluetoothOffErrorMessage()
		{
			var alternative1 = new UICommand("Go to settings", new UICommandInvokedHandler(GoToBluetoothSettingPage), 0);
			var alternative2 = new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1);
			ShowMessage(BluetoothIsOffMessageTitle, BluetoothIsOffMessageContent, alternative1, alternative2);
		}

		private void CloseBluetoothIsOffMessage(IUICommand command)
		{ }

		private async void GoToBluetoothSettingPage(IUICommand command)
		{
			await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				await Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
			});
		}
		#endregion

		public ControlButtonViewModel SelectedAvailableButton { get; set; }

		internal void UpdateSelectedControlButton()
		{
			this.ButtonInPanel.Title = this.SelectedAvailableButton.Title;
			this.ButtonInPanel.Image = this.SelectedAvailableButton.Image;
			this.ButtonInPanel.ButtonId = this.SelectedAvailableButton.ButtonId;
		}

		public void ClearSelectedControlButtons() 
		{
			Uri _baseUri = new Uri("ms-appx:///Assets/image/ControlButton/");
			foreach(var item in this.selectedControlButton)
			{
				item.SetLocalImage(_baseUri, "");
				item.Title = string.Empty;
				item.ButtonId = string.Empty;
			}
		}
	}
}
