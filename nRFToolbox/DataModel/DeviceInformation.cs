using Common.Service;
using nRFToolbox.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage;

namespace nRFToolbox.DataModel
{
	public class DeviceInformationItem : DeviceInformationBase
	{
		public BluetoothLEDevice Device { get; set; }
		//public string DeviceAddress { get; set; }
		//public List<Boolean> Flags { get; set; }
		//public string UUID { get; set; }
		//public string CompleteDeviceName { get; set; }
		//public bool AlertOnPhone { get; set; }
		//public bool AlertOnDevice { get; set; }
		//public string DeviceID { get; set; }
		//public Dictionary<string, GattDeviceService> Services = new Dictionary<string, GattDeviceService>();
		private BluetoothConnectionStatus _connectionStatus;
		public BluetoothConnectionStatus ConnectionStatus
		{
			get
			{
				return _connectionStatus;
			}
			set
			{

				this._connectionStatus = value;
			}
		}

		public delegate void BLEDeviceConnectionStatusHandler(bool isConnected);

		private static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

		public DeviceInformationItem(string deviceAddress, List<Boolean> flags, string uUID, string completeName)
			: base()
		{
			this.DeviceAddress = deviceAddress;
			this.Flags = flags;
			this.UUID = uUID;
			this.CompleteDeviceName = completeName;
		}

		public DeviceInformationItem(BluetoothLEDevice device)
			: base()
		{
			this.Device = device;
			this.DeviceAddress = GattServiceHelper.ToStringAddress(device.BluetoothAddress);
			try
			{
				foreach (var service in device.GattServices)
				{
					switch (service.Uuid.ToString())
					{
						case "00001811-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.ALERT_NOTIFICATION, service);
							break;
						case "0000180f-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.BATTERY, service);
							break;
						case "00001810-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.BLOOD_PRESSURE, service);
							break;
						case "00001805-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.CURRENT_TIME, service);
							break;
						case "00001818-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.CYCLING_POWER, service);
							break;
						case "00001816-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.CYCLING_SPEED_AND_CADENCE, service);
							break;
						case "0000180a-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.DEVICE_INFORMATION, service);
							break;
						case "00001800-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.GENERIC_ACCESS, service);
							break;
						case "00001801-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.GENERIC_ATTRIBUTES, service);
							break;
						case "00001808-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.GLUCOSE, service);
							break;
						case "00001809-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.HEALTH_THERMOMETER, service);
							break;
						case "0000180d-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.HEART_RATE, service);
							break;
						case "00001812-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.HUMAN_INTERFACE_DEVICE, service);
							break;
						case "00001802-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.IMMEDIATE_ALERT, service);
							break;
						case "00001803-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.LINK_LOSS, service);
							break;
						case "00001819-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.LOCATION_AND_NAVIGATION, service);
							break;
						case "00001807-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.NEXT_DST_CHANGE, service);
							break;
						case "0000180e-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.PHONE_ALERT_STATUS, service);
							break;
						case "00001806-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.REFERENCE_TIME_UPDATE, service);
							break;
						case "00001814-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.RUNNING_SPEED_AND_CADENCE, service);
							break;
						case "00001813-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.SCAN_PARAMETERS, service);
							break;
						case "00001804-0000-1000-8000-00805f9b34fb":
							Services.Add(ToolboxIdentifications.GattServiceNames.TX_POWER, service);
							break;
						case "00001530-1212-efde-1523-785feabcd123":
							Services.Add(ToolboxIdentifications.GattServiceNames.DEVICE_FIRMWARE_UPDATE, service);
							break;
						case "6e400001-b5a3-f393-e0a9-e50e24dcca9e":
							Services.Add(ToolboxIdentifications.GattServiceNames.NORDIC_UART, service);
							break;
						default:
							break;
					}

				}
			}
			catch (Exception e)
			{
				//Catch any exceptions and make alert to phone or user
			}
			if (LocalSettings.Values.ContainsKey(DeviceAddress))
			{
				string[] values = ((string)LocalSettings.Values[DeviceAddress]).Split(',');
				AlertOnPhone = bool.Parse(values[0]);
				AlertOnDevice = bool.Parse(values[1]);
				//alertLevel = (AlertLevel)Enum.Parse(typeof(AlertLevel), values[2]);
			}
			this.CompleteDeviceName = device.Name;
			this.DeviceID = device.DeviceId;
			this.ConnectionStatus = device.ConnectionStatus;
		}
	}
}
