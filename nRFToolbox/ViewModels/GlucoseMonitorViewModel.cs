using Common.Service;
using Common.Service.GattService;
using nRFToolbox.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace nRFToolbox.ViewModels
{
	public class GlucoseMonitorViewModel : ViewModelBase
	{
		private static GlucoseMonitorViewModel managerInstance = null;
		public static GlucoseMonitorViewModel GetGlucoseMonitorViewModel()
		{
			if (managerInstance != null)
				return managerInstance;
			else
			{
				managerInstance = new GlucoseMonitorViewModel();
				return managerInstance;
			}
		}

		public GlucoseMonitorViewModel() 
		{
			DeviceSelectionViewModel = new ToolboxDeviceSelectionViewModel();
			Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			GetGlocuseServices();
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

		private string _unit = "mol/l";
		public string Unit
		{
			get
			{
				return _unit;
			}
			set
			{
				_unit = value;
				OnPropertyChanged("BloodPressureUnit");
			}
		}

		private ObservableCollection<GlucoseMeasurementValueWrapper> glucoseMeasurementList = new ObservableCollection<GlucoseMeasurementValueWrapper>();
		public ObservableCollection<GlucoseMeasurementValueWrapper> GlucoseMeasurementList
		{
			get
			{
				return glucoseMeasurementList;
			}
		} 

		public async Task<bool> UpdateAvailableDevice()
		{
			DeviceSelectionViewModel.ClearCachedDevices();
			try
			{
				DeviceSelectionViewModel.ClearCachedDevices();
				var result = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.Glucose));
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

		public void GetGlocuseServices()
		{
			var glucoseMonitorServices = GattServiceManager.GetGATTServiceManager().GetServiceForGlucoseMonitor();
			dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			foreach (var service in glucoseMonitorServices) 
			{
				switch(service.Name)
				{
					case ToolboxIdentifications.GattServiceNames.GLUCOSE:
						this.GlucoseServiceHandler = service as IGlocuseService;
						RegisterGlucoseMeasurementListener(service);
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

		private void RegisterBatteryLevelListener(IGattService service)
		{
			this.BatteryServiceHandler.ValueChangeCompleted += BatteryServiceHandler_ValueChangeCompleted;
		}

		async void BatteryServiceHandler_ValueChangeCompleted(BatteryLevelCharacteristics batteryLevel)
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.BatteryLevel = batteryLevel.BatteryLevel + "%";
			});
		}

		private void RegisterGlucoseMeasurementListener(IGattService service)
		{
			this.GlucoseServiceHandler.MeasurementNotification += GlucoseServiceHandler_MeasurementNotification; ;
		}

		private async void GlucoseServiceHandler_MeasurementNotification(GlucoseMeasurementValue measurement)
		{
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			 {
				 var value = new GlucoseMeasurementValueWrapper(measurement);
				 this.glucoseMeasurementList.Add(value);
			 });
		}

		internal async Task<bool> Start(Dictionary<string, GattDeviceService> services) 
		{
			GattDeviceService glucoseService;
			GattDeviceService batteryService;
			if (services.TryGetValue(ToolboxIdentifications.GattServiceNames.GLUCOSE, out glucoseService))
				IsServiceStarted = await this.GlucoseServiceHandler.Start(glucoseService);
			if (services.TryGetValue(ToolboxIdentifications.GattServiceNames.BATTERY, out batteryService))
				IsServiceStarted = await this.BatteryServiceHandler.Start(batteryService);
			return IsServiceStarted;
		}

		internal async Task GetAllStoredRecords() 
		{
			try 
			{
				if (!IsServiceStarted)
					return;
				await ClearMeasurements();
				await this.GlucoseServiceHandler.GetAllStoredRecord();
			}
			catch(Exception)
			{

			}
		}

		internal async Task DeleteAllStoredRecords()
		{
			try
			{
				if (!IsServiceStarted)
					return;
				await ClearMeasurements();
				await this.GlucoseServiceHandler.DeleteAllStoredRecord();
			}
			catch(Exception e)
			{

			}
		}

		internal async Task GetFirstRecords()
		{
			try 
			{
				if (!IsServiceStarted)
					return;
				await ClearMeasurements();
				await this.GlucoseServiceHandler.GetFirstRecord();
			}
			catch(Exception e)
			{

			}
		}

		internal async Task GetLastRecords()
		{
			try
			{
				if (!IsServiceStarted)
					return;
				await ClearMeasurements();
				await this.GlucoseServiceHandler.GetLastRecord();
			}
			catch(Exception e)
			{

			}
		}

		private async Task ClearMeasurements() 
		{
			try
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					this.glucoseMeasurementList.Clear();
				});
			}
			catch(Exception e)
			{

			}
		}

		public async void StopServices()
		{

			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				if (!IsServiceStarted)
					return;
				this.BatteryLevel = DEFAULT_NOT_AVAILABLE;
				this.DeviceName = DEFAULT_DEVICE;
				this.chosenDevice = null;
				await this.ClearMeasurements();
				await this.BatteryServiceHandler.Stop();
				await this.GlucoseServiceHandler.Stop();
				IsServiceStarted = false;
			});
		}

		public ToolboxDeviceSelectionViewModel DeviceSelectionViewModel { get; set; }

		public CoreDispatcher Dispatcher { get; set; }

		public IGlocuseService GlucoseServiceHandler { get; set; }

		public bool IsServiceStarted = false;

		private CoreDispatcher dispatcher { get; set; }

		public IBatteryService BatteryServiceHandler { get; set; }

		public DeviceInformationItem chosenDevice { get; set; }

		public object PageId 
		{
			get 
			{
				return ToolboxIdentifications.PageId.GLUCOSE;
			}
		}
	}
}
