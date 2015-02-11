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
using nRFToolbox.Base;
using nRFToolbox.DataModel;
using nRFToolbox.Service;
using nRFToolbox.Service.GattService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;


namespace nRFToolbox.ViewModels
{
	public class DeviceFirmwareUpdateViewModel : ViewModelBase
	{
		private FirmwareTypeEnum firmware;
		public FirmwareTypeEnum Firmware 
		{
			get 
			{
				return firmware;
			}
			set 
			{
				if (this.firmware != value)
				{
					this.firmware = value;
					this.OnPropertyChanged("To be done");
				}
			}
		}
		public DeviceFirmwareUpdateViewModel() 
		{
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			deviceFirmwareUpdateService = GattServiceManager.GetGATTServiceManager().GetDeviceFirmwareUpdateService();
			deviceFirmwareUpdateService.ServiceChanged += deviceFirmwareUpdateService_ServiceChanged;
			deviceFirmwareUpdateService.DeviceFirmwareUpdateComplete += deviceFirmwareUpdateService_DeviceFirmwareUpdateComplete;
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
		}

		async void deviceFirmwareUpdateService_DeviceFirmwareUpdateComplete(bool IsComplete)
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				ShowTaskCompleteMessage();
			});
		}

		async void deviceFirmwareUpdateService_ServiceChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			UpdateAvailableDevice();
			DeviceInformationItem newDevice = null;
			GattDeviceService dfuService;
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					newDevice = DeviceSelectionViewModel.GetBLEDevice(sender.Service.Device.DeviceId);
				});
			if(newDevice != null && newDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.DEVICE_FIRMWARE_UPDATE, out dfuService))
			{
				deviceFirmwareUpdateService.StarDfu(dfuService);
			}	
		}
		public void StartDeviceFirmwareUpdate(DeviceInformationItem targetDevice) 
		{
			GattDeviceService genericAttribute;
			GattDeviceService deviceFirmwareUpdate;
			targetDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.GENERIC_ATTRIBUTES, out genericAttribute);
			targetDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.DEVICE_FIRMWARE_UPDATE, out deviceFirmwareUpdate);
			if(genericAttribute != null && deviceFirmwareUpdate != null)
			{
				deviceFirmwareUpdateService.Start(deviceFirmwareUpdate, genericAttribute);
				deviceFirmwareUpdateService.EnableServiceChange();
				deviceFirmwareUpdateService.SwitchOnBootLoader(FirmwareTypeEnum.Application);
			}
		}

		public async void UpdateAvailableDevice() 
		{
			var result = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(ToolboxIdentifications.GattServiceUuids.DeviceFirmwareUpdate));
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
						ShowErrorMessage();
					});
				}
			}
		}

		#region popup message
		public string BluetoothIsOffMessageTitle = "Can't scan devices";
		public string BluetoothIsOffMessageContent = "Bluetooth setting is off";
		public string DFUSendingCompeleteTitle = "Sending Compelete";
		public string DFUSendingInfoContent = "FileName and bytes are transfered";
		public void ShowToastMessage(string content)
		{
			var toastNotifier = ToastNotificationManager.CreateToastNotifier();
			var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);
			var toastText = toastXml.GetElementsByTagName("text");
			(toastText[0] as XmlElement).InnerText = content;
			var toast = new ToastNotification(toastXml);
			toast.Activated += toast_Activated;
			toastNotifier.Show(toast);
		}

		async void toast_Activated(ToastNotification sender, object args)
		{
			//  .RunAsync(CoreDispatcherPriority.Normal, async () =>
			//{
			//	await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
			//});
		}
		public void ShowErrorMessage()
		{
			var alternative1 = new UICommand("Go to settings", new UICommandInvokedHandler(GoToBluetoothSettingPage), 0);
			var alternative2 = new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1);
			ShowMessage(BluetoothIsOffMessageTitle, BluetoothIsOffMessageContent, alternative1, alternative2);
		}
		public void ShowTaskCompleteMessage() 
		{
			var alternative1 = new UICommand("Close", new UICommandInvokedHandler(TaskCompleteMessage), 0);
			var messageDialog = new MessageDialog(DFUSendingCompeleteTitle);
			messageDialog.Commands.Add(alternative1);
			messageDialog.CancelCommandIndex = 0;
			messageDialog.ShowAsync();
		}
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

		public IDeviceFirmwareUpdateService deviceFirmwareUpdateService { get; set; }
		public ToolboxDeviceSelectionViewModel DeviceSelectionViewModel { get; set; }
		private CoreDispatcher dispatcher { get; set; }
	}
}
