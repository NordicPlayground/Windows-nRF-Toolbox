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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace nRFToolbox.ViewModels
{
	public class BloodPressureMonitorViewModel : ViewModelBase
	{
		private static BloodPressureMonitorViewModel managerInstance = null;
		public static BloodPressureMonitorViewModel GetBloodPressureMonitorViewModel()
		{
			if (managerInstance != null)
				return managerInstance;
			else
			{
				managerInstance = new BloodPressureMonitorViewModel();
				return managerInstance;
			}
		}

		public BloodPressureMonitorViewModel() 
		{
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			GetService();
		}

		private string _systolic = DEFAULT_VALUE;
		public string Systolic
		{
			get
			{
				return _systolic;
			}
			set
			{
				_systolic = value;
				OnPropertyChanged("Systolic");
			}
		}

		private string _diastolic = DEFAULT_VALUE;
		public string Diastolic 
		{
			get
			{
				return _diastolic;
			}
			set
			{
				_diastolic = value;
				OnPropertyChanged("Diastolic");
			}
		}

		private string _meanArterialPressure = DEFAULT_VALUE;
		public string MeanArterialPressure
		{
			get
			{
				return _meanArterialPressure;
			}
			set
			{
				_meanArterialPressure = value;
				OnPropertyChanged("MeanArterialPressure");
			}
		}

		private string _pulse = DEFAULT_VALUE;
		public string Pulse
		{
			get
			{
				return _pulse;
			}
			set
			{
				_pulse = value;
				OnPropertyChanged("Pulse");
			}
		}

		private string _batteryLevel = DEFAULT_NOT_AVAILABLE;
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

		private string _bloodPressureUnit;
		public string BloodPressureUnit
		{
			get
			{
				return _bloodPressureUnit;
			}
			set
			{
				_bloodPressureUnit = value;
				OnPropertyChanged("BloodPressureUnit");
			}
		}

		private string _pulseUnit;
		public string PulseUnit
		{
			get
			{
				return _pulseUnit;
			}
			set
			{
				_pulseUnit = value;
				OnPropertyChanged("PulseUnit");
			}
		}


		private string _timeStamp = DEFAULT_NOT_AVAILABLE;
		public string TimeStamp
		{
			get
			{
				return _timeStamp;
			}
			set
			{
				_timeStamp = value;
				OnPropertyChanged("TimeStamp");
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

		private void GetService()
		{
			bloodPressureMonitorServices = GattServiceManager.GetGATTServiceManager().GetServicesForBloodPressureMonitor();
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			foreach(var service in bloodPressureMonitorServices)
			{
				switch(service.Name)
				{
					case ToolboxIdentifications.GattServiceNames.BLOOD_PRESSURE:
						this.BloodPressureServiceHandler = service as IBloodPressureService;
						RegisterBloodPressureMeasurementListener(service);
						break;
					case ToolboxIdentifications.GattServiceNames.BATTERY:
						this.BatteryServiceHandler = service as IBatteryService;
						RegisterBatteryLevelListener(service);
						break;
					default:
						break;
				}
			}
		}

		private void RegisterBatteryLevelListener(global::Common.Service.GattService.IGattService service)
		{
			this.BatteryServiceHandler.ValueChangeCompleted += BatteryServiceHandler_ValueChangeCompleted;
		}

		private async void BatteryServiceHandler_ValueChangeCompleted(BatteryLevelCharacteristics batteryLevel)
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.BatteryLevel = batteryLevel.BatteryLevel + "%";
			});
		}

		private void RegisterBloodPressureMeasurementListener(global::Common.Service.GattService.IGattService service)
		{
			this.BloodPressureServiceHandler.MeasurementNotification += BloodPressureServiceHandler_MeasurementNotification;
		}

		async void BloodPressureServiceHandler_MeasurementNotification(BloodPressureMeasurementValue measurement)
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
			{
				this.Systolic = measurement.Systolic.ToString("F1", CultureInfo.CurrentCulture);
				this.Diastolic = measurement.Diastolic.ToString("F1", CultureInfo.CurrentCulture);
				this.MeanArterialPressure = measurement.MeanArterialPressure.ToString("F1", CultureInfo.CurrentCulture);
				if (measurement.PulseRate != 0)
					this.Pulse = measurement.PulseRate.ToString();
				else
					this.Pulse = DEFAULT_VALUE;
					this.BloodPressureUnit = measurement.Unit;
				this.PulseUnit = "bmp";
				if (measurement.TimeStanp.HasValue)
					this.TimeStamp = measurement.TimeStanp.Value.ToString("d. MMM yyyy hh:mm:ss", CultureInfo.CurrentCulture);
				else
					this.TimeStamp = DEFAULT_NOT_AVAILABLE;
			});
		}

		public async Task<bool> UpdateAvailableDevice()
		{
			DeviceSelectionViewModel.ClearCachedDevices();
			try
			{
				DeviceSelectionViewModel.ClearCachedDevices();
				var result = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.BloodPressure));
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
						//ShowErrorMessage();
					}
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task StopService() 
		{
			await this.BloodPressureServiceHandler.Stop();
			await this.BatteryServiceHandler.Stop();
			IsServiceStarted = false;
		}

		public async Task<bool> Start()
		{
			if (this.chosenDevice == null)
				return IsServiceStarted = false;
			GattDeviceService bloodPressureService;
			GattDeviceService batteryService;
			if (chosenDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.BLOOD_PRESSURE, out bloodPressureService))
				IsServiceStarted = await this.BloodPressureServiceHandler.Start(bloodPressureService);
			if (chosenDevice.Services.TryGetValue(ToolboxIdentifications.GattServiceNames.BATTERY, out batteryService))
				IsServiceStarted = await this.BatteryServiceHandler.Start(batteryService);
			return IsServiceStarted = true;
		}

		public async Task EnableBloodPressureMeasurementIndication()
		{
			await this.BloodPressureServiceHandler.EnableBloodPressureMeasurementIndication();
			await this.BloodPressureServiceHandler.GetBloodPressureFeature();
		}

		public DeviceInformationItem chosenDevice { get; set; }
		public bool IsServiceStarted { get; set; }
		public ToolboxDeviceSelectionViewModel DeviceSelectionViewModel { get; set; }
		public CoreDispatcher Dispatcher { get; set; }

		public object PageId
		{
			get 
			{
				return ToolboxIdentifications.PageId.BLOOD_PRESSURE;
			}
		}

		public CoreDispatcher dispatcher { get; set; }

		public List<IGattService> bloodPressureMonitorServices { get; set; }

		public IBloodPressureService BloodPressureServiceHandler { get; set; }

		public IBatteryService BatteryServiceHandler { get; set; }

		internal void ClearData()
		{
			this.Systolic = DEFAULT_VALUE;
			this.Diastolic = DEFAULT_VALUE;
			this.MeanArterialPressure = DEFAULT_VALUE;
			this.Pulse = DEFAULT_VALUE;
			this.TimeStamp = DEFAULT_NOT_AVAILABLE;
			this.BatteryLevel = DEFAULT_NOT_AVAILABLE;
		}
	}
}
