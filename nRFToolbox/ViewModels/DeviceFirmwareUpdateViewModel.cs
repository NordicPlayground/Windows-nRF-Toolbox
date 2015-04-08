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
using Common.Service.DFUPackage;
using Common.Service.GattService;
using nRFToolbox.Common;
using nRFToolbox.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;


namespace nRFToolbox.ViewModels
{
	public class DeviceFirmwareUpdateViewModel : ViewModelBase
	{
		private string pageId = ToolboxIdentifications.PageId.DEVICE_FIRMWARE_UPDATE;
		public string PageId 
		{
			get 
			{
				return pageId;
			}
		}

		private string selectedDeviceFirmwareTypeName;
		public string SelectedDeviceFirmwareTypeName
		{
			get
			{
				return selectedDeviceFirmwareTypeName;
			}
			set
			{
				if (this.selectedDeviceFirmwareTypeName != value)
				{
					this.selectedDeviceFirmwareTypeName = value;
					this.OnPropertyChanged("selectedDeviceFirmwareTypeName");
				}
			}
		}

		private string imageFileNames;
		public string ImageFileNames
		{
			get
			{
				return imageFileNames;
			}
			set
			{
				if (this.imageFileNames != value)
				{
					this.imageFileNames = value;
					this.OnPropertyChanged("ImageFileNames");
				}
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

		private string status;
		public string Status
		{
			get
			{
				return status;
			}
			set
			{
				if (this.status != value)
				{
					this.status = value;
					this.OnPropertyChanged("Status");
				}
			}
		}

		private int progressPercent = 0;
		public int ProgressPercent
		{
			get
			{
				return progressPercent;
			}
			set
			{
				if (this.progressPercent != value)
				{
					this.progressPercent = value;
					this.OnPropertyChanged("ProgressPercent");
				}
			}
		}

		internal async void LoadDFUSettings() 
		{
			dfuSettingViewModel = SettingPivotViewModel.GetInstance().GetDeviceFirmwareUpdateSettingPageViewModel();
			this.SelectedDeviceFirmwareTypeName = dfuSettingViewModel.SelectedDeviceFirmwareTypeName == null ? "Image type:" : dfuSettingViewModel.SelectedDeviceFirmwareTypeName;
			foreach (var token in dfuSettingViewModel.FileToken.Values) 
			{
				if (StorageApplicationPermissions.FutureAccessList.ContainsItem(token)) 
				{
					var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
					this.ChosenFiles.Add(file);
				}
			}
			this.ImageFileNames = dfuSettingViewModel.GetShortFileName();

			if(!IsImagesReadyToSend())
			{
				await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.FILES_NOT_CHOSEN);
			}
			else 
			{
				await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.READY);
			}
		}

		public DeviceFirmwareUpdateViewModel() 
		{
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			deviceFirmwareUpdateService = GattServiceManager.GetGATTServiceManager().GetDeviceFirmwareUpdateService();
			deviceFirmwareUpdateService.ServiceChanged += deviceFirmwareUpdateService_ServiceChanged;
			deviceFirmwareUpdateService.DeviceFirmwareUpdateComplete += deviceFirmwareUpdateService_DeviceFirmwareUpdateComplete;
			deviceFirmwareUpdateService.PacketReceiptConfirmed += deviceFirmwareUpdateService_PacketReceiptConfirmed;
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			LoadDFUSettings();
		}

