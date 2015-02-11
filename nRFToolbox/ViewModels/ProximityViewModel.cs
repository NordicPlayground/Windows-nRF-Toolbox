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
using nRFToolbox.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using BackgroundExecutiondManager = Windows.ApplicationModel.Background.BackgroundExecutionManager;
using nRFToolbox.Base;
using nRFToolbox.Service.GattService;
using nRFToolbox.DataModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;


namespace nRFToolbox.ViewModels
{
	public class ProximityViewModel : ViewModelBase
	{
		private string batteryLevel = DEFAULT_BATTERY;
		public string BatteryLevel
		{
			get 
			{ 
				return batteryLevel; 
			}
			set 
			{
				if (this.batteryLevel != value)
				{
					this.batteryLevel = value;
					this.OnPropertyChanged("BatteryLevel");
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

		public string PageId = ToolboxIdentifications.PageId.PROXIMITY;

		public ILinkLossService LinkLossService { get; private set; }
		public IBatteryService BatteryService { get; private set; }
		public IImmediateAlertService ImmediateAlertService { get; private set; }
		public ProximityViewModel() 
		{
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			Dispatcher = CoreApplication.GetCurrentView().Dispatcher;
			GetProximityMonitorServices();
		}

		private static ProximityViewModel proximityViewModelInstance = null;
		public static ProximityViewModel GetInstance() 
		{
			if (proximityViewModelInstance != null)
			{
				return proximityViewModelInstance;
			}
			else 
			{
				proximityViewModelInstance = new ProximityViewModel();
				return proximityViewModelInstance;
			}
		}


		public async void StartServicesOnDevice(DeviceInformationItem chosenBLEDevice) 
		{
			var linkLossServiceOnDevice = chosenBLEDevice.Device.GetGattService(GattServiceUuids.LinkLoss);
			var batteryServiceOnDevice = chosenBLEDevice.Device.GetGattService(GattServiceUuids.Battery);
			var ImmediateAlertServiceOnDevice = chosenBLEDevice.Device.GetGattService(GattServiceUuids.ImmediateAlert);
			await BatteryService.Start(batteryServiceOnDevice);
			await LinkLossService.Start(linkLossServiceOnDevice);
			LinkLossService.WriteAlertLevelCharacteristicAsync(AlertLevelEnum.HighAlert);
			await ImmediateAlertService.Start(ImmediateAlertServiceOnDevice);
			BatteryService.ValueChangeCompleted += BatteryService_ValueChangeCompleted;
		}

		public async Task<bool> TryRetrieveServices()
		{
			bool retrieveComplete = false;
			var cachedTask = await HasExistingLinkLossTask();
			if(cachedTask != null)
			{
				var gattDeviceService = RetrieveLinkLossService(cachedTask);
				if (gattDeviceService != null) 
				{
					RetrieveBatteryService(gattDeviceService.Device.GetGattService(GattServiceUuids.Battery));
					RetrieveImmediateAlertService(gattDeviceService.Device.GetGattService(GattServiceUuids.ImmediateAlert));
					return retrieveComplete = true;
				}
			}
			return retrieveComplete;
		}
		private void GetProximityMonitorServices()
		{
			List<IGattService> result = GattServiceManager.GetGATTServiceManager().GetServicesForProximityMonitor();
			foreach(var item in result)
			{
				switch (item.Name)
				{
					case ToolboxIdentifications.GattServiceNames.LINK_LOSS:
						LinkLossService = item as ILinkLossService;
						break;
					case ToolboxIdentifications.GattServiceNames.BATTERY:
						BatteryService = item as IBatteryService;
						break;
					case ToolboxIdentifications.GattServiceNames.IMMEDIATE_ALERT:
						ImmediateAlertService = item as IImmediateAlertService;
						break;
					default:
						break;
				}
			}
		}
		private async Task<BackgroundTaskRegistration> HasExistingLinkLossTask()
		{
			BackgroundTaskRegistration existingLinkLossTask = null;
			if (await BackgroundExecutiondManager.RequestAccessAsync() == BackgroundAccessStatus.Denied)
			{
			}
			foreach (BackgroundTaskRegistration task in BackgroundTaskRegistration.AllTasks.Values)
			{
				if (task.Name.Split(',')[0].Equals(ToolboxIdentifications.PageId.PROXIMITY))
				{
					return existingLinkLossTask = task;
				}
			}
			return existingLinkLossTask;
		}

		/// <summary>
		/// If there is any uncached task in service, we register new one.
		/// </summary>
		/// <param name="e">Existing Link Loss Task 
		/// This is task is only registered through Proximity Monitor</param>
		private GattDeviceService RetrieveLinkLossService(BackgroundTaskRegistration existingLinkLossTask)
		{
			GattDeviceService gattService = null;
			KeyValuePair<GattDeviceService, BackgroundTaskRegistration> cachedTask;
			if (LinkLossService.LinkLossServiceTaskRegistrations.TryGetValue(existingLinkLossTask.Name, out cachedTask))
			{
				LinkLossService.WriteAlertLevelCharacteristicAsync(AlertLevelEnum.HighAlert);
				this.DeviceName = cachedTask.Key.Device.Name;
				gattService = cachedTask.Key;
			}
			else
			{
				existingLinkLossTask.Unregister(true);
				DeviceName = DEFAULT_DEVICE;
			}
			return gattService;
		}

		private void RetrieveImmediateAlertService(GattDeviceService immediateAlertService)
		{
			ImmediateAlertService.Start(immediateAlertService);
		}

		private async void RetrieveBatteryService(GattDeviceService gattDeviceService) 
		{
			await BatteryService.Start(gattDeviceService);
			BatteryService.ValueChangeCompleted += BatteryService_ValueChangeCompleted;
		}

		async void BatteryService_ValueChangeCompleted(BatteryLevelCharacteristics batteryLevel)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				BatteryLevel = batteryLevel.BatteryLevel + "%";
			});
		}

