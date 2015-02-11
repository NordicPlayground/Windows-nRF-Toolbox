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
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using System.Collections.ObjectModel;
using nRFToolbox.DataModel;
using Windows.Foundation;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.System;
using Windows.ApplicationModel.Background;
using nRFToolbox.Base;
using nRFToolbox.Service.GattService;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using nRFToolbox.Service;

namespace nRFToolbox.ViewModels
{
	public class HeartRateMonitorViewModel : ViewModelBase
	{
		public string PageId = ToolboxIdentifications.PageId.HEART_RATE;
		public string DeviceAddress { get; set; }

		private string _bodySensorLocation;
		public string BodySensorLocation 
		{
			get 
			{
				return _bodySensorLocation;
			}
			set 
			{
				_bodySensorLocation = value;
				OnPropertyChanged("BodySensorLocation");
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

		private string _batteryLevel;
		public string BatteryLevel
		{
			get
			{
				return _batteryLevel;
			}
			set
			{
				_batteryLevel = value;
				OnPropertyChanged("BatteryLevel");
			}
		}

		private string _beatPerMinute;
		public string BeatPerMinute 
		{ 
			get
			{
				return _beatPerMinute;
			}
			set 
			{
				_beatPerMinute = value;
				OnPropertyChanged("BeatPerMinute");
			}
		}

		private DateTimeOffset _timeStamp;
		public DateTimeOffset TimeStamp 
		{
			get 
			{
				return _timeStamp;
			}
			set
			{
				if (_timeStamp.Equals(value))
				{
					_timeStamp = value;
					OnPropertyChanged("ReceivedTime");
				}
			}
		}

		private BluetoothConnectionStatus _connectionStatus;
		public BluetoothConnectionStatus ConnectionStatus
		{
			get
			{
				return _connectionStatus;
			}
			set
			{
				if (_connectionStatus != value)
				{
					this._connectionStatus = value;
					OnPropertyChanged("ConnectionStatus");
				}
			}
		}

		private ObservableCollection<BeatPerMinuteLineChart> heartRatio = new ObservableCollection<BeatPerMinuteLineChart>() { new BeatPerMinuteLineChart(0, 0) };
		public ObservableCollection<BeatPerMinuteLineChart> HeartRatio
		{
			get
			{
				return heartRatio;
			}
		}

		private int count = 1;
		public void AddBMPValue(ushort beatPerMinute) 
		{
		     heartRatio.Add(new BeatPerMinuteLineChart(count+=1, beatPerMinute));
		}

		public void ClearHeartRatio() 
		{
			this.heartRatio.Clear();
		}

		public void AddBMPRange(List<BeatPerMinuteLineChart> list) 
		{
			foreach (var item in list)
			{
				heartRatio.Add(new BeatPerMinuteLineChart(count += 1, item.Beat));
			}
		}

		public HeartRateMonitorViewModel() 
		{
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			RegisterHeartRateService();
		}
		void RegisterHeartRateService()
		{
			heartRateService = GattServiceManager.GetGATTServiceManager().GetHeartRateService();
			batteryService = GattServiceManager.GetGATTServiceManager().GetBatteryService();
			heartRateService.HeartRateValueChangeCompleted += heartRate_ValueChangeCompleted;
			heartRateService.SensorLocation_Detected += sensorLocation_detected;
			batteryService.ValueChangeCompleted += battery_ValueChangeCompleted;
		}

		public void StartHeartRateMonitor(DeviceInformationItem heartRateDevice) 
		{
			GattDeviceService hrService;
			GattDeviceService baService;
			if (heartRateDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.HEART_RATE, out hrService))
				heartRateService.Start(hrService);
			if (heartRateDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.BATTERY, out baService))
				batteryService.Start(baService);
		}

		private async void heartRate_ValueChangeCompleted(HeartRateMeasurementCharacteristic heartRateMeasurementValue)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.BeatPerMinute = heartRateMeasurementValue.BeatPerMinute.ToString();
			});
		}

		private async void sensorLocation_detected(string detectedLocation)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.BodySensorLocation = detectedLocation;
			});
		}

		private async void battery_ValueChangeCompleted(BatteryLevelCharacteristics batteryLevel)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.BatteryLevel = batteryLevel.BatteryLevel + "%";
			});
		}

		public async Task<bool> UpdateAvailableDevice()
		{
			try 
			{
				DeviceSelectionViewModel.ClearCachedDevices();
				var result = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.HeartRate));
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
			this.heartRateService.Stop();
			this.batteryService.Stop();
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

		public Service.GattService.IHeartRateService heartRateService { get; set; }

		public Service.GattService.IBatteryService batteryService { get; set; }

		private CoreDispatcher Dispatcher { get; set; }
	}
}
