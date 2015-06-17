using Common.Service;
using Common.Service.GattService;
using nRFToolbox.Base;
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
	public class NordicUARTViewModel : ViewModelBase
	{
		public delegate void EchoMessageReceived(UartDialogViewModel message);
		public event EchoMessageReceived InvalidateDialog;
		private static NordicUARTViewModel managerInstance = null;
		public static NordicUARTViewModel GetNordicUartViewModel()
		{
			if (managerInstance != null)
				return managerInstance;
			else
			{
				managerInstance = new NordicUARTViewModel();
				return managerInstance;
			}
		}
 
		public NordicUARTViewModel() 
		{
			uartService = GattServiceManager.GetGATTServiceManager().GetServiceForNordicUart().FirstOrDefault() as UARTService;
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			SetWelcomeMessage();
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

		private string echo;
		public string Echo
		{
			get
			{
				return echo;
			}
			set
			{
				if (this.echo != value)
				{
					this.echo = value;
					this.OnPropertyChanged("Echo");
				}
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

		private ObservableCollection<UartDialogViewModel> dialog = new ObservableCollection<UartDialogViewModel>();
		public ObservableCollection<UartDialogViewModel> Dialog
		{
			get
			{
				return this.dialog;
			}
		}

		public DeviceInformationItem chosenDevice { get; set; }

		public void InitializeDevice(DeviceInformationItem chosenUartDevice) 
		{
			if(this.chosenDevice == null || this.chosenDevice.DeviceID != chosenUartDevice.DeviceID)
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
			else if(this.chosenDevice != null || uartService.IsServiceStarted)
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

		public void SetWelcomeMessage() 
		{
			var item = RegisterDialog("Connect your device and you are ready to play!" + Environment.NewLine + PageId, false);
			if (InvalidateDialog != null)
				InvalidateDialog(item);
		}

		public UartDialogViewModel RegisterDialog(string text, bool iSSendByUser) 
		{
			var diag = new UartDialogViewModel(text, iSSendByUser);
			this.Dialog.Add(diag);
			return diag;
		}

		public async Task<UartDialogViewModel> TalkTo(string text) 
		{
			try
			{
				if (IsReadyToTalk && !IsTextExceed(text))
				{
					var item = RegisterDialog(text, true);
					await uartService.SendPeer(text);
					return item;
				}
				return null;
			}
			catch(Exception)
			{
				return null;
			}
		}

		private bool IsTextExceed(string text)
		{
			var textBinary = GattServiceHelper.ToIBuffer(text);
			if(textBinary.Length > MAXIMUN_BYTES)
			{
				var item = RegisterDialog("Try with a shorter text." + Environment.NewLine + PageId, false);
				if (InvalidateDialog != null)
					InvalidateDialog(item);
				return true;
			}
			return false;
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

		public void StopUartService() 
		{
			if(this.InvalidateDialog != null)
				uartService.EchoReceived -= uartService_EchoReceived;
			this.uartService.Stop();
			this.chosenDevice = null;
		}

		public ToolboxDeviceSelectionViewModel DeviceSelectionViewModel { get; set; }

		public CoreDispatcher dispatcher { get; set; }

		public IUARTService uartService { get; set; }

		#region popup messageType
		public string BluetoothIsOffMessageTitle = "Can't scan devices";
		public string BluetoothIsOffMessageContent = "Bluetooth is turned off";
		//public string NoImageMessageTitle = "No image selected";
		//public string NoImageMessageContent = "Select a image to send.";
		//public string DFUSendingCompleteTitle = " " + "has been updated successfully!"; //Select device name should be add in front.
		//public string DFUSendingInfoContent = "FileName and bytes are transfered";
		//public void ShowToastMessage(string content)
		//{
		//	var toastNotifier = ToastNotificationManager.CreateToastNotifier();
		//	var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);
		//	var toastText = toastXml.GetElementsByTagName("text");
		//	(toastText[0] as XmlElement).InnerText = content;
		//	var toast = new ToastNotification(toastXml);
		//	toast.Activated += toast_Activated;
		//	toastNotifier.Show(toast);
		//}

		//async void toast_Activated(ToastNotification sender, object args)
		//{
		//	//  .RunAsync(CoreDispatcherPriority.Normal, async () =>
		//	//{
		//	//	await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
		//	//});
		//}
		public void ShowBluetoothOffErrorMessage()
		{
			var alternative1 = new UICommand("Go to settings", new UICommandInvokedHandler(GoToBluetoothSettingPage), 0);
			var alternative2 = new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1);
			ShowMessage(BluetoothIsOffMessageTitle, BluetoothIsOffMessageContent, alternative1, alternative2);
		}

		//public void ShowSettingErrorMessage()
		//{
		//	var alternative1 = new UICommand("Go to settings", new UICommandInvokedHandler(GoToApplicationSettingPage), 0);
		//	var alternative2 = new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1);
		//	ShowMessage(NoImageMessageTitle, NoImageMessageContent, alternative1, alternative2);
		//}

		//public async Task ShowTaskCompleteMessage()
		//{
		//	var alternative1 = new UICommand("Close", new UICommandInvokedHandler(TaskCompleteMessage), 0);
		//	var messageDialog = new MessageDialog(DeviceName + DFUSendingCompleteTitle);
		//	messageDialog.Commands.Add(alternative1);
		//	messageDialog.CancelCommandIndex = 0;
		//	await messageDialog.ShowAsync();
		//}

		public override void ShowMessage(string title, string content, UICommand alternative1, UICommand alternative2)
		{
			base.ShowMessage(title, content, alternative1, alternative2);
		}

		private void CloseBluetoothIsOffMessage(IUICommand command)
		{ }
		private void TaskCompleteMessage(IUICommand command)
		{ }

		private async void GoToBluetoothSettingPage(IUICommand command)
		{
			await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				await Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
			});
		}

		#endregion

		public bool IsReadyToTalk = false;
		public uint MAXIMUN_BYTES = 20; // Current support

		internal void ClearConversation()
		{
			this.Dialog.Clear();
		}
	}
}