		public bool SetImmediateAlertOn() 
		{
			bool result = false;
			if (ImmediateAlertService.IsServiceStarted)
			{
				this.ImmediateAlertService.WriteAlertLevel(AlertLevelEnum.HighAlert);
				return result = true;
			}
			return result;
		}

		public bool SetImmediateAlertOff() 
		{
			bool result = false;
			if (ImmediateAlertService.IsServiceStarted)
			{
				this.ImmediateAlertService.WriteAlertLevel(AlertLevelEnum.NoAlert);
				return result = true;
			}
			return result;
		}

		public async Task<bool> UpdateAvailableDevice()
		{
			try 
			{
				DeviceSelectionViewModel.ClearCachedDevices();
				var result = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.LinkLoss));
				if (result.Count > 0)
				{
					foreach (var device in result)
					{
						var ble = await BluetoothLEDevice.FromIdAsync(device.Id) as BluetoothLEDevice;
						DeviceSelectionViewModel.AddBLEDevice(ble);
					}
				}
				else
				{
					if (!await DeviceSelectionViewModel.IsBluetoothSettingOn())
					{
						ShowErrorMessage();
					}
				}
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		public void StopServices() 
		{
			this.BatteryService.ValueChangeCompleted -= BatteryService_ValueChangeCompleted;
			this.BatteryLevel = DEFAULT_BATTERY;
			BatteryService.Stop();
			LinkLossService.Stop();
			ImmediateAlertService.Stop();
		}

		#region popup message

		public string BluetoothIsOffMessageTitle = "Can't scan devices";
		public string BluetoothIsOffMessageContent = "Bluetooth setting is off";
		public void ShowErrorMessage()
		{
			var alternative1 = new UICommand("Go to settings", new UICommandInvokedHandler(GoToBluetoothSettingPage), 0);
			var alternative2 = new UICommand("Close", new UICommandInvokedHandler(CloseBluetoothIsOffMessage), 1);
			ShowMessage(BluetoothIsOffMessageTitle, BluetoothIsOffMessageContent, alternative1, alternative2);
		}
		public override void ShowMessage(string title, string content, UICommand alternative1, UICommand alternative2)
		{
			base.ShowMessage(title, content, alternative1, alternative2);
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

		public ToolboxDeviceSelectionViewModel DeviceSelectionViewModel { get; set; }

		public CoreDispatcher Dispatcher { get; set; }
	}
}