		async void deviceFirmwareUpdateService_PacketReceiptConfirmed(int sizeOfBytesSent, int totalFirmwareLength, string messageType, string messageData)
		{
			if(messageType != string.Empty)
			{
				await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.DFU_ERROR, 0, messageType, messageData);
				return;
			}
			_totalFirmwareLength = totalFirmwareLength;
		   var percentage = GattServiceHelper.GetPercentage(sizeOfBytesSent, totalFirmwareLength);
			await UpdatePogressBar(percentage);
			await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.SENDING, percentSent: percentage);
		}

		async void deviceFirmwareUpdateService_DeviceFirmwareUpdateComplete(bool IsComplete)
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				await ShowTaskCompleteMessage();
			});
			await UpdatePogressBar(100);
			await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.SENDING_COMPLETE);
		}

		async void deviceFirmwareUpdateService_ServiceChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			IsServiceChanged = true;
			await UpdateAvailableDevice();
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

		public bool IsImagesReadyToSend() 
		{
			bool ready = false;
			if (ChosenFiles.Count == 2)
			{
				var flag1 = ChosenFiles.Exists(x => x.FileType == DeviceFirmwareUpdateSettingPageViewModel.ImageFile_Bin);
				var flag2 = ChosenFiles.Exists(x => x.FileType == DeviceFirmwareUpdateSettingPageViewModel.DataFile_dat);
				if(flag1 && flag2)
					ready = true;
			}
			else
			{
				ready = false;
			}
			return ready;
		}

		private string FILES_NOT_CHOSEN = "Select files";
		private string DEVICE_NOT_CONNECTED = "Please connect your device";
		private string SERVICES_NOT_AVAILABLE = "Try to re-pair the device";
		private string START_DFU = "Waiting for updating...";
		private string READY = "Ready for an update";
		private string SENDING = "Updated";
		private string SENDING_COMPLETE = "Updated complete!";
		private string DFU_TIMEOUT = "Update time out";
		private string DFU_ERROR = "Update error";

		public async Task<bool> UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum status, int percentSent = 0, string errorType = "none", string errorCode = "none") 
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				switch (status)
				{
					case DeviceFirmwareUpdateStatusEnum.FILES_NOT_CHOSEN:
						this.Status = FILES_NOT_CHOSEN;
						break;
					case DeviceFirmwareUpdateStatusEnum.DEVICE_NOT_CONNECTED:
						this.Status = DEVICE_NOT_CONNECTED;
						break;
					case DeviceFirmwareUpdateStatusEnum.SERVICES_NOT_AVAILABLE:
						this.Status = SERVICES_NOT_AVAILABLE;
						break;
					case DeviceFirmwareUpdateStatusEnum.READY:
						this.Status = READY;
						break;
					case DeviceFirmwareUpdateStatusEnum.START_DFU:
						this.Status = START_DFU;
						break;
					case DeviceFirmwareUpdateStatusEnum.SENDING:
						SendingPackStatus(SENDING, percentSent);
						break;
					case DeviceFirmwareUpdateStatusEnum.SENDING_COMPLETE:
						this.Status = SENDING_COMPLETE;
						break;
					case DeviceFirmwareUpdateStatusEnum.DFU_TIMEOUT:
						this.Status = DFU_TIMEOUT;
						break;
					case DeviceFirmwareUpdateStatusEnum.DFU_ERROR:
						SendingErrors(DFU_ERROR, errorType, errorCode);
						break;
				}
			});
			return true;
		}

		private void SendingPackStatus(string status, int percentSent)
		{
			this.Status = status + ": " + percentSent.ToString() + "%";
		}

		private void SendingErrors(string status, string errorType, string errorCode)
		{
			this.Status = status + ": " + errorType + "(" + errorCode + ")";
		}

		public async Task<bool> StartDeviceFirmwareUpdate(DeviceInformationItem targetDevice) 
		{
				GattDeviceService genericAttribute;
				GattDeviceService deviceFirmwareUpdate;
				targetDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.GENERIC_ATTRIBUTES, out genericAttribute);
				targetDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.DEVICE_FIRMWARE_UPDATE, out deviceFirmwareUpdate);
				if (genericAttribute != null && deviceFirmwareUpdate != null)
				{
					try
					{
						var type = DFUPackageHandler.GetFirmwareType(SelectedDeviceFirmwareTypeName);
						if (type.Equals(FirmwareTypeEnum.MultiFiles))
						{
							var imageFile = ChosenFiles.Find(x => x.Name == dfuSettingViewModel.manifestObject.manifest.softdevice_bootloader.bin_file);
							var dataFile = ChosenFiles.Find(x => x.Name == dfuSettingViewModel.manifestObject.manifest.softdevice_bootloader.dat_file);
							var sdSize = dfuSettingViewModel.manifestObject.manifest.softdevice_bootloader.sd_size;
							var blSize = dfuSettingViewModel.manifestObject.manifest.softdevice_bootloader.bl_size;
							await deviceFirmwareUpdateService.Start(deviceFirmwareUpdate, genericAttribute, type, dataFile, imageFile, sdSize, blSize);
						}
						else
						{
							var dat = ChosenFiles.Find(x => x.FileType == DeviceFirmwareUpdateSettingPageViewModel.DataFile_dat);
							var img = ChosenFiles.Find(x => x.FileType == DeviceFirmwareUpdateSettingPageViewModel.ImageFile_Bin);
							await deviceFirmwareUpdateService.Start(deviceFirmwareUpdate, genericAttribute, type, dat, img);
						}
						await deviceFirmwareUpdateService.EnableServiceChange();
						await deviceFirmwareUpdateService.SwitchOnBootLoader();
					}
					catch (Exception e)
					{
						this.exception = e;
					}
					if(this.exception != null)
					{
						await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.DFU_ERROR, 0, this.exception.GetType().ToString(), this.exception.Message);
						return false;
					}
					else 
					{
						RegisterTimer();
						return true;
					}
				}
				else
				{
					await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.SERVICES_NOT_AVAILABLE);
					return false;
				}

		}

		private void RegisterTimer() 
		{
			timer = new SmartDispatcherTimer();
			timer.IsReentrant = false;
			timer.Interval = TimeSpan.FromSeconds(13);
			timer.TickTask = async () =>
			{
				await CheckServiceChanged();
			};
			timer.Start();
		}

		private async Task CheckServiceChanged()
		{
			timer.Stop();
			if (!IsServiceChanged) 
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
				{
					await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.DFU_TIMEOUT);
				});
			}
		}

		public void StopDFUService() 
		{
			deviceFirmwareUpdateService.Stop();
		}

		public async void ClearStatus()
		{
			await UpdateDFUStatus(DeviceFirmwareUpdateStatusEnum.READY);
			await UpdatePogressBar(0);
		}

		public async Task UpdatePogressBar(int percent) 
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.ProgressPercent = percent;
			});
		}

		public async Task<bool> UpdateAvailableDevice() 
		{
			try
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					DeviceSelectionViewModel.ClearCachedDevices();
				});
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
							ShowBluetoothOffErrorMessage();
						});
					}
				}
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		#region popup messageType
		public string BluetoothIsOffMessageTitle = "Can't scan devices";
		public string BluetoothIsOffMessageContent = "Bluetooth is turned off";
		public string NoImageMessageTitle = "No imageFile selected";
		public string NoImageMessageContent = "Select a imageFile to send.";
		public string DFUSendingCompleteTitle = " " + "has been updated successfully!"; //Select device name should be add in front.
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
			//	await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-typeAndLocation:"));
			//});
		}
		public void ShowBluetoothOffErrorMessage()
		{
			var alternative1 = new UICommand("Go to settings", new UICommandInvokedHandler(GoToBluetoothSettingPage), 0);
			var alternative2 = new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1);
			ShowMessage(BluetoothIsOffMessageTitle, BluetoothIsOffMessageContent, alternative1, alternative2);
		}

		public void ShowSettingErrorMessage()
		{
			var alternative1 = new UICommand("Go to settings", new UICommandInvokedHandler(GoToApplicationSettingPage), 0);
			var alternative2 = new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1);
			ShowMessage(NoImageMessageTitle, NoImageMessageContent, alternative1, alternative2);
		}

		public async Task ShowTaskCompleteMessage() 
		{
			var alternative1 = new UICommand("Close", new UICommandInvokedHandler(TaskCompleteMessage), 0);
			var messageDialog = new MessageDialog(DeviceName + DFUSendingCompleteTitle);
			messageDialog.Commands.Add(alternative1);
			messageDialog.CancelCommandIndex = 0;
			await messageDialog.ShowAsync();
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

		private async void GoToApplicationSettingPage(IUICommand command) 
		{
			await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
			var rootFrame = Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
			rootFrame.Navigate(typeof(Settings), this.PageId);
			});
		}

		#endregion
		public IDeviceFirmwareUpdateService deviceFirmwareUpdateService { get; set; }
		public ToolboxDeviceSelectionViewModel DeviceSelectionViewModel { get; set; }
		private CoreDispatcher dispatcher { get; set; }

		private List<StorageFile> ChosenFiles = new List<StorageFile>();

		private int _totalFirmwareLength { get; set; }

		private Stopwatch stopWatch { get; set; }

		private SmartDispatcherTimer timer { get; set; }
		private bool IsServiceChanged = false;

		public DeviceFirmwareUpdateSettingPageViewModel dfuSettingViewModel { get; set; }

		private Exception exception = null;
	}
}
